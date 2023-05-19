
Namespace DotNet.Exceptions

    Public Module mExceptions

        Public Function GetExLineNumber(ByVal e As Exception) As String
            Try
                Dim s() As String = Split(e.StackTrace, ":line ")
                Return s(1)
            Catch ex As Exception
                Throw New Exception("Problem with GetExLineNumber(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Module

End Namespace
