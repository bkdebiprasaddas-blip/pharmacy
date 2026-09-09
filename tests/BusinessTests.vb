Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Linq
Imports System.Threading.Tasks

' Run only through scripts/Test-Business.ps1, which creates a disposable Access database.
Module BusinessTests
    Private assertions As Integer
    Sub Main(args As String())
        If args.Length <> 1 Then Throw New Exception("Expected isolated App_Data directory.")
        AppDomain.CurrentDomain.SetData("DataDirectory", args(0))
        Try
            TestRules()
            TestShopping()
            Console.WriteLine("PASS: " & assertions & " assertions against Microsoft ACE.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
    Private Sub Check(condition As Boolean, message As String)
        If Not condition Then Throw New Exception("FAIL: " & message)
        assertions += 1
        Console.WriteLine("PASS: " & message)
    End Sub
    Private Sub Reject(action As Action, message As String)
        Try
            action()
        Catch ex As BusinessException
            Check(True, message)
            Return
        End Try
        Throw New Exception("FAIL: expected BusinessException: " & message)
    End Sub
    Private Function I(value As Integer) As OleDbParameter
        Return DatabaseHelper.P(value, OleDbType.Integer)
    End Function
    Private Sub TestRules()
        Dim password = Guid.NewGuid().ToString("N")
        Dim hash = SecurityHelper.HashPassword(password)
        Check(SecurityHelper.VerifyPassword(password, hash), "Password verifies")
        Check(Not SecurityHelper.VerifyPassword("wrong-password", hash), "Wrong password rejected")
        Check(hash <> SecurityHelper.HashPassword(password), "Random salts produce different hashes")
        Check(Not SecurityHelper.VerifyPassword(password, "broken"), "Malformed hash rejected")
        Reject(Sub() SecurityHelper.HashPassword("short"), "Short password rejected")
        Check(CommonHelper.Price("12.50") = 12.5D, "Decimal currency parsed")
        Reject(Sub() CommonHelper.Price("-1"), "Negative price rejected")
        Reject(Sub() CommonHelper.Price("1.001"), "Sub-paise price rejected")
        Reject(Sub() CommonHelper.PositiveId("0"), "Zero quantity rejected")
        Reject(Sub() CommonHelper.Nonnegative("-1", "Stock"), "Negative stock rejected")
        Check(AdminService.CanTransition("Pending", "Confirmed"), "Valid order transition")
        Check(AdminService.CanTransition("Packed", "Cancelled"), "Pre-dispatch cancellation allowed")
        Check(Not AdminService.CanTransition("Delivered", "Cancelled"), "Delivered order is terminal")
        Check(Not AdminService.CanTransition("Cancelled", "Confirmed"), "Cancelled order is terminal")
        Check(Not AdminService.CanTransition("Pending", "Delivered"), "Skipping order stages rejected")
    End Sub
    Private Sub TestShopping()
        Dim password = "Test-" & Guid.NewGuid().ToString("N")
        SecurityHelper.Register("Test Customer", "customer@example.test", "9876543210", password, password, "Test Street", "Ahmedabad")
        Dim userId = CInt(DatabaseHelper.ExecuteScalar("SELECT [UserID] FROM [Users] WHERE [Email]=?", DatabaseHelper.P("customer@example.test")))
        Reject(Sub() SecurityHelper.Register("Duplicate", "CUSTOMER@example.test", "9876543210", password, password, "Test Street", "Ahmedabad"), "Duplicate normalized email rejected")
        Check(StoreService.Products("apples", 0).Rows.Count = 1, "Name search works")
        Check(StoreService.Products("Orchard", 0).Rows.Count = 2, "Brand search works")
        Check(StoreService.Products("' OR 1=1 --", 0).Rows.Count = 0, "SQL-like search treated literally")
        Check(StoreService.Products("", 1).Rows.Count = 2, "Category filtering works")
        Dim productId = CInt(StoreService.Products("apples", 0).Rows(0)("ProductID"))
        Dim initialStock = CInt(StoreService.Product(productId)("StockQuantity"))
        StoreService.AddToCart(userId, productId, 1)
        StoreService.AddToCart(userId, productId, 2)
        Dim cart = StoreService.Cart(userId)
        Check(cart.Rows.Count = 1 AndAlso CInt(cart.Rows(0)("Quantity")) = 3, "Duplicate add merges into one cart row")
        Check(StoreService.Total(cart) = 540D, "Cart total calculated from price and quantity")
        Reject(Sub() StoreService.AddToCart(userId, productId, initialStock), "Combined quantity cannot exceed stock")
        Reject(Sub() StoreService.UpdateCart(userId, CInt(cart.Rows(0)("CartID")), 0), "Zero cart quantity rejected")
        StoreService.UpdateCart(userId, CInt(cart.Rows(0)("CartID")), 2)
        Check(StoreService.Total(StoreService.Cart(userId)) = 360D, "Quantity update recalculates total")
        Dim token = Guid.NewGuid().ToString("N")
        Reject(Sub() StoreService.Checkout(userId, "Test Street", "Ahmedabad", "Online Payment", token), "Unsupported payment rejected")
        DatabaseHelper.ExecuteNonQuery("UPDATE [Products] SET [StockQuantity]=1 WHERE [ProductID]=?", I(productId))
        Reject(Sub() StoreService.Checkout(userId, "Test Street", "Ahmedabad", "Cash on Delivery", token), "Checkout rechecks changed stock")
        Check(StoreService.Orders(userId).Rows.Count = 0 AndAlso StoreService.Cart(userId).Rows.Count = 1, "Failed checkout retains cart and creates no order")
        DatabaseHelper.ExecuteNonQuery("UPDATE [Products] SET [StockQuantity]=? WHERE [ProductID]=?", I(initialStock), I(productId))
        Using db As New DbSession(True)
            db.Execute("UPDATE [Products] SET [StockQuantity]=0 WHERE [ProductID]=?", I(productId))
            ' No Commit: Dispose must roll back.
        End Using
        Check(CInt(StoreService.Product(productId)("StockQuantity")) = initialStock, "Uncommitted database transaction rolls back")
        Dim orderId = StoreService.Checkout(userId, "Test Street", "Ahmedabad", "Cash on Delivery", token)
        Check(StoreService.Cart(userId).Rows.Count = 0, "Successful checkout clears cart")
        Check(CInt(StoreService.Product(productId)("StockQuantity")) = initialStock - 2, "Successful checkout decrements stock")
        Check(CDec(StoreService.Order(userId, orderId)("TotalAmount")) = 360D, "Order total correct")
        Check(StoreService.Checkout(userId, "Test Street", "Ahmedabad", "Cash on Delivery", token) = orderId, "Checkout replay returns same order")
        Check(StoreService.Orders(userId).Rows.Count = 1, "Checkout replay creates no duplicate order")
        DatabaseHelper.ExecuteNonQuery("UPDATE [Products] SET [Price]=999,[ProductName]='Renamed apples' WHERE [ProductID]=?", I(productId))
        Dim items = StoreService.OrderItems(userId, orderId)
        Check(CDec(items.Rows(0)("Price")) = 180D AndAlso CStr(items.Rows(0)("ProductName")) = "Fresh apples", "Historical price and name preserved")
        SecurityHelper.Register("Other Customer", "other@example.test", "9876543211", password, password, "Other Street", "Ahmedabad")
        Dim otherId = CInt(DatabaseHelper.ExecuteScalar("SELECT [UserID] FROM [Users] WHERE [Email]=?", DatabaseHelper.P("other@example.test")))
        Reject(Sub() StoreService.Order(otherId, orderId), "Order ownership enforced")
        Check(StoreService.OrderItems(otherId, orderId).Rows.Count = 0, "Other customer's order items hidden")
        Reject(Sub() StoreService.Checkout(otherId, "Test Street", "Ahmedabad", "Cash on Delivery", Guid.NewGuid().ToString("N")), "Empty checkout rejected")
        DatabaseHelper.ExecuteNonQuery("UPDATE [Products] SET [Status]='Inactive' WHERE [ProductID]=?", I(productId))
        Reject(Sub() StoreService.AddToCart(userId, productId, 1), "Inactive products cannot be added")
        DatabaseHelper.ExecuteNonQuery("UPDATE [Products] SET [Status]='Active',[StockQuantity]=1 WHERE [ProductID]=?", I(productId))
        StoreService.AddToCart(userId, productId, 1)
        StoreService.AddToCart(otherId, productId, 1)
        Dim tasks = New Task(Of Boolean)() {
            Task.Run(Function() TryCheckout(userId)),
            Task.Run(Function() TryCheckout(otherId))
        }
        Task.WaitAll(tasks)
        Check(tasks.Count(Function(t) t.Result) = 1, "Only one customer purchases the last unit concurrently")
        Check(CInt(StoreService.Product(productId)("StockQuantity")) = 0, "Concurrent checkout cannot oversell")
    End Sub
    Private Function TryCheckout(userId As Integer) As Boolean
        Try
            StoreService.Checkout(userId, "Test Street", "Ahmedabad", "Cash on Delivery", Guid.NewGuid().ToString("N"))
            Return True
        Catch ex As BusinessException
            Return False
        Catch ex As OleDbException
            ' ACE can reject one contender with a lock conflict instead of waiting.
            Return False
        End Try
    End Function
End Module
