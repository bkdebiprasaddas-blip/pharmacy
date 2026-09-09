Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class ProductDetailsPage
    Inherits GroceryPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim row = StoreService.Product(CommonHelper.PositiveId(Request.QueryString("id")))
        ProductImage.ImageUrl = CommonHelper.ProductImage(row("ImagePath"))
        ProductName.Text = CStr(row("ProductName"))
        CategoryName.Text = CStr(row("CategoryName"))
        BrandUnit.Text = CStr(row("Brand")) & " · " & CStr(row("Unit"))
        PriceLabel.Text = CommonHelper.FormatCurrency(row("Price"))
        Description.Text = Convert.ToString(row("Description"))
        Stock.Text = CStr(row("StockQuantity")) & " units available"
        SaveButton.Enabled = CInt(row("StockQuantity")) > 0
        Quantity.Text = "1"
        DetailsPanel.Visible = True
    End Sub
    Protected Sub AddClick(sender As Object, e As EventArgs)
        RequireLogin()
        If Not Page.IsValid Then Return
        Run(Sub()
                StoreService.AddToCart(UserId, CommonHelper.PositiveId(Request.QueryString("id")), CommonHelper.PositiveId(Quantity.Text))
                Notice = "Added to your cart."
                BindData()
            End Sub)
    End Sub

End Class
