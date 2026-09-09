Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class CartPage
    Inherits CustomerPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim rows = StoreService.Cart(UserId)
        CartGrid.DataSource = rows
        CartGrid.DataBind()
        TotalLabel.Text = CommonHelper.FormatCurrency(StoreService.Total(rows))
        CheckoutLink.Visible = rows.Rows.Count > 0
        ClearButton.Visible = rows.Rows.Count > 0
    End Sub
    Protected Sub CartCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName <> "SaveQty" AndAlso e.CommandName <> "RemoveItem" Then Return
        Run(Sub()
                Dim row = DirectCast(DirectCast(e.CommandSource, Control).NamingContainer, GridViewRow)
                Dim quantity = If(e.CommandName = "RemoveItem", 0, CommonHelper.PositiveId(DirectCast(row.FindControl("Qty"), TextBox).Text))
                StoreService.UpdateCart(UserId, CommonHelper.PositiveId(CStr(e.CommandArgument)), quantity, e.CommandName = "RemoveItem")
                BindData()
                Notice = "Basket updated."
            End Sub)
    End Sub
    Protected Sub ClearClick(sender As Object, e As EventArgs)
        Run(Sub()
                StoreService.ClearCart(UserId)
                BindData()
                Notice = "Your basket is now empty."
            End Sub)
    End Sub

End Class
