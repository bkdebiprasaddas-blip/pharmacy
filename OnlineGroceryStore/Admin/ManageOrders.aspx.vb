Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_ManageOrdersPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        OrdersGrid.DataSource = AdminService.AllOrders(FilterStatus.SelectedValue)
        OrdersGrid.DataBind()
    End Sub
    Protected Sub FilterClick(sender As Object, e As EventArgs)
        Run(Sub() BindData())
    End Sub
    Protected Sub OrderCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName <> "ShowItems" Then Return
        Run(Sub()
                Dim id = CommonHelper.PositiveId(CStr(e.CommandArgument))
                ItemsGrid.DataSource = AdminService.OrderItems(id)
                ItemsGrid.DataBind()
                ItemsHeading.Text = "Items for order #" & id.ToString()
                OrderIdInput.Text = id.ToString()
            End Sub)
    End Sub
    Protected Sub StatusClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                AdminService.ChangeOrderStatus(CommonHelper.PositiveId(OrderIdInput.Text), NextStatus.SelectedValue)
                BindData()
                Notice = "Order status updated."
            End Sub)
    End Sub

End Class
