Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Web

Public NotInheritable Class AdminService
    Private Shared Sub Authorize()
        Dim page = TryCast(HttpContext.Current.Handler, GroceryPage)
        If page Is Nothing OrElse Not page.IsAdmin Then Throw New HttpException(403, "Administrator access required.")
    End Sub
    Private Shared Function I(value As Integer) As OleDbParameter
        Return DatabaseHelper.P(value, OleDbType.Integer)
    End Function

    Public Shared Sub SaveCategory(id As Integer, name As String, description As String, status As String)
        Authorize()
        name = CommonHelper.Required(name, "Category name", 100)
        status = CommonHelper.Status(status)
        If description.Length > 2000 Then Throw New BusinessException("Description is too long.")
        Try
            If id = 0 Then
                DatabaseHelper.ExecuteNonQuery("INSERT INTO [Categories] ([CategoryName],[Description],[Status]) VALUES (?,?,?)", DatabaseHelper.P(name), DatabaseHelper.P(description, OleDbType.LongVarWChar), DatabaseHelper.P(status))
            Else
                If DatabaseHelper.ExecuteNonQuery("UPDATE [Categories] SET [CategoryName]=?,[Description]=?,[Status]=? WHERE [CategoryID]=?", DatabaseHelper.P(name), DatabaseHelper.P(description, OleDbType.LongVarWChar), DatabaseHelper.P(status), I(id)) <> 1 Then Throw New BusinessException("Category not found.")
            End If
        Catch ex As OleDbException
            If Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Categories] WHERE [CategoryName]=? AND [CategoryID]<>?", DatabaseHelper.P(name), I(id))) > 0 Then Throw New BusinessException("A category with this name already exists.")
            Throw
        End Try
    End Sub
    Public Shared Sub DeleteCategory(id As Integer)
        Authorize()
        Using db As New DbSession(True)
            If Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM [Products] WHERE [CategoryID]=?", I(id))) > 0 Then Throw New BusinessException("This category contains products. Deactivate it instead.")
            If db.Execute("DELETE FROM [Categories] WHERE [CategoryID]=?", I(id)) <> 1 Then Throw New BusinessException("Category not found.")
            db.Commit()
        End Using
    End Sub
    Public Shared Sub SaveProduct(id As Integer, version As Integer, category As Integer, name As String, brand As String, unit As String, priceText As String, stockText As String, expiryText As String, description As String, imagePath As String, status As String)
        Authorize()
        name = CommonHelper.Required(name, "Product name", 150)
        brand = CommonHelper.Required(brand, "Brand", 100)
        unit = CommonHelper.Required(unit, "Unit", 40)
        Dim price = CommonHelper.Price(priceText)
        Dim stock = CommonHelper.Nonnegative(stockText, "Stock")
        status = CommonHelper.Status(status)
        If description.Length > 4000 Then Throw New BusinessException("Description is too long.")
        Dim expiry As Object = DBNull.Value
        If Not String.IsNullOrWhiteSpace(expiryText) Then
            Dim parsed As DateTime
            If Not DateTime.TryParseExact(expiryText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, parsed) Then Throw New BusinessException("Expiry must use yyyy-MM-dd.")
            If status = "Active" AndAlso parsed < Date.Today Then Throw New BusinessException("Expired products cannot be activated.")
            expiry = parsed
        End If
        imagePath = imagePath.Trim()
        If imagePath.Length > 0 Then
            If Not System.Text.RegularExpressions.Regex.IsMatch(imagePath, "\A~/Images/Products/[a-zA-Z0-9_-]+\.(png|jpg|jpeg|webp)\z", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then Throw New BusinessException("Use a local image path such as ~/Images/Products/rice.jpg, or leave it blank.")
            If Not System.IO.File.Exists(HttpContext.Current.Server.MapPath(imagePath)) Then Throw New BusinessException("Copy the image to Images/Products first.")
        End If
        Using db As New DbSession(True)
            If Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM [Categories] WHERE [CategoryID]=? AND [Status]='Active'", I(category))) <> 1 Then Throw New BusinessException("Select an active category.")
            Dim args As New List(Of OleDbParameter) From {I(category), DatabaseHelper.P(name), DatabaseHelper.P(brand), DatabaseHelper.P(unit), DatabaseHelper.P(price, OleDbType.Currency), I(stock), DatabaseHelper.P(expiry, OleDbType.Date), DatabaseHelper.P(description, OleDbType.LongVarWChar), DatabaseHelper.P(imagePath), DatabaseHelper.P(status)}
            If id = 0 Then
                args.Add(DatabaseHelper.P(DateTime.Now, OleDbType.Date))
                db.Execute("INSERT INTO [Products] ([CategoryID],[ProductName],[Brand],[Unit],[Price],[StockQuantity],[ExpiryDate],[Description],[ImagePath],[Status],[CreatedDate],[Version]) VALUES (?,?,?,?,?,?,?,?,?,?,?,0)", args.ToArray())
            Else
                args.Add(I(id))
                args.Add(I(version))
                If db.Execute("UPDATE [Products] SET [CategoryID]=?,[ProductName]=?,[Brand]=?,[Unit]=?,[Price]=?,[StockQuantity]=?,[ExpiryDate]=?,[Description]=?,[ImagePath]=?,[Status]=?,[Version]=[Version]+1 WHERE [ProductID]=? AND [Version]=?", args.ToArray()) <> 1 Then Throw New BusinessException("Product or stock changed while editing. Reload this page before saving again.")
            End If
            db.Commit()
        End Using
    End Sub
    Public Shared Sub DeleteProduct(id As Integer)
        Authorize()
        Using db As New DbSession(True)
            If Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM [OrderItems] WHERE [ProductID]=?", I(id))) > 0 OrElse Convert.ToInt32(db.Scalar("SELECT COUNT(*) FROM [Cart] WHERE [ProductID]=?", I(id))) > 0 Then Throw New BusinessException("This product is in a cart or order. Deactivate it instead to preserve records.")
            If db.Execute("DELETE FROM [Products] WHERE [ProductID]=?", I(id)) <> 1 Then Throw New BusinessException("Product not found.")
            db.Commit()
        End Using
    End Sub
    Public Shared Function Customers(search As String) As DataTable
        Authorize()
        If search.Length > 100 Then Throw New BusinessException("Search is too long.")
        Return DatabaseHelper.FillDataTable("SELECT [UserID],[FullName],[Email],[Phone],[Address],[City],[Status],[CreatedDate] FROM [Users] WHERE [Role]='Customer' AND (InStr(1,[FullName],?,1)>0 OR InStr(1,[Email],?,1)>0) ORDER BY [UserID] DESC", DatabaseHelper.P(search), DatabaseHelper.P(search))
    End Function
    Public Shared Sub CustomerStatus(id As Integer, status As String)
        Authorize()
        status = CommonHelper.Status(status)
        If DatabaseHelper.ExecuteNonQuery("UPDATE [Users] SET [Status]=? WHERE [UserID]=? AND [Role]='Customer'", DatabaseHelper.P(status), I(id)) <> 1 Then Throw New BusinessException("Customer not found.")
    End Sub
    Public Shared Function AllOrders(Optional status As String = "") As DataTable
        Authorize()
        Return DatabaseHelper.FillDataTable("SELECT o.[OrderID], u.[FullName], u.[Email], u.[Phone], o.[DeliveryAddress], o.[City], o.[OrderDate], o.[TotalAmount], o.[PaymentMethod], o.[Status] FROM [Orders] AS o INNER JOIN [Users] AS u ON o.[UserID]=u.[UserID] WHERE (?='' OR o.[Status]=?) ORDER BY o.[OrderID] DESC", DatabaseHelper.P(status), DatabaseHelper.P(status))
    End Function
    Public Shared Function OrderItems(id As Integer) As DataTable
        Authorize()
        Return DatabaseHelper.FillDataTable("SELECT [ProductName],[Unit],[Quantity],[Price],[Subtotal] FROM [OrderItems] WHERE [OrderID]=?", I(id))
    End Function
    Public Shared Function CanTransition(current As String, target As String) As Boolean
        Select Case current
            Case "Pending"
                Return target = "Confirmed" OrElse target = "Cancelled"
            Case "Confirmed"
                Return target = "Packed" OrElse target = "Cancelled"
            Case "Packed"
                Return target = "Out for Delivery" OrElse target = "Cancelled"
            Case "Out for Delivery"
                Return target = "Delivered"
            Case Else
                Return False
        End Select
    End Function
    Public Shared Sub ChangeOrderStatus(id As Integer, target As String)
        Authorize()
        Using db As New DbSession(True)
            Dim current = Convert.ToString(db.Scalar("SELECT [Status] FROM [Orders] WHERE [OrderID]=?", I(id)))
            If Not CanTransition(current, target) Then Throw New BusinessException("Invalid transition. Follow Pending → Confirmed → Packed → Out for Delivery → Delivered. Cancellation is allowed only before dispatch.")
            If db.Execute("UPDATE [Orders] SET [Status]=? WHERE [OrderID]=? AND [Status]=?", DatabaseHelper.P(target), I(id), DatabaseHelper.P(current)) <> 1 Then Throw New BusinessException("Order changed. Refresh and try again.")
            If target = "Cancelled" Then
                For Each row As DataRow In db.Table("SELECT [ProductID],[Quantity] FROM [OrderItems] WHERE [OrderID]=? ORDER BY [ProductID]", I(id)).Rows
                    If db.Execute("UPDATE [Products] SET [StockQuantity]=[StockQuantity]+?, [Version]=[Version]+1 WHERE [ProductID]=?", I(CInt(row("Quantity"))), I(CInt(row("ProductID")))) <> 1 Then Throw New BusinessException("Cannot restore stock. No changes were saved.")
                Next
            End If
            db.Commit()
        End Using
    End Sub
    Public Shared Function Statistics() As DataTable
        Authorize()
        Dim table As New DataTable()
        table.Columns.Add("Label")
        table.Columns.Add("Value")
        table.Rows.Add("Customers", DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Users] WHERE [Role]='Customer'"))
        table.Rows.Add("Products", DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Products]"))
        table.Rows.Add("Orders", DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Orders]"))
        table.Rows.Add("Delivered revenue", CommonHelper.FormatCurrency(DatabaseHelper.ExecuteScalar("SELECT SUM([TotalAmount]) FROM [Orders] WHERE [Status]='Delivered'")))
        table.Rows.Add("Pending orders", DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM [Orders] WHERE [Status]='Pending'"))
        Return table
    End Function
    Public Shared Function LowStock(threshold As Integer) As DataTable
        Authorize()
        Return DatabaseHelper.FillDataTable("SELECT [ProductID],[ProductName],[StockQuantity],[Status] FROM [Products] WHERE [StockQuantity]<=? ORDER BY [StockQuantity],[ProductName]", I(threshold))
    End Function
    Public Shared Function TopSelling() As DataTable
        Authorize()
        Return DatabaseHelper.FillDataTable("SELECT TOP 5 i.[ProductID], i.[ProductName], SUM(i.[Quantity]) AS [UnitsSold], SUM(i.[Subtotal]) AS [Revenue] FROM [OrderItems] AS i INNER JOIN [Orders] AS o ON i.[OrderID]=o.[OrderID] WHERE o.[Status]='Delivered' GROUP BY i.[ProductID],i.[ProductName] ORDER BY SUM(i.[Quantity]) DESC")
    End Function
    Public Shared Function Sales(startDate As DateTime, endDate As DateTime) As DataTable
        Authorize()
        If endDate < startDate OrElse (endDate - startDate).TotalDays > 366 Then Throw New BusinessException("Choose a date range of at most 366 days, with end on or after start.")
        Return DatabaseHelper.FillDataTable("SELECT o.[OrderID],o.[OrderDate],u.[FullName],o.[TotalAmount],o.[Status] FROM [Orders] AS o INNER JOIN [Users] AS u ON o.[UserID]=u.[UserID] WHERE o.[OrderDate]>=? AND o.[OrderDate]<? ORDER BY o.[OrderID] DESC", DatabaseHelper.P(startDate.Date, OleDbType.Date), DatabaseHelper.P(endDate.Date.AddDays(1), OleDbType.Date))
    End Function
End Class
