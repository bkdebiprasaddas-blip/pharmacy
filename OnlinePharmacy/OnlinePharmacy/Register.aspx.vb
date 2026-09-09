Public Class Register
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            ' Check if email already exists
        End If
    End Sub

    Protected Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim fullName As String = txtFullName.Text.Trim()
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()
        Dim phone As String = txtPhone.Text.Trim()
        Dim address As String = txtAddress.Text.Trim()

        ' Validate
        If String.IsNullOrEmpty(fullName) OrElse String.IsNullOrEmpty(email) OrElse String.IsNullOrEmpty(password) Then
            lblMessage.Text = "Please fill all required fields."
            lblMessage.CssClass = "error"
            Return
        End If

        ' Check if email exists
        Dim checkSql As String = "SELECT UserID FROM Users WHERE Email=?"
        Dim existingUser As Object = DatabaseHelper.ExecuteScalar(checkSql, DatabaseHelper.P(email))

        If existingUser IsNot Nothing AndAlso Not IsDBNull(existingUser) Then
            lblMessage.Text = "Email already registered. Please login."
            lblMessage.CssClass = "error"
            Return
        End If

        ' Insert new user
        Dim insertSql As String = "INSERT INTO Users (FullName, Email, Password, Phone, Address, Role, Status, CreatedDate) " &
                                 "VALUES (?, ?, ?, ?, ?, 'USER', 'ACTIVE', Now())"
        Try
            DatabaseHelper.ExecuteNonQuery(insertSql,
                DatabaseHelper.P(fullName),
                DatabaseHelper.P(email),
                DatabaseHelper.P(password),
                DatabaseHelper.P(phone),
                DatabaseHelper.P(address))

            lblMessage.Text = "Registration successful! Please login."
            lblMessage.CssClass = "success"
            txtFullName.Text = ""
            txtEmail.Text = ""
            txtPassword.Text = ""
            txtPhone.Text = ""
            txtAddress.Text = ""
        Catch ex As Exception
            lblMessage.Text = "Error: " & ex.Message
            lblMessage.CssClass = "error"
        End Try
    End Sub
End Class