Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_ManageCustomersPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        CustomersGrid.DataSource = AdminService.Customers(Search.Text)
        CustomersGrid.DataBind()
    End Sub
    Protected Sub SearchClick(sender As Object, e As EventArgs)
        Run(Sub() BindData())
    End Sub
    Protected Sub CustomerCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName <> "ActivateCustomer" AndAlso e.CommandName <> "DeactivateCustomer" Then Return
        Run(Sub()
                AdminService.CustomerStatus(CommonHelper.PositiveId(CStr(e.CommandArgument)), If(e.CommandName = "ActivateCustomer", "Active", "Inactive"))
                BindData()
                Notice = "Customer status updated."
            End Sub)
    End Sub

End Class
