Imports System.Windows.Forms
Imports System.Drawing

Namespace DotNet.UserInterface.WindowsForms

    Public Class cDataGridNoActiveCellColumn
        Inherits DataGridTextBoxColumn

        Private SelectedRow As Integer
        'Fields
        'Constructors
        'Events
        'Methods
        Public Sub New()
            'Warning: Implementation not found
        End Sub
        Protected Overloads Overrides Sub Edit(ByVal source As CurrencyManager, ByVal rowNum As Integer, ByVal bounds As Rectangle, ByVal [readOnly] As Boolean, ByVal instantText As String, ByVal cellIsVisible As Boolean)
            'make sure selectedrow is valid
            If (SelectedRow > -1) And (SelectedRow < source.List.Count + 1) Then
                Me.DataGridTableStyle.DataGrid.UnSelect(SelectedRow)
            End If
            SelectedRow = rowNum
            Me.DataGridTableStyle.DataGrid.Select(SelectedRow)

        End Sub
    End Class

End Namespace

