Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Globalization
Imports System.Web

Public Class BusinessException
    Inherits Exception
    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class

Public NotInheritable Class CommonHelper
    Public Shared Function Required(value As String, label As String, max As Integer) As String
        value = If(value, "").Trim()
        If value.Length = 0 OrElse value.Length > max Then Throw New BusinessException(label & " is required (maximum " & max & " characters).")
        Return value
    End Function
    Public Shared Function PositiveId(value As String) As Integer
        Dim id As Integer
        If Not Integer.TryParse(value, id) OrElse id < 1 Then Throw New BusinessException("Invalid record or quantity.")
        Return id
    End Function
    Public Shared Function Nonnegative(value As String, label As String) As Integer
        Dim n As Integer
        If Not Integer.TryParse(value, n) OrElse n < 0 OrElse n > 1000000 Then Throw New BusinessException(label & " must be between 0 and 1,000,000.")
        Return n
    End Function
    Public Shared Function Price(value As String) As Decimal
        Dim n As Decimal
        If Not Decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, n) OrElse n < 0D OrElse n > 1000000D OrElse Decimal.Round(n, 2) <> n Then Throw New BusinessException("Price must be between 0 and 1,000,000 with at most two decimal places.")
        Return n
    End Function
    Public Shared Function FormatCurrency(value As Object) As String
        Return Convert.ToDecimal(If(Convert.IsDBNull(value), 0D, value)).ToString("C2", CultureInfo.GetCultureInfo("en-IN"))
    End Function
    Public Shared Function ProductImage(value As Object) As String
        Dim path = Convert.ToString(value)
        If System.Text.RegularExpressions.Regex.IsMatch(path, "\A~/Images/Products/[a-zA-Z0-9_-]+\.(png|jpg|jpeg|webp)\z", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then Return VirtualPathUtility.ToAbsolute(path)
        Return VirtualPathUtility.ToAbsolute("~/Content/grocery.svg")
    End Function
    Public Shared Function Status(value As String) As String
        If value <> "Active" AndAlso value <> "Inactive" Then Throw New BusinessException("Invalid status.")
        Return value
    End Function
End Class
