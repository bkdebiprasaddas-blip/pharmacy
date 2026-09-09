Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_ReportsPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            StartDate.Text = New DateTime(Date.Today.Year, Date.Today.Month, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            EndDate.Text = Date.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            Threshold.Text = "5"
            Run(Sub() BindData())
        End If
    End Sub
    Protected Sub FilterClick(sender As Object, e As EventArgs)
        If Page.IsValid Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim first, last As DateTime
        If Not DateTime.TryParseExact(StartDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, first) OrElse Not DateTime.TryParseExact(EndDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, last) Then Throw New BusinessException("Enter valid report dates.")
        Dim rows = AdminService.Sales(first, last)
        SalesGrid.DataSource = rows
        SalesGrid.DataBind()
        Dim amount As Decimal = 0D
        For Each row As DataRow In rows.Rows
            If CStr(row("Status")) = "Delivered" Then amount += CDec(row("TotalAmount"))
        Next
        Revenue.Text = CommonHelper.FormatCurrency(amount)
        LowStockGrid.DataSource = AdminService.LowStock(CommonHelper.Nonnegative(Threshold.Text, "Threshold"))
        LowStockGrid.DataBind()
        ProductsGrid.DataSource = StoreService.Products("", 0, True)
        ProductsGrid.DataBind()
    End Sub

End Class
