Imports System.IO

Namespace Win.ProcessExecution

    Public Module RunningInstanceCheck

        Public Function IsProcessRunning(ByVal MyName As String) As Boolean
            Dim cnt As Byte
            'MyName = Microsoft.VisualBasic.Replace(MyName.ToLower, ".exe", "")
            For Each P As Process In Process.GetProcesses
                If InStr(P.ProcessName.ToLower, MyName.ToLower) Then
                    cnt += 1
                End If
            Next
            If cnt > 1 Then
                Return True
            Else
                Return False
            End If
        End Function

    End Module

End Namespace
