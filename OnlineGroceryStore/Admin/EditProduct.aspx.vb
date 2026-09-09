Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_EditProductPage
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
        Dim row = StoreService.Product(CommonHelper.PositiveId(Request.QueryString("id")), True)
        ViewState("ProductID") = CInt(row("ProductID"))
        ViewState("Version") = CInt(row("Version"))
        ProductName.Text = CStr(row("ProductName"))
        Dim selected = Category.Items.FindByValue(CStr(row("CategoryID")))
        If selected IsNot Nothing Then Category.SelectedValue = selected.Value
        Brand.Text = Convert.ToString(row("Brand"))
        Unit.Text = CStr(row("Unit"))
        Price.Text = CDec(row("Price")).ToString("0.00", CultureInfo.InvariantCulture)
        Stock.Text = CStr(row("StockQuantity"))
        If Not IsDBNull(row("ExpiryDate")) Then Expiry.Text = CDate(row("ExpiryDate")).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        Description.Text = Convert.ToString(row("Description"))
        ImagePath.Text = Convert.ToString(row("ImagePath"))
        Status.SelectedValue = CStr(row("Status"))
    End Sub
    Protected Sub SaveClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                If Convert.ToInt32(ViewState("ProductID")) <> CommonHelper.PositiveId(Request.QueryString("id")) Then Throw New BusinessException("Reload the product before saving.")
                AdminService.SaveProduct(Convert.ToInt32(ViewState("ProductID")), Convert.ToInt32(ViewState("Version")), CommonHelper.PositiveId(Category.SelectedValue), ProductName.Text, Brand.Text, Unit.Text, Price.Text, Stock.Text, Expiry.Text, Description.Text, ImagePath.Text, Status.SelectedValue)
                Response.Redirect("~/Admin/ManageProducts.aspx")
            End Sub)
    End Sub

End Class
