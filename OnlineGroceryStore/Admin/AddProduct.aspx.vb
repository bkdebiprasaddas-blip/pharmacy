Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_AddProductPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Category.DataSource = StoreService.Categories()
        Category.DataTextField = "CategoryName"
        Category.DataValueField = "CategoryID"
        Category.DataBind()
        Category.Items.Insert(0, New ListItem("Select an active category", "0"))
        Price.Text = "0.00"
        Stock.Text = "0"
    End Sub
    Protected Sub SaveClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                AdminService.SaveProduct(Convert.ToInt32(ViewState("ProductID")), Convert.ToInt32(ViewState("Version")), CommonHelper.PositiveId(Category.SelectedValue), ProductName.Text, Brand.Text, Unit.Text, Price.Text, Stock.Text, Expiry.Text, Description.Text, ImagePath.Text, Status.SelectedValue)
                Response.Redirect("~/Admin/ManageProducts.aspx")
            End Sub)
    End Sub

End Class
