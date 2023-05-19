Imports System.Drawing

Namespace Multimedia.Formats.StillImage

    Public Module mImageScaling

        Public Function ScaleImage(ByVal InBM As Bitmap, ByVal NewHeight As Short, ByVal NewWidth As Short) As Bitmap
            Try
                Dim Out As New Bitmap(NewWidth, NewHeight, InBM.PixelFormat)
                Dim gr As Graphics = Graphics.FromImage(Out)
                gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                gr.DrawImage(InBM, New Rectangle(0, 0, Out.Width, Out.Height), New Rectangle(0, 0, InBM.Width, InBM.Height), GraphicsUnit.Pixel)
                Return Out

                'Dim bm_dest As New Bitmap(NewWidth, NewHeight)
                'Dim gr_dest As Graphics = Graphics.FromImage(bm_dest)
                'gr_dest.DrawImage(InBM, 0, 0, bm_dest.Width + 1, bm_dest.Height + 1)
                'Return bm_dest
            Catch ex As Exception
                Throw New Exception("Problem scaling image. Error: " & ex.Message)
            End Try
        End Function

    End Module

End Namespace
