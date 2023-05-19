Imports System.Windows.Forms
Imports System.Windows.Media
Imports System.Windows.Interop
Imports System.Windows

Namespace Multimedia.GraphConstruction

    Public Class cSMTForm

        Public Top, Left, Width, Height As Integer
        Public Property Handle() As IntPtr
            Get
                If IsWPF Then
                    If FormWPF Is Nothing Then Return IntPtr.Zero
                    If _Handle = Nothing Then
                        _Handle = New WindowInteropHelper(FormWPF).Handle
                    End If
                    Return _Handle
                Else
                    Return _Handle
                End If
            End Get
            Set(ByVal value As IntPtr)
                _Handle = value
            End Set
        End Property
        Private _Handle As IntPtr

        Public Icon As System.Drawing.Icon
        Public WindowsForm As Form

        Public IsWPF As Boolean = False
        Public FormWPF As Window
        Public IconWPF As ImageSource

        Public Sub New()
        End Sub

        Public Sub New(ByRef nForm As System.Windows.Forms.Form)
            Me.Top = nForm.Top
            Me.Left = nForm.Left
            Me.Width = nForm.Width
            Me.Height = nForm.Height
            Me.Icon = nForm.Icon
            Me.Handle = nForm.Handle
            WindowsForm = nForm
        End Sub

        Public Sub New(ByRef nForm As System.Windows.Window)
            IsWPF = True
            Me.Top = nForm.Top
            Me.Left = nForm.Left
            Me.Width = nForm.Width
            Me.Height = nForm.Height
            Me.IconWPF = nForm.Icon
            FormWPF = nForm
        End Sub

        Public Sub New(ByVal Loc As System.Drawing.Point, ByVal Sz As System.Drawing.Size, ByVal Hndle As IntPtr, ByVal Icn As System.Drawing.Icon)
            Me.Top = Loc.Y
            Me.Left = Loc.X
            Me.Width = Sz.Width
            Me.Height = Sz.Height
            Me.Icon = Icn
            Me.Handle = Hndle
        End Sub

    End Class

End Namespace
