Imports System.Windows.Input

Namespace Multimedia.UI.WPF.Dialogs

    Partial Public Class PTS_Dialog

        Public Property PTS_Start() As UInt64
            Get
                Return Me.txtStartPTS.Text
            End Get
            Set(ByVal value As UInt64)
                Me.txtStartPTS.Text = value
            End Set
        End Property

        Public Property PTS_End() As UInt64
            Get
                Return Me.txtEndPTS.Text
            End Get
            Set(ByVal value As UInt64)
                Me.txtEndPTS.Text = value
            End Set
        End Property

        Public Sub New(ByRef nPTS_Start As UInt64, ByVal nPTS_End As UInt64)
            ' This call is required by the Windows Form Designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.
            PTS_Start = nPTS_Start
            PTS_End = nPTS_End
        End Sub

        Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOK.Click
            OK()
        End Sub

        Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnCancel.Click
            Cancel()
        End Sub

        Private Sub PTS_Dialog_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Input.KeyEventArgs) Handles Me.KeyUp
            Select Case e.Key
                Case Key.Enter
                    OK()
                Case Key.Escape
                    Cancel()
            End Select
        End Sub

        Private Sub Cancel()
            Me.DialogResult = False
            Me.Close()
        End Sub

        Private Sub OK()
            Me.DialogResult = True
            Me.Close()
        End Sub

    End Class

End Namespace
