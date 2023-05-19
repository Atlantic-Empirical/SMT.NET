
Namespace DotNet.Reflection

    Public Module Utilities

        Public Function GetFieldValue(ByRef SearchObject As Object, ByVal FieldName As String) As Object
            Try
                Dim info As System.Reflection.FieldInfo = SearchObject.GetType().GetField(FieldName.ToString, _
                System.Reflection.BindingFlags.NonPublic Or _
                System.Reflection.BindingFlags.Instance Or _
                System.Reflection.BindingFlags.Public Or _
                System.Reflection.BindingFlags.IgnoreCase)

                If info Is Nothing Then Throw New Exception("Field " & FieldName & " was not found.")
                Dim o As Object = info.GetValue(SearchObject)
                Return o
            Catch ex As Exception
                Throw New Exception("Problem with GetFieldValue(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetFieldNameValues() As SortedList(Of String, String)

        End Function

        Public Function GetExePath() As String
            Return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) & "\"
        End Function

    End Module

End Namespace
