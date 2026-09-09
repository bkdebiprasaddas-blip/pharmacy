Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb

Public NotInheritable Class DatabaseHelper
    Public Shared Function OpenConnection() As OleDbConnection
        Dim c As New OleDbConnection(ConfigurationManager.ConnectionStrings("GroceryDB").ConnectionString)
        Try
            c.Open()
            Return c
        Catch
            c.Dispose()
            Throw
        End Try
    End Function

    ' OleDb ignores parameter names. Callers MUST pass parameters in SQL placeholder order.
    Public Shared Function P(value As Object, Optional kind As OleDbType = OleDbType.VarWChar) As OleDbParameter
        Return New OleDbParameter With {.OleDbType = kind, .Value = If(value, DBNull.Value)}
    End Function

    Public Shared Function FillDataTable(sql As String, ParamArray args As OleDbParameter()) As DataTable
        Using db As New DbSession()
            Return db.Table(sql, args)
        End Using
    End Function

    Public Shared Function ExecuteScalar(sql As String, ParamArray args As OleDbParameter()) As Object
        Using db As New DbSession()
            Return db.Scalar(sql, args)
        End Using
    End Function

    Public Shared Function ExecuteNonQuery(sql As String, ParamArray args As OleDbParameter()) As Integer
        Using db As New DbSession()
            Return db.Execute(sql, args)
        End Using
    End Function
End Class

Public NotInheritable Class DbSession
    Implements IDisposable
    Private ReadOnly connection As OleDbConnection
    Private ReadOnly transaction As OleDbTransaction
    Private committed As Boolean

    Public Sub New(Optional transactional As Boolean = False)
        connection = DatabaseHelper.OpenConnection()
        If transactional Then
            Try
                transaction = connection.BeginTransaction(IsolationLevel.Serializable)
            Catch
                connection.Dispose()
                Throw
            End Try
        End If
    End Sub

    Private Function Command(sql As String, args As OleDbParameter()) As OleDbCommand
        Dim cmd As New OleDbCommand(sql, connection, transaction)
        cmd.Parameters.AddRange(args)
        Return cmd
    End Function

    Public Function Table(sql As String, ParamArray args As OleDbParameter()) As DataTable
        Using cmd = Command(sql, args), adapter As New OleDbDataAdapter(cmd)
            Dim result As New DataTable()
            adapter.Fill(result)
            Return result
        End Using
    End Function

    Public Function Scalar(sql As String, ParamArray args As OleDbParameter()) As Object
        Using cmd = Command(sql, args)
            Return cmd.ExecuteScalar()
        End Using
    End Function

    Public Function Execute(sql As String, ParamArray args As OleDbParameter()) As Integer
        Using cmd = Command(sql, args)
            Return cmd.ExecuteNonQuery()
        End Using
    End Function

    Public Function Identity() As Integer
        Return Convert.ToInt32(Scalar("SELECT @@IDENTITY"))
    End Function

    Public Sub Commit()
        transaction.Commit()
        committed = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If transaction IsNot Nothing Then
            If Not committed Then
                Try
                    transaction.Rollback()
                Catch ex As OleDbException
                    System.Diagnostics.Trace.TraceError(ex.ToString())
                End Try
            End If
            transaction.Dispose()
        End If
        connection.Dispose()
    End Sub
End Class
