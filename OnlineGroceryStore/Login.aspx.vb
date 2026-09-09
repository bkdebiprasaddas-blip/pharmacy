Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class LoginPage
    Inherits GroceryPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack AndAlso Request.QueryString("registered") = "1" Then Notice = "Registration successful. You can now log in."
    End Sub
    Protected Sub LoginClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                SecurityHelper.SignIn(Context, Email.Text, Password.Text)
                Response.Redirect(If(IsAdmin, "~/Admin/Dashboard.aspx", "~/Default.aspx"))
            End Sub)
    End Sub

End Class
