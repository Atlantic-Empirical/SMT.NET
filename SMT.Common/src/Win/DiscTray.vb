Imports SMT.Win.WinAPI.WinMM
Imports System.Runtime.InteropServices

Namespace Win

    Public Module DiscTray

        Public Sub OpenTray()
            Try
                Dim ReturnString As String
                Dim HR As Integer = mciSendString("set CDAudio door open", ReturnString, 127, 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem opening tray. Error: " & ex.Message)
            End Try
        End Sub

        Public Sub CloseTray()
            Try
                Dim ReturnString As String
                Dim HR As Integer = mciSendString("set CDAudio door closed", ReturnString, 127, 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem closing tray. Error: " & ex.Message)
            End Try
        End Sub

    End Module

End Namespace
