Imports SMT.Win.ProcessExecution.Utilites

Namespace Win.Printing

    Public Module Printing

        Public Sub PrintTextFileWithNotepad(ByVal FilePath As String)
            Try
                Dim SYSTEM_FILES As String = Environment.GetFolderPath(Environment.SpecialFolder.System)
                RunCommandLine(SYSTEM_FILES & "\notepad.exe", "/p " & FilePath)
            Catch ex As Exception
                Throw New Exception("Problem with PrintTextFileWithNotepad(). Error: " & ex.Message)
            End Try
        End Sub

    End Module

End Namespace 'Utilities.Printing
