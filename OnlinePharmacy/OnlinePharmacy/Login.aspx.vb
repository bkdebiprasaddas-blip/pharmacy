Imports System.Data

Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim email As String = txtEmail.Text.Trim()
        Dim password As String = txtPassword.Text.Trim()

        If String.IsNullOrEmpty(email) OrElse String.IsNullOrEmpty(password) Then
            lblMessage.Text = "Please enter email and password."
            Return
        End If

        Dim sql As String = "SELECT UserID, FullName, Role, Status FROM Users WHERE Email=? AND Password=?"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql,
            DatabaseHelper.P(email), DatabaseHelper.P(password))

        If dt.Rows.Count > 0 Then
            Dim row As DataRow = dt.Rows(0)
            Dim status As String = row("Status").ToString()

            If status = "INACTIVE" Then
                lblMessage.Text = "Your account is inactive. Please contact admin."
                Return
            End If

            Session("UserID") = row("UserID")
            Session("UserName") = row("FullName")
            Session("Role") = row("Role")

            If row("Role").ToString() = "ADMIN" Then
                Response.Redirect("Admin/Dashboard.aspx")
            Else
                Response.Redirect("Default.aspx")
            End If
        Else
            lblMessage.Text = "Invalid email or password."
        End If
    End Sub
End Class