Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data.OleDb
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI

Public Class GroceryPage
    Inherits Page
    Public Property Notice As String = ""
    Public ReadOnly Property UserId As Integer
        Get
            Return Convert.ToInt32(Session("UserID"))
        End Get
    End Property
    Public ReadOnly Property IsAdmin As Boolean
        Get
            Return UserId > 0 AndAlso Convert.ToString(Session("Role")) = "Admin"
        End Get
    End Property
    Public ReadOnly Property CsrfToken As String
        Get
            If Session("CsrfToken") Is Nothing Then Session("CsrfToken") = Guid.NewGuid().ToString("N")
            Return CStr(Session("CsrfToken"))
        End Get
    End Property
    Protected Overrides Sub OnInit(e As EventArgs)
        ViewStateUserKey = CsrfToken
        If Request.HttpMethod = "POST" AndAlso Request.Form("__csrf") <> CsrfToken Then Throw New HttpException(403, "The form expired. Reload the page.")
        ValidateIdentity()
        MyBase.OnInit(e)
    End Sub
    Private Sub ValidateIdentity()
        If UserId = 0 Then Return
        Dim identity = TryCast(Context.User.Identity, FormsIdentity)
        If identity Is Nothing OrElse Not identity.IsAuthenticated OrElse identity.Ticket.Expired OrElse identity.Name <> UserId.ToString() OrElse identity.Ticket.UserData <> Convert.ToString(Session("AuthToken")) Then
            SecurityHelper.SignOut(Context)
            Response.Redirect("~/Login.aspx")
            Return
        End If
        Dim rows = DatabaseHelper.FillDataTable("SELECT [FullName],[Role],[Status] FROM [Users] WHERE [UserID]=?", DatabaseHelper.P(UserId, OleDbType.Integer))
        If rows.Rows.Count <> 1 OrElse CStr(rows.Rows(0)("Status")) <> "Active" Then
            SecurityHelper.SignOut(Context)
            Response.Redirect("~/Login.aspx")
            Return
        End If
        Session("Role") = CStr(rows.Rows(0)("Role"))
        Session("UserName") = CStr(rows.Rows(0)("FullName"))
    End Sub
    Protected Sub RequireLogin()
        If UserId = 0 Then Response.Redirect("~/Login.aspx")
    End Sub
    Protected Sub Run(action As Action)
        Try
            action()
        Catch ex As BusinessException
            Notice = ex.Message
        Catch ex As OleDbException
            System.Diagnostics.Trace.TraceError(ex.ToString())
            Notice = "The database is unavailable or this record changed. Refresh and try again. If the problem continues, contact the administrator."
        End Try
    End Sub
End Class

Public Class CustomerPage
    Inherits GroceryPage
    Protected Overrides Sub OnInit(e As EventArgs)
        MyBase.OnInit(e)
        RequireLogin()
        Response.Cache.SetCacheability(HttpCacheability.NoCache)
        Response.Cache.SetNoStore()
    End Sub
End Class

Public Class AdminPage
    Inherits CustomerPage
    Protected Overrides Sub OnInit(e As EventArgs)
        MyBase.OnInit(e)
        If Not IsAdmin Then Throw New HttpException(403, "Administrator access required.")
    End Sub
End Class
