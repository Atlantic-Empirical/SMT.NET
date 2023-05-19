Imports System.Drawing

Namespace Multimedia.Formats.StillImage

    Public Module mImageHandlingFunctions

        Public Sub ImageResize(ByVal in_file As String, ByVal out_file As String, ByVal new_wid As Integer, ByVal new_hgt As Integer, ByVal output_format As System.Drawing.Imaging.ImageFormat, Optional ByVal interpolation_mode As System.Drawing.Drawing2D.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic)
            Try
                Dim in_bm As New Bitmap(in_file)

                Dim out_bm As New Bitmap(new_wid, new_hgt, in_bm.PixelFormat)

                ' Resize the image.
                Dim gr As Graphics = Graphics.FromImage(out_bm)
                gr.InterpolationMode = interpolation_mode
                gr.DrawImage(in_bm, New Rectangle(0, 0, out_bm.Width, out_bm.Height), New Rectangle(0, 0, in_bm.Width, in_bm.Height), GraphicsUnit.Pixel)

                ' Save the results.
                out_bm.Save(out_file, output_format)

                ' Clean up.
                in_bm.Dispose()
                gr.Dispose()
                out_bm.Dispose()
            Catch ex As Exception
                Throw New Exception("Problem with ImageResize. Error: " & ex.Message)
            End Try
        End Sub

        Public Function ImageResize(ByVal InBM As Bitmap, ByVal NewHeight As Short, ByVal NewWidth As Short) As Bitmap
            Try
                Dim Out As New Bitmap(NewWidth, NewHeight, InBM.PixelFormat)
                Dim gr As Graphics = Graphics.FromImage(Out)
                gr.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                gr.DrawImage(InBM, New Rectangle(0, 0, Out.Width, Out.Height), New Rectangle(0, 0, InBM.Width, InBM.Height), GraphicsUnit.Pixel)
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with ImageResize. Error: " & ex.Message)
            End Try
        End Function

        Public Sub ImageCrop(ByVal in_file As String, ByVal out_file As String, ByVal new_wid As Integer, ByVal new_hgt As Integer, ByVal output_format As System.Drawing.Imaging.ImageFormat)
            ' Load in_file.
            Dim in_bm As New Bitmap(in_file)

            ' Make the output file.
            Dim out_bm As New Bitmap(new_wid, new_hgt, in_bm.PixelFormat)

            ' See which part of the input image we want.
            Dim x As Integer = (in_bm.Width - new_wid) \ 2
            Dim y As Integer = (in_bm.Height - new_hgt) \ 2

            ' Copy the part of the image we want.
            Dim gr As Graphics = Graphics.FromImage(out_bm)
            gr.DrawImage(in_bm, New Rectangle(0, 0, new_wid, new_hgt), New Rectangle(x, y, new_wid, new_hgt), GraphicsUnit.Pixel)

            ' Save the results.
            out_bm.Save(out_file, output_format)

            ' Clean up.
            in_bm.Dispose()
            gr.Dispose()
            out_bm.Dispose()
        End Sub

        Public Function ImageCrop(ByVal in_bm As Bitmap, ByVal new_wid As Integer, ByVal new_hgt As Integer) As Bitmap
            ' Make the output file.
            Dim out_bm As New Bitmap(new_wid, new_hgt, in_bm.PixelFormat)

            ' See which part of the input image we want.
            Dim x As Integer = (in_bm.Width - new_wid) \ 2
            Dim y As Integer = (in_bm.Height - new_hgt) \ 2

            ' Copy the part of the image we want.
            Dim gr As Graphics = Graphics.FromImage(out_bm)
            gr.DrawImage(in_bm, New Rectangle(0, 0, new_wid, new_hgt), New Rectangle(x, y, new_wid, new_hgt), GraphicsUnit.Pixel)
            Return out_bm

            '' Clean up.
            'in_bm.Dispose()
            'gr.Dispose()
            'out_bm.Dispose()
        End Function

    End Module 'ImageHandlingFunctions

End Namespace 'ImageHandling
