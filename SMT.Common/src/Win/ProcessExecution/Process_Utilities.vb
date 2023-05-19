
Namespace Win.ProcessExecution

    Public Module Utilites

        Public Function RunCommandLine(ByVal Cmd As String, ByVal Params As String) As Boolean
            Try
                System.Diagnostics.Process.Start(Cmd, Params)
                Return True
            Catch ex As Exception
                Throw New Exception("problem with runcommandline. error: " & ex.Message)
                Return False
            End Try
        End Function

    End Module

End Namespace
