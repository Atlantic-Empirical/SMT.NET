Imports System.Drawing

Namespace Multimedia.Formats.StillImage

    Public Module mImageText

        Public Function DrawTextOnBMP(ByVal InBM As Bitmap, ByVal Str As String, ByVal Position As PointF, ByVal Size As Byte) As Bitmap
            Try
                Dim gr As Graphics = Graphics.FromImage(InBM)
                Dim F As New System.Drawing.Font("Arial", Size)
                gr.DrawString(Str, F, Brushes.Lime, Position)
                Return InBM
            Catch ex As Exception
                Throw New Exception("Problem TextOnBMP. Error: " & ex.Message)
            End Try
        End Function

    End Module

End Namespace
