Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data.OleDb
Imports System.Security.Cryptography
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.Security

Public NotInheritable Class SecurityHelper
    Private Const Iterations As Integer = 600000

    Public Shared Function HashPassword(password As String) As String
        If password Is Nothing OrElse password.Length < 12 OrElse password.Length > 128 Then Throw New BusinessException("Use a password of 12–128 characters.")
        Dim salt(15) As Byte
        Using rng = RandomNumberGenerator.Create()
            rng.GetBytes(salt)
        End Using
        Using kdf As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
            Return "pbkdf2-sha256$" & Iterations.ToString() & "$" & Convert.ToBase64String(salt) & "$" & Convert.ToBase64String(kdf.GetBytes(32))
        End Using
    End Function

    Public Shared Function VerifyPassword(password As String, stored As String) As Boolean
        If password Is Nothing OrElse password.Length > 128 Then Return False
        Try
            Dim parts = stored.Split("$"c)
            If parts.Length <> 4 OrElse parts(0) <> "pbkdf2-sha256" Then Return False
            Dim rounds = Integer.Parse(parts(1))
            If rounds < Iterations OrElse rounds > 2000000 Then Return False
            Dim salt = Convert.FromBase64String(parts(2))
            Dim expected = Convert.FromBase64String(parts(3))
            If salt.Length <> 16 OrElse expected.Length <> 32 Then Return False
            Using kdf As New Rfc2898DeriveBytes(password, salt, rounds, HashAlgorithmName.SHA256)
                Dim actual = kdf.GetBytes(32)
                Dim difference As Integer = 0
                For i As Integer = 0 To actual.Length - 1
                    difference = difference Or (CInt(actual(i)) Xor CInt(expected(i)))
                Next
                Return difference = 0
            End Using
        Catch ex As FormatException
            Return False
        Catch ex As OverflowException
            Return False
        End Try
    End Function

    Public Shared Function Email(value As String) As String
        value = CommonHelper.Required(value, "Email", 254).ToLowerInvariant()
        If Not Regex.IsMatch(value, "\A[^\s@]+@[^\s@]+\.[^\s@]+\z") Then Throw New BusinessException("Enter a valid email address.")
        Return value
    End Function

    Public Shared Sub Register(name As String, emailAddress As String, phone As String, password As String, confirm As String, address As String, city As String)
        name = CommonHelper.Required(name, "Full name", 100)
        emailAddress = Email(emailAddress)
        phone = CommonHelper.Required(phone, "Phone", 20)
        If Not Regex.IsMatch(phone, "\A[+0-9 ()-]{7,20}\z") Then Throw New BusinessException("Enter a valid phone number.")
        address = CommonHelper.Required(address, "Address", 1000)
        city = CommonHelper.Required(city, "City", 100)
        If password <> confirm Then Throw New BusinessException("Passwords do not match.")
        Dim hash = HashPassword(password)
        Try
            DatabaseHelper.ExecuteNonQuery("INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Phone],[Address],[City],[Role],[Status],[CreatedDate],[CartVersion]) VALUES (?,?,?,?,?,?,'Customer','Active',?,0)", DatabaseHelper.P(name), DatabaseHelper.P(emailAddress), DatabaseHelper.P(hash), DatabaseHelper.P(phone), DatabaseHelper.P(address, OleDbType.LongVarWChar), DatabaseHelper.P(city), DatabaseHelper.P(DateTime.Now, OleDbType.Date))
        Catch ex As OleDbException
            ' ACE versions expose different native duplicate-key codes. Check the invariant
            ' after the failed INSERT; the unique index remains the race-safe authority.
            If Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Users] WHERE [Email]=?", DatabaseHelper.P(emailAddress))) > 0 Then Throw New BusinessException("This email is already registered. Please log in.")
            Throw
        End Try
    End Sub

    Public Shared Sub SignIn(context As HttpContext, emailAddress As String, password As String)
        emailAddress = Email(emailAddress)
        ' Single-server, bounded in-memory throttle: per client and per account, not just per session.
        LoginThrottle.Check(context.Request.UserHostAddress, emailAddress)
        Dim users = DatabaseHelper.FillDataTable("SELECT [UserID],[FullName],[PasswordHash],[Role],[Status] FROM [Users] WHERE [Email]=?", DatabaseHelper.P(emailAddress))
        Dim valid As Boolean = False
        If users.Rows.Count = 1 Then
            valid = VerifyPassword(password, CStr(users.Rows(0)("PasswordHash"))) AndAlso CStr(users.Rows(0)("Status")) = "Active"
        Else
            ' Similar work for unknown accounts to limit simple user enumeration.
            VerifyPassword(password, "pbkdf2-sha256$600000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")
        End If
        If Not valid Then Throw New BusinessException("Invalid email or password, or account inactive.")
        Dim row = users.Rows(0)
        context.Session.Clear()
        Dim nonce = Guid.NewGuid().ToString("N")
        context.Session("UserID") = CInt(row("UserID"))
        context.Session("UserName") = CStr(row("FullName"))
        context.Session("Role") = CStr(row("Role"))
        context.Session("AuthToken") = nonce
        Dim ticket As New FormsAuthenticationTicket(2, CStr(row("UserID")), DateTime.Now, DateTime.Now.AddMinutes(30), False, nonce, FormsAuthentication.FormsCookiePath)
        Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket)) With {.HttpOnly = True, .Secure = FormsAuthentication.RequireSSL, .SameSite = SameSiteMode.Lax, .Path = FormsAuthentication.FormsCookiePath}
        context.Response.Cookies.Add(cookie)
    End Sub

    Public Shared Sub SignOut(context As HttpContext)
        FormsAuthentication.SignOut()
        context.Session.Clear()
        context.Session.Abandon()
    End Sub
End Class

Public NotInheritable Class LoginThrottle
    Private Shared ReadOnly Gate As New Object()
    Public Shared Sub Check(ip As String, email As String)
        SyncLock Gate
            For Each key In New String() {"login-ip:" & ip, "login-email:" & email}
                Dim existing = TryCast(HttpRuntime.Cache(key), AttemptCounter)
                Dim count = If(existing Is Nothing, 0, existing.Count)
                Dim limit = If(key.StartsWith("login-ip:", StringComparison.Ordinal), 30, 10)
                If count >= limit Then Throw New BusinessException("Too many login attempts. Please wait 15 minutes.")
            Next
            For Each key In New String() {"login-ip:" & ip, "login-email:" & email}
                Dim entry = HttpRuntime.Cache(key)
                If entry Is Nothing Then
                    HttpRuntime.Cache.Insert(key, New AttemptCounter(), Nothing, DateTime.UtcNow.AddMinutes(15), System.Web.Caching.Cache.NoSlidingExpiration)
                Else
                    DirectCast(entry, AttemptCounter).Count += 1
                End If
            Next
        End SyncLock
    End Sub
    Private Class AttemptCounter
        Public Count As Integer = 1
    End Class
End Class
