Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class LogoutPage
    Inherits CustomerPage
    Protected Sub LogoutClick(sender As Object, e As EventArgs)
        SecurityHelper.SignOut(Context)
        Response.Redirect("~/Login.aspx")
    End Sub

End Class
