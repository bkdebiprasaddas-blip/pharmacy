Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class CheckoutPage
    Inherits CustomerPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim rows = StoreService.Cart(UserId)
        ItemsGrid.DataSource = rows
        ItemsGrid.DataBind()
        TotalLabel.Text = CommonHelper.FormatCurrency(StoreService.Total(rows))
        SaveButton.Enabled = rows.Rows.Count > 0
        If rows.Rows.Count = 0 Then Notice = "Your cart is empty. Add products before checking out."
        Dim user = DatabaseHelper.FillDataTable("SELECT [Address],[City] FROM [Users] WHERE [UserID]=?", DatabaseHelper.P(UserId, OleDbType.Integer)).Rows(0)
        Address.Text = Convert.ToString(user("Address"))
        City.Text = Convert.ToString(user("City"))
        ViewState("CheckoutToken") = Guid.NewGuid().ToString("N")
    End Sub
    Protected Sub PlaceClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                Dim id = StoreService.Checkout(UserId, Address.Text, City.Text, Payment.SelectedValue, Convert.ToString(ViewState("CheckoutToken")))
                Response.Redirect("~/OrderDetails.aspx?id=" & id.ToString() & "&placed=1")
            End Sub)
    End Sub

End Class
