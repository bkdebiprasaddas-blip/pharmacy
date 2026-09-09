Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_ManageCategoriesPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        CategoriesGrid.DataSource = StoreService.Categories(False)
        CategoriesGrid.DataBind()
    End Sub
    Protected Sub CategoryCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName <> "LoadCategory" AndAlso e.CommandName <> "RemoveCategory" Then Return
        Run(Sub()
                Dim id = CommonHelper.PositiveId(CStr(e.CommandArgument))
                If e.CommandName = "RemoveCategory" Then
                    AdminService.DeleteCategory(id)
                    ResetEditor()
                    BindData()
                    Notice = "Category deleted."
                Else
                    Dim rows = DatabaseHelper.FillDataTable("SELECT * FROM [Categories] WHERE [CategoryID]=?", DatabaseHelper.P(id, OleDbType.Integer))
                    If rows.Rows.Count <> 1 Then Throw New BusinessException("Category not found.")
                    Dim row = rows.Rows(0)
                    ViewState("CategoryID") = id
                    CategoryName.Text = CStr(row("CategoryName"))
                    Description.Text = Convert.ToString(row("Description"))
                    Status.SelectedValue = CStr(row("Status"))
                    EditorTitle.Text = "Edit category #" & id.ToString()
                End If
            End Sub)
    End Sub
    Protected Sub SaveClick(sender As Object, e As EventArgs)
        If Not Page.IsValid Then Return
        Run(Sub()
                AdminService.SaveCategory(Convert.ToInt32(ViewState("CategoryID")), CategoryName.Text, Description.Text, Status.SelectedValue)
                ResetEditor()
                BindData()
                Notice = "Category saved."
            End Sub)
    End Sub
    Private Sub ResetEditor()
        ViewState("CategoryID") = 0
        CategoryName.Text = ""
        Description.Text = ""
        Status.SelectedValue = "Active"
        EditorTitle.Text = "Add category"
    End Sub
    Protected Sub ResetClick(sender As Object, e As EventArgs)
        ResetEditor()
    End Sub

End Class
