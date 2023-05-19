Imports System.Data.SqlClient

Namespace Database.SQL

    Public Class SQLFunctions

        Public Function NewConnection(ByVal ServerAddress As String, ByVal UserID As String, ByVal Password As String, ByVal DatabaseName As String) As SqlConnection
            Dim SQLserver As String = ServerAddress
            Dim SQLuid As String = UserID
            Dim SQLpw As String = Password
            Dim SQLdb As String = DatabaseName
            Return New SqlConnection("Data Source=" & SQLserver & ";uid=" & SQLuid & ";pwd=" & SQLpw & "; Database='" & SQLdb & "'")
        End Function

    End Class 'SQLFunctions

End Namespace 'SQL
