Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Linq
Imports System.Collections.Generic

Public NotInheritable Class StoreService
    Private Shared Function I(value As Integer) As OleDbParameter
        Return DatabaseHelper.P(value, OleDbType.Integer)
    End Function
    Private Shared Sub LockCart(db As DbSession, userId As Integer)
        If db.Execute("UPDATE [Users] SET [CartVersion]=[CartVersion]+1 WHERE [UserID]=? AND [Status]='Active'", I(userId)) <> 1 Then Throw New BusinessException("Your account is not active.")
    End Sub

    Public Shared Function Categories(Optional activeOnly As Boolean = True) As DataTable
        If activeOnly Then Return DatabaseHelper.FillDataTable("SELECT * FROM [Categories] WHERE [Status]='Active' ORDER BY [CategoryName]")
        Return DatabaseHelper.FillDataTable("SELECT * FROM [Categories] ORDER BY [CategoryName]")
    End Function

    Public Shared Function Products(search As String, categoryId As Integer, Optional admin As Boolean = False) As DataTable
        search = If(search, "").Trim()
        If search.Length > 100 Then Throw New BusinessException("Search must be 100 characters or fewer.")
        ' InStr provides literal substring search (including %, _, and apostrophes).
        Dim sql = "SELECT p.*, c.[CategoryName] FROM [Products] AS p INNER JOIN [Categories] AS c ON p.[CategoryID]=c.[CategoryID] WHERE (InStr(1,p.[ProductName],?,1)>0 OR InStr(1,p.[Brand],?,1)>0) AND (?=0 OR p.[CategoryID]=?)"
        If Not admin Then sql &= " AND p.[Status]='Active' AND c.[Status]='Active' AND (p.[ExpiryDate] IS NULL OR p.[ExpiryDate]>=?)"
        sql &= " ORDER BY p.[ProductName]"
        Dim args As New List(Of OleDbParameter) From {DatabaseHelper.P(search), DatabaseHelper.P(search), I(categoryId), I(categoryId)}
        If Not admin Then args.Add(DatabaseHelper.P(Date.Today, OleDbType.Date))
        Return DatabaseHelper.FillDataTable(sql, args.ToArray())
    End Function

    Public Shared Function Product(id As Integer, Optional admin As Boolean = False) As DataRow
        Dim sql = "SELECT p.*, c.[CategoryName], c.[Status] AS [CategoryStatus] FROM [Products] AS p INNER JOIN [Categories] AS c ON p.[CategoryID]=c.[CategoryID] WHERE p.[ProductID]=?"
        Dim args As New List(Of OleDbParameter) From {I(id)}
        If Not admin Then
            sql &= " AND p.[Status]='Active' AND c.[Status]='Active' AND (p.[ExpiryDate] IS NULL OR p.[ExpiryDate]>=?)"
            args.Add(DatabaseHelper.P(Date.Today, OleDbType.Date))
        End If
        Dim result = DatabaseHelper.FillDataTable(sql, args.ToArray())
        If result.Rows.Count = 0 Then Throw New BusinessException("Product not found or unavailable.")
        Return result.Rows(0)
    End Function

    Private Shared Function ReadCart(db As DbSession, userId As Integer) As DataTable
        Return db.Table("SELECT ct.[CartID], ct.[ProductID], ct.[Quantity], p.[ProductName], p.[Unit], p.[Price], p.[StockQuantity], p.[Status], p.[ExpiryDate], c.[Status] AS [CategoryStatus], (ct.[Quantity]*p.[Price]) AS [Subtotal] FROM ([Cart] AS ct INNER JOIN [Products] AS p ON ct.[ProductID]=p.[ProductID]) INNER JOIN [Categories] AS c ON p.[CategoryID]=c.[CategoryID] WHERE ct.[UserID]=? ORDER BY ct.[ProductID]", I(userId))
    End Function
    Public Shared Function Cart(userId As Integer) As DataTable
        Using db As New DbSession()
            Return ReadCart(db, userId)
        End Using
    End Function
    Public Shared Function Total(rows As DataTable) As Decimal
        Dim amount As Decimal = 0D
        For Each row As DataRow In rows.Rows
            amount += CDec(row("Price")) * CInt(row("Quantity"))
        Next
        Return amount
    End Function
    Private Shared Sub ValidateStock(row As DataRow, quantity As Integer)
        If quantity < 1 OrElse quantity > 1000000 Then Throw New BusinessException("Quantity must be between 1 and 1,000,000.")
        If CStr(row("Status")) <> "Active" OrElse CStr(row("CategoryStatus")) <> "Active" OrElse (Not IsDBNull(row("ExpiryDate")) AndAlso CDate(row("ExpiryDate")) < Date.Today) Then Throw New BusinessException(CStr(row("ProductName")) & " is no longer available. Remove it from your cart.")
        If quantity > CInt(row("StockQuantity")) Then Throw New BusinessException("Not enough stock for " & CStr(row("ProductName")) & ". Available: " & CStr(row("StockQuantity")))
        If CDec(row("Price")) < 0D Then Throw New BusinessException("Invalid product price. Contact the store.")
    End Sub

    Public Shared Sub AddToCart(userId As Integer, productId As Integer, quantity As Integer)
        If quantity < 1 OrElse quantity > 1000000 Then Throw New BusinessException("Invalid quantity.")
        Using db As New DbSession(True)
            LockCart(db, userId)
            Dim products = db.Table("SELECT p.*, c.[Status] AS [CategoryStatus] FROM [Products] AS p INNER JOIN [Categories] AS c ON p.[CategoryID]=c.[CategoryID] WHERE p.[ProductID]=?", I(productId))
            If products.Rows.Count <> 1 Then Throw New BusinessException("Product not found.")
            Dim existing = db.Table("SELECT [CartID],[Quantity] FROM [Cart] WHERE [UserID]=? AND [ProductID]=?", I(userId), I(productId))
            Dim oldQuantity = If(existing.Rows.Count = 0, 0, CInt(existing.Rows(0)("Quantity")))
            If oldQuantity > 1000000 - quantity Then Throw New BusinessException("Quantity is too large.")
            Dim combined = quantity + oldQuantity
            ValidateStock(products.Rows(0), combined)
            If existing.Rows.Count = 0 Then
                db.Execute("INSERT INTO [Cart] ([UserID],[ProductID],[Quantity],[AddedDate]) VALUES (?,?,?,?)", I(userId), I(productId), I(combined), DatabaseHelper.P(DateTime.Now, OleDbType.Date))
            Else
                db.Execute("UPDATE [Cart] SET [Quantity]=? WHERE [UserID]=? AND [ProductID]=?", I(combined), I(userId), I(productId))
            End If
            db.Commit()
        End Using
    End Sub

    Public Shared Sub UpdateCart(userId As Integer, cartId As Integer, quantity As Integer, Optional remove As Boolean = False)
        Using db As New DbSession(True)
            LockCart(db, userId)
            Dim rows = ReadCart(db, userId).Select("CartID=" & cartId.ToString(Globalization.CultureInfo.InvariantCulture))
            If rows.Length <> 1 Then Throw New BusinessException("Cart item not found.")
            If remove Then
                db.Execute("DELETE FROM [Cart] WHERE [CartID]=? AND [UserID]=?", I(cartId), I(userId))
            Else
                ValidateStock(rows(0), quantity)
                db.Execute("UPDATE [Cart] SET [Quantity]=? WHERE [CartID]=? AND [UserID]=?", I(quantity), I(cartId), I(userId))
            End If
            db.Commit()
        End Using
    End Sub

    Public Shared Sub ClearCart(userId As Integer)
        Using db As New DbSession(True)
            LockCart(db, userId)
            db.Execute("DELETE FROM [Cart] WHERE [UserID]=?", I(userId))
            db.Commit()
        End Using
    End Sub

    Public Shared Function Checkout(userId As Integer, address As String, city As String, payment As String, token As String) As Integer
        address = CommonHelper.Required(address, "Delivery address", 1000)
        city = CommonHelper.Required(city, "City", 100)
        If payment <> "Cash on Delivery" Then Throw New BusinessException("Only Cash on Delivery is currently supported. No online payment will be collected.")
        Dim parsed As Guid
        If Not Guid.TryParseExact(token, "N", parsed) Then Throw New BusinessException("Checkout expired. Reload the page.")
        Using db As New DbSession(True)
            LockCart(db, userId)
            Dim previous = db.Table("SELECT [OrderID] FROM [Orders] WHERE [CheckoutToken]=? AND [UserID]=?", DatabaseHelper.P(token), I(userId))
            If previous.Rows.Count > 0 Then
                db.Commit()
                Return CInt(previous.Rows(0)("OrderID"))
            End If
            Dim rows = ReadCart(db, userId)
            If rows.Rows.Count = 0 Then Throw New BusinessException("Your cart is empty.")
            For Each row As DataRow In rows.Rows
                ValidateStock(row, CInt(row("Quantity")))
            Next
            Dim amount = Total(rows)
            If amount > 100000000D Then Throw New BusinessException("Order total is too large. Please contact the store.")
            db.Execute("INSERT INTO [Orders] ([UserID],[OrderDate],[TotalAmount],[Status],[DeliveryAddress],[City],[PaymentMethod],[CheckoutToken]) VALUES (?,?,?,'Pending',?,?,?,?)", I(userId), DatabaseHelper.P(DateTime.Now, OleDbType.Date), DatabaseHelper.P(amount, OleDbType.Currency), DatabaseHelper.P(address, OleDbType.LongVarWChar), DatabaseHelper.P(city), DatabaseHelper.P(payment), DatabaseHelper.P(token))
            Dim orderId = db.Identity()
            For Each row As DataRow In rows.Rows
                Dim qty = CInt(row("Quantity"))
                Dim productId = CInt(row("ProductID"))
                ' Conditional decrement is essential even after the initial stock check.
                If db.Execute("UPDATE [Products] SET [StockQuantity]=[StockQuantity]-?, [Version]=[Version]+1 WHERE [ProductID]=? AND [StockQuantity]>=? AND [Status]='Active' AND ([ExpiryDate] IS NULL OR [ExpiryDate]>=?)", I(qty), I(productId), I(qty), DatabaseHelper.P(Date.Today, OleDbType.Date)) <> 1 Then Throw New BusinessException("Stock changed during checkout. Refresh your cart and try again.")
                Dim price = CDec(row("Price"))
                db.Execute("INSERT INTO [OrderItems] ([OrderID],[ProductID],[ProductName],[Unit],[Quantity],[Price],[Subtotal]) VALUES (?,?,?,?,?,?,?)", I(orderId), I(productId), DatabaseHelper.P(CStr(row("ProductName"))), DatabaseHelper.P(CStr(row("Unit"))), I(qty), DatabaseHelper.P(price, OleDbType.Currency), DatabaseHelper.P(price * qty, OleDbType.Currency))
            Next
            db.Execute("DELETE FROM [Cart] WHERE [UserID]=?", I(userId))
            db.Commit()
            Return orderId
        End Using
    End Function

    Public Shared Function Orders(userId As Integer) As DataTable
        Return DatabaseHelper.FillDataTable("SELECT [OrderID],[OrderDate],[TotalAmount],[PaymentMethod],[Status] FROM [Orders] WHERE [UserID]=? ORDER BY [OrderID] DESC", I(userId))
    End Function
    Public Shared Function Order(userId As Integer, orderId As Integer) As DataRow
        Dim rows = DatabaseHelper.FillDataTable("SELECT o.*, u.[FullName], u.[Email], u.[Phone] FROM [Orders] AS o INNER JOIN [Users] AS u ON o.[UserID]=u.[UserID] WHERE o.[OrderID]=? AND o.[UserID]=?", I(orderId), I(userId))
        If rows.Rows.Count <> 1 Then Throw New BusinessException("Order not found.")
        Return rows.Rows(0)
    End Function
    Public Shared Function OrderItems(userId As Integer, orderId As Integer) As DataTable
        Return DatabaseHelper.FillDataTable("SELECT i.[ProductName],i.[Unit],i.[Quantity],i.[Price],i.[Subtotal] FROM [OrderItems] AS i INNER JOIN [Orders] AS o ON i.[OrderID]=o.[OrderID] WHERE o.[OrderID]=? AND o.[UserID]=?", I(orderId), I(userId))
    End Function
End Class
