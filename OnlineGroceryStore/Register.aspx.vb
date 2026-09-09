Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class RegisterPage
    Inherits GroceryPage
    Protected Sub RegisterClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                SecurityHelper.Register(FullName.Text, Email.Text, Phone.Text, Password.Text, ConfirmPassword.Text, Address.Text, City.Text)
                Response.Redirect("~/Login.aspx?registered=1")
            End Sub)
    End Sub

End Class
