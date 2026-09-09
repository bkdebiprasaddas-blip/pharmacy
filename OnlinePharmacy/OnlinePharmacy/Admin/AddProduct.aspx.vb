Public Class Admin_AddProduct
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("Role") Is Nothing OrElse Session("Role").ToString() <> "ADMIN" Then
            Response.Redirect("../Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadCategories()
        End If
    End Sub

    Private Sub LoadCategories()
        Dim sql As String = "SELECT CategoryID, CategoryName FROM Categories WHERE Status='ACTIVE' ORDER BY CategoryName"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql)
        ddlCategory.DataSource = dt
        ddlCategory.DataTextField = "CategoryName"
        ddlCategory.DataValueField = "CategoryID"
        ddlCategory.DataBind()
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim productName As String = txtProductName.Text.Trim()
        Dim categoryID As Integer = Convert.ToInt32(ddlCategory.SelectedValue)
        Dim brand As String = txtBrand.Text.Trim()
        Dim price As Decimal = Convert.ToDecimal(txtPrice.Text)
        Dim stock As Integer = Convert.ToInt32(txtStock.Text)
        Dim expiry As DateTime = Convert.ToDateTime(txtExpiry.Text)
        Dim manufacturer As String = txtManufacturer.Text.Trim()
        Dim description As String = txtDescription.Text.Trim()

        If String.IsNullOrEmpty(productName) Then
            lblMessage.Text = "Product name is required."
            Return
        End If

        Dim sql As String = "INSERT INTO Products (ProductName, CategoryID, Brand, Price, StockQuantity, " &
                           "ExpiryDate, Manufacturer, Description, Status, CreatedDate) " &
                           "VALUES (?, ?, ?, ?, ?, ?, ?, ?, 'ACTIVE', Now())"
        Try
            DatabaseHelper.ExecuteNonQuery(sql,
                DatabaseHelper.P(productName),
                DatabaseHelper.P(categoryID),
                DatabaseHelper.P(brand),
                DatabaseHelper.P(price),
                DatabaseHelper.P(stock),
                DatabaseHelper.P(expiry),
                DatabaseHelper.P(manufacturer),
                DatabaseHelper.P(description))

            lblMessage.ForeColor = System.Drawing.Color.Green
            lblMessage.Text = "Product added successfully!"
            Response.Redirect("ManageProducts.aspx")
        Catch ex As Exception
            lblMessage.Text = "Error: " & ex.Message
        End Try
    End Sub
End Class