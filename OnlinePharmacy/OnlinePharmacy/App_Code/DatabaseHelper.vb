Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb

Public Class DatabaseHelper
    Public Shared Function GetConnectionString() As String
        Return ConfigurationManager.ConnectionStrings("PharmacyDB").ConnectionString
    End Function

    Public Shared Function OpenConnection() As OleDbConnection
        Dim conn As New OleDbConnection(GetConnectionString())
        conn.Open()
        Return conn
    End Function

    Public Shared Function P(value As Object) As OleDbParameter
        Dim param As New OleDbParameter()
        If TypeOf value Is DateTime Then
            param.OleDbType = OleDbType.Date
        ElseIf TypeOf value Is Decimal OrElse TypeOf value Is Double Then
            param.OleDbType = OleDbType.Decimal
        ElseIf TypeOf value Is Integer Then
            param.OleDbType = OleDbType.Integer
        End If
        param.Value = If(value Is Nothing, DBNull.Value, value)
        Return param
    End Function

    Public Shared Function ExecuteNonQuery(sql As String, ParamArray parameters() As OleDbParameter) As Integer
        Using conn As OleDbConnection = OpenConnection()
            Using cmd As New OleDbCommand(sql, conn)
                If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                    cmd.Parameters.AddRange(parameters)
                End If
                Return cmd.ExecuteNonQuery()
            End Using
        End Using
    End Function

    Public Shared Function ExecuteScalar(sql As String, ParamArray parameters() As OleDbParameter) As Object
        Using conn As OleDbConnection = OpenConnection()
            Using cmd As New OleDbCommand(sql, conn)
                If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                    cmd.Parameters.AddRange(parameters)
                End If
                Return cmd.ExecuteScalar()
            End Using
        End Using
    End Function

    Public Shared Function FillDataTable(sql As String, ParamArray parameters() As OleDbParameter) As DataTable
        Using conn As OleDbConnection = OpenConnection()
            Using cmd As New OleDbCommand(sql, conn)
                If parameters IsNot Nothing AndAlso parameters.Length > 0 Then
                    cmd.Parameters.AddRange(parameters)
                End If
                Using adapter As New OleDbDataAdapter(cmd)
                    Dim dt As New DataTable()
                    adapter.Fill(dt)
                    Return dt
                End Using
            End Using
        End Using
    End Function
End Class