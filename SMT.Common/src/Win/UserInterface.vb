Imports SMT.Win.WinAPI.User32
Imports System.Drawing
Imports System.Windows.Forms
Imports SMT.Win.WinAPI.Constants

Namespace Win

    Public Module UserInterface

        Private Const MF_BYPOSITION = &H400&

        Public Sub DisableFormClose(ByVal FormHandle As Integer)
            Try
                Dim hMenu As Integer = GetSystemMenu(FormHandle, False)
                DeleteMenu(hMenu, 6, MF_BYPOSITION)
            Catch ex As Exception
                Throw New Exception("Problem with DisableClose. Error: " & ex.Message)
            End Try
        End Sub

        Public Sub MakeWindowPerminantTopMost(ByVal hWnd As Integer)
            'To make a window like the viewer in phoenix - perminant topmost
            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOACTIVATE Or SWP_SHOWWINDOW Or SWP_NOMOVE Or SWP_NOSIZE)
        End Sub

#Region "ProgressBar Helper"

        Public Class cProgressBarHelper

            Private Const WM_USER As Int32 = &H400

            Private Const CCM_FIRST As Int32 = &H2000
            Private Const CCM_SETBKCOLOR As Int32 = (CCM_FIRST + &H1)

            Private Const PBM_SETBARCOLOR As Int32 = (WM_USER + 9)
            Private Const PBM_SETBKCOLOR As Int32 = CCM_SETBKCOLOR

            Private Const CLR_DEFAULT As Int32 = &HFF000000I

            Private m_ProgressBar As ProgressBar
            Private m_BackColor As Color
            Private m_ForeColor As Color
            Private m_OldBackColor As Color
            Private m_OldForeColor As Color

            Public Sub New(ByVal ProgressBar As ProgressBar)
                If ProgressBar Is Nothing Then
                    Throw New NullReferenceException
                End If
                m_ProgressBar = ProgressBar

                ' Reset colors to make calls to property gets for 'ForeColor' and
                ' 'BackColor' return the right value.
                Me.ForeColor = Me.DefaultColor
                Me.BackColor = Me.DefaultColor
            End Sub

            Public ReadOnly Property ProgressBar() As ProgressBar
                Get
                    Return m_ProgressBar
                End Get
            End Property

            Public Property ForeColor() As Color
                Get
                    Return m_ForeColor
                End Get
                Set(ByVal Value As Color)
                    m_ForeColor = Value
                    SetBarColor(Me.ForeColor)
                End Set
            End Property

            Public Property BackColor() As Color
                Get
                    Return m_BackColor
                End Get
                Set(ByVal Value As Color)
                    m_BackColor = Value
                    SetBkColor(Me.BackColor)
                End Set
            End Property

            Public Shared ReadOnly Property DefaultColor() As Color
                Get
                    Return Color.Transparent
                End Get
            End Property

            Private Function SetBarColor(ByVal Color As Color) As Color
                Return ProgressBarColorToDotNetColor(SendMessage(m_ProgressBar.Handle, PBM_SETBARCOLOR, 0, DotNetColorToProgressBarColor(Color)))
            End Function

            Private Function SetBkColor(ByVal Color As Color) As Color
                Return ProgressBarColorToDotNetColor(SendMessage(m_ProgressBar.Handle, PBM_SETBKCOLOR, 0, DotNetColorToProgressBarColor(Color)))
            End Function

            Private Function DotNetColorToProgressBarColor(ByVal Color As Color) As Int32
                If Color.Equals(Me.DefaultColor) Then
                    Return CLR_DEFAULT
                Else
                    Return ColorTranslator.ToWin32(Color)
                End If
            End Function

            Private Function ProgressBarColorToDotNetColor(ByVal Color As Int32) As Color
                If Color = CLR_DEFAULT Then
                    Return Me.DefaultColor
                Else
                    Return ColorTranslator.FromWin32(Color)
                End If
            End Function

        End Class

#End Region     'ProgressBar Helper

        Public Function Desktop_H(ByRef frm As Form) As Short
            Return Screen.GetBounds(frm).Height
        End Function

        Public Function Desktop_W(ByRef frm As Form) As Short
            Return Screen.GetBounds(frm).Width
        End Function

#Region "ScreenSaver"

        Public Function DisableScreensaver() As Boolean
            Try
                Dim lRetval As Integer = SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 0, 0, 0)
                If lRetval > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Throw New Exception("Problem with DisableScreensaver. Error: " & ex.Message)
            End Try
        End Function

        Public Function EnableScreensaver() As Boolean
            Try
                Dim lRetval As Integer = SystemParametersInfo(SPI_SETSCREENSAVEACTIVE, 1, 0, 0)
                If lRetval > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Throw New Exception("Problem with EnableScreensaver. Error: " & ex.Message)
            End Try
        End Function

#End Region 'ScreenSaver

        Public Function FindControl(ByVal ControlName As String, ByVal CurrentControl As Control) As Control
            For Each ctr As Control In CurrentControl.Controls
                If ctr.Name = ControlName Then
                    Return ctr
                Else
                    ctr = FindControl(ControlName, ctr)
                    If Not ctr Is Nothing Then
                        Return ctr
                    End If
                End If
            Next ctr
            Return Nothing
        End Function

    End Module

End Namespace
