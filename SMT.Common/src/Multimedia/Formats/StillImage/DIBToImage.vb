Imports System
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Namespace Multimedia.Formats.StillImage

    Public Class DibToImage

        Public Shared Function WithStream(ByVal dibPtr As IntPtr) As Bitmap
            Dim fh As BITMAPFILEHEADER = New BITMAPFILEHEADER
            Dim bmiTyp As Type = GetType(BITMAPINFOHEADER)
            Dim bmi As BITMAPINFOHEADER = CType(Marshal.PtrToStructure(dibPtr, bmiTyp), BITMAPINFOHEADER)
            If bmi.biSizeImage = 0 Then
                bmi.biSizeImage = ((((bmi.biWidth * bmi.biBitCount) + 31) And Not 31) >> 3) * Math.Abs(bmi.biHeight)
            End If
            If (bmi.biClrUsed = 0) AndAlso (bmi.biBitCount < 16) Then
                bmi.biClrUsed = 1 << bmi.biBitCount
            End If
            Dim fhSize As Integer = Marshal.SizeOf(GetType(BITMAPFILEHEADER))
            Dim dibSize As Integer = bmi.biSize + (bmi.biClrUsed * 4) + bmi.biSizeImage
            fh.Type = New Char() {"B"c, "M"c}
            fh.Size = fhSize + dibSize
            fh.OffBits = fhSize + bmi.biSize + (bmi.biClrUsed * 4)
            Dim data(fh.Size) As Byte
            RawSerializeInto(fh, data)
            Marshal.Copy(dibPtr, data, fhSize, dibSize)
            Dim stream As MemoryStream = New MemoryStream(data)
            Dim tmp As Bitmap = New Bitmap(stream)
            Dim result As Bitmap = New Bitmap(tmp)
            tmp.Dispose()
            tmp = Nothing
            stream.Close()
            stream = Nothing
            data = Nothing
            Return result
        End Function

        Public Shared Function WithScan0(ByVal dibPtr As IntPtr) As Bitmap
            Dim bmiTyp As Type = GetType(BITMAPINFOHEADER)
            Dim bmi As BITMAPINFOHEADER = CType(Marshal.PtrToStructure(dibPtr, bmiTyp), BITMAPINFOHEADER)
            If Not (bmi.biCompression = 0) Then
                Throw New ArgumentException("Invalid bitmap format (non-RGB)", "BITMAPINFOHEADER.biCompression")
            End If
            Dim fmt As PixelFormat = PixelFormat.Undefined
            If bmi.biBitCount = 24 Then
                fmt = PixelFormat.Format24bppRgb
            Else
                If bmi.biBitCount = 32 Then
                    fmt = PixelFormat.Format32bppRgb
                Else
                    If bmi.biBitCount = 16 Then
                        fmt = PixelFormat.Format16bppRgb555
                    Else
                        Throw New ArgumentException("Invalid pixel depth (<16-Bits)", "BITMAPINFOHEADER.biBitCount")
                    End If
                End If
            End If
            Dim scan0 As Integer = dibPtr.ToInt32 + bmi.biSize + (bmi.biClrUsed * 4)
            Dim stride As Integer = (((bmi.biWidth * bmi.biBitCount) + 31) And Not 31) >> 3
            If bmi.biHeight > 0 Then
                scan0 += stride * (bmi.biHeight - 1)
                stride = -stride
            End If
            Dim tmp As Bitmap = New Bitmap(bmi.biWidth, Math.Abs(bmi.biHeight), stride, fmt, New IntPtr(scan0))
            Dim result As Bitmap = New Bitmap(tmp)
            tmp.Dispose()
            tmp = Nothing
            Return result
        End Function

        Public Shared Function WithHBitmap(ByVal dibPtr As IntPtr) As Bitmap
            Dim bmiTyp As Type = GetType(BITMAPINFOHEADER)
            Dim bmi As BITMAPINFOHEADER = CType(Marshal.PtrToStructure(dibPtr, bmiTyp), BITMAPINFOHEADER)
            If bmi.biSizeImage = 0 Then
                bmi.biSizeImage = ((((bmi.biWidth * bmi.biBitCount) + 31) And Not 31) >> 3) * Math.Abs(bmi.biHeight)
            End If
            If (bmi.biClrUsed = 0) AndAlso (bmi.biBitCount < 16) Then
                bmi.biClrUsed = 1 << bmi.biBitCount
            End If
            Dim pixPtr As IntPtr = New IntPtr(dibPtr.ToInt32 + bmi.biSize + (bmi.biClrUsed * 4))
            Dim img As IntPtr = IntPtr.Zero
            Dim st As Integer = GdipCreateBitmapFromGdiDib(dibPtr, pixPtr, img)
            If (Not (st = 0)) OrElse (img.ToInt32 = IntPtr.Zero.ToInt32) Then
                Throw New ArgumentException("Invalid bitmap for GDI+", "IntPtr dibPtr")
            End If
            Dim hbitmap As IntPtr
            st = GdipCreateHBITMAPFromBitmap(img, hbitmap, 0)
            If (Not (st = 0)) OrElse (hbitmap.ToInt32 = IntPtr.Zero.ToInt32) Then
                GdipDisposeImage(img)
                Throw New ArgumentException("can't get HBITMAP with GDI+", "IntPtr dibPtr")
            End If
            Dim tmp As Bitmap = Image.FromHbitmap(hbitmap)
            Dim result As Bitmap = New Bitmap(tmp)
            tmp.Dispose()
            tmp = Nothing
            Dim ok As Boolean = DeleteObject(hbitmap)
            hbitmap = IntPtr.Zero
            st = GdipDisposeImage(img)
            img = IntPtr.Zero
            Return result
        End Function

        Private Shared Sub RawSerializeInto(ByVal anything As Object, ByVal datas As Byte())
            Dim rawsize As Integer = Marshal.SizeOf(anything)
            If rawsize > datas.Length Then
                Throw New ArgumentException(" buffer too small ", " byte[] datas ")
            End If
            Dim handle As GCHandle = GCHandle.Alloc(datas, GCHandleType.Pinned)
            Dim buffer As IntPtr = handle.AddrOfPinnedObject
            Marshal.StructureToPtr(anything, buffer, False)
            handle.Free()
        End Sub

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Ansi, Pack:=1)> _
        Private Class BITMAPFILEHEADER
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)> _
            Public Type As Char()
            Public Size As Int32
            Public reserved1 As Int16
            Public reserved2 As Int16
            Public OffBits As Int32
        End Class

        <StructLayout(LayoutKind.Sequential, Pack:=2)> _
        Private Class BITMAPINFOHEADER
            Public biSize As Integer
            Public biWidth As Integer
            Public biHeight As Integer
            Public biPlanes As Short
            Public biBitCount As Short
            Public biCompression As Integer
            Public biSizeImage As Integer
            Public biXPelsPerMeter As Integer
            Public biYPelsPerMeter As Integer
            Public biClrUsed As Integer
            Public biClrImportant As Integer
        End Class

        <DllImport("gdi32.dll", ExactSpelling:=True)> _
        Private Shared Function DeleteObject(ByVal obj As IntPtr) As Boolean
        End Function

        <DllImport("gdiplus.dll", ExactSpelling:=True)> _
        Private Shared Function GdipCreateBitmapFromGdiDib(ByVal bminfo As IntPtr, ByVal pixdat As IntPtr, ByRef image As IntPtr) As Integer
        End Function

        <DllImport("gdiplus.dll", ExactSpelling:=True)> _
        Private Shared Function GdipCreateHBITMAPFromBitmap(ByVal image As IntPtr, ByRef hbitmap As IntPtr, ByVal bkg As Integer) As Integer
        End Function

        <DllImport("gdiplus.dll", ExactSpelling:=True)> _
        Private Shared Function GdipDisposeImage(ByVal image As IntPtr) As Integer
        End Function

    End Class

End Namespace