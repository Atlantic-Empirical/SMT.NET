Imports System.IO
Imports SMT.DotNet.Utility

Namespace Multimedia.Formats.StillImage

    Public Module mImageUtilties

        Private Function RasterBufferToBMP(ByVal MS As MemoryStream, ByVal SampH As Short) As Byte()
            Dim OutputFileSize As Integer = MS.Length + 54

            'Read raster data buffer from memory
            Dim Buffer(MS.Length - 1) As Byte
            'Debug.WriteLine("FG Source: " & SamplePtr.ToInt32)
            MS.Read(Buffer, 0, MS.Length)

            ''DEBUGGING - Save the buffer
            'FS = New FileStream("C:\Test\Buffer.bin", FileMode.OpenOrCreate)
            'FS.Write(Buffer, 0, Buffer.Length)
            'FS.Close()
            'FS = Nothing
            ''END DEBUGGING

            'Flip Image
            Buffer = FlipImageBuffer_Vertically(Buffer)
            Buffer = FlipRGB24ImageBuffer_Horizontally(Buffer, 720, SampH)

            'Make BMI header
            Dim BMI(53) As Byte
            BMI(0) = 66 'B
            BMI(1) = 77 'M

            Dim TmpBuff() As Byte

            'FileSize
            TmpBuff = ConvertDecimalIntoByteArray(OutputFileSize, 4)
            BMI(2) = TmpBuff(0)
            BMI(3) = TmpBuff(1)
            BMI(4) = TmpBuff(2)
            BMI(5) = TmpBuff(3)

            'Old way
            'Dim HexSize As String = Hex(OutputFileSize)
            'BMI(2) = CInt("&H" & Microsoft.VisualBasic.Right(HexSize, 2))
            'BMI(3) = CInt("&H" & Microsoft.VisualBasic.Left(Mid(HexSize, 2), 2))
            'BMI(4) = CInt("&H" & Microsoft.VisualBasic.Left(HexSize, 1))

            BMI(10) = 54 'Header size
            BMI(14) = 40 'InfoHeader size

            'Width
            TmpBuff = ConvertDecimalIntoByteArray(720, 4)
            BMI(18) = TmpBuff(0)
            BMI(19) = TmpBuff(1)
            BMI(20) = TmpBuff(2)
            BMI(21) = TmpBuff(3)

            'Height
            TmpBuff = ConvertDecimalIntoByteArray(SampH, 4)
            BMI(22) = TmpBuff(0)
            BMI(23) = TmpBuff(1)
            BMI(24) = TmpBuff(2)
            BMI(25) = TmpBuff(3)

            BMI(26) = 1 'Planes
            BMI(28) = 24 'Bit depth

            BMI(38) = 196
            BMI(39) = 14
            BMI(42) = 196
            BMI(43) = 14

            Dim SampleBitmapBuffer(OutputFileSize) As Byte
            Array.Copy(BMI, 0, SampleBitmapBuffer, 0, 54)
            Array.Copy(Buffer, 0, SampleBitmapBuffer, 54, Buffer.Length)

            'Debugging - Dump the bitmap
            'FS = New FileStream("C:\Temp_" & DateTime.Now.Ticks & ".bmp", FileMode.OpenOrCreate)
            'FS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
            'FS.Close()
            'FS = Nothing
            'End Debugging

            Buffer = Nothing
            BMI = Nothing
            Return SampleBitmapBuffer
        End Function

    End Module

End Namespace
