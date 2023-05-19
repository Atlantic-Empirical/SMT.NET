Imports System.Drawing

Namespace Multimedia.Utility

    Public Module mNTCSColor

        Public Function ConformToNTSCColor(ByVal value As Color) As Color
            Try
                If value.R > 235 Or value.G > 235 Or value.B > 235 Or value.R < 16 Or value.G < 16 Or value.B < 16 Then
                    If value.R > 235 Then value.FromArgb(235, value.G, value.B)
                    If value.G > 235 Then value.FromArgb(value.R, 235, value.B)
                    If value.B > 235 Then value.FromArgb(value.R, value.G, 235)
                    If value.R < 16 Then value.FromArgb(16, value.G, value.B)
                    If value.G < 16 Then value.FromArgb(value.R, 16, value.B)
                    If value.B < 16 Then value.FromArgb(value.R, value.G, 16)
                End If
                Return value
            Catch ex As Exception
                Throw New Exception("Problem with ConformToNTSCColor. Error: " & ex.Message, Nothing)
            End Try
        End Function

    End Module

End Namespace
