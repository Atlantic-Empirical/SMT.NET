Imports System.Drawing
Imports System.IO
Imports System.Security.Permissions.FileIOPermission
Imports Microsoft.VisualBasic.Information
Imports System.Text

Namespace Multimedia.Formats.StillImage

    Public Module mBitmapEditor

        ''' <summary>
        ''' So this method takes a 24-bit bitmap and will use provided coordinates to change the color of 
        ''' sections of the image to the user-provided color. Pretty simple but this should be a pixel-accurate
        ''' algorithm (better then using the .NET bitmap classes because who knows what they'd do to the image.
        ''' </summary>
        ''' <param name="TwentyFourBitSubpicturePath"></param>
        ''' <param name="OutputPath"></param>
        ''' <param name="Input"></param>
        ''' <param name="NTSC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ToggleSubpictures(ByVal TwentyFourBitSubpicturePath As String, ByVal OutputPath As String, ByVal Input() As sSubpictureToggleInfo, ByVal NTSC As Boolean) As Boolean
            Try

                ' VID STD SETUP
                Dim W As Integer = 720
                Dim H As Integer = 0
                If NTSC Then
                    H = 480
                Else
                    H = 576
                End If

                ' READ DATA FROM FILE
                Dim FS As New FileStream(TwentyFourBitSubpicturePath, FileMode.Open)
                Dim buf(FS.Length - 1) As Byte
                FS.Read(buf, 0, FS.Length)
                FS.Close()

                ' CALCULATE BIH INFO AS NEEDED
                Dim RasterSize As Integer = H * W * 3

                ' COPY RASTER BYTES TO NEW ARRAY FOR MANIPULATION
                Dim RasterBytes(RasterSize - 1) As Byte
                Array.Copy(buf, 54, RasterBytes, 0, RasterSize)

                ' REVERSE THE ARRAY (makes math much easier later)
                Array.Reverse(RasterBytes)

                ' EXECUTE USER'S WISHES
                For Each TI As sSubpictureToggleInfo In Input

                    ' REALITY CHECK
                    If TI.X2 < TI.X1 Or TI.Y2 < TI.Y1 Then
                        Throw New Exception("Invalid coordinate value(s).")
                    End If

                    SetBitmapRegionColor(RasterBytes, TI)
                Next

                ' REVERSE THE ARRAY AGAIN SO THE IMAGE IS RIGHT-SIDE UP
                Array.Reverse(RasterBytes)

                ' RECONSTITUTE BITMAP
                Array.Copy(RasterBytes, 0, buf, 54, RasterSize)
                If File.Exists(OutputPath) Then File.Delete(OutputPath)
                FS = New FileStream(OutputPath, FileMode.Create)
                FS.Write(buf, 0, buf.Length)
                FS.Close()
                FS.Dispose()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with ToggleSubpictures(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' So this method takes a 24-bit bitmap and will use provided coordinates to change the color of 
        ''' sections of the image to the user-provided color. Pretty simple but this should be a pixel-accurate
        ''' algorithm (better then using the .NET bitmap classes because who knows what they'd do to the image.
        ''' </summary>
        ''' <param name="TwentyFourBitSubpicturePath"></param>
        ''' <param name="OutputPath"></param>
        ''' <param name="Input"></param>
        ''' <param name="NTSC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SwapTwoColors(ByVal TwentyFourBitSubpicturePath As String, ByVal OutputPath As String, ByVal Input As sTwoColorSwap, ByVal NTSC As Boolean) As Boolean
            Try

                ' VID STD SETUP
                Dim W As Integer = 720
                Dim H As Integer = 0
                If NTSC Then
                    H = 480
                Else
                    H = 576
                End If

                ' READ DATA FROM FILE
                Dim FS As New FileStream(TwentyFourBitSubpicturePath, FileMode.Open)
                Dim buf(FS.Length - 1) As Byte
                FS.Read(buf, 0, FS.Length)
                FS.Close()

                ' CALCULATE BIH INFO AS NEEDED
                Dim RasterSize As Integer = H * W * 3

                ' COPY RASTER BYTES TO NEW ARRAY FOR MANIPULATION
                Dim RasterBytes(RasterSize - 1) As Byte
                Array.Copy(buf, 54, RasterBytes, 0, RasterSize)

                ' REVERSE THE ARRAY (makes math much easier later)
                Array.Reverse(RasterBytes)

                ' REALITY CHECK
                If Input.X2 < Input.X1 Or Input.Y2 < Input.Y1 Then
                    Throw New Exception("Invalid coordinate value(s).")
                End If

                ' EXECUTE USER'S WISHES
                Dim iOC1 As Integer = RGB(Input.OldCol_1.R, Input.OldCol_1.G, Input.OldCol_1.B)
                Dim iOC2 As Integer = RGB(Input.OldCol_2.R, Input.OldCol_2.G, Input.OldCol_2.B)

                Dim aNewCol_1(2) As Byte 'RGB (due to the above array.reverse)
                aNewCol_1(0) = Input.NewCol_1.R
                aNewCol_1(1) = Input.NewCol_1.G
                aNewCol_1(2) = Input.NewCol_1.B

                Dim aNewCol_2(2) As Byte 'RGB (due to the above array.reverse)
                aNewCol_2(0) = Input.NewCol_2.R
                aNewCol_2(1) = Input.NewCol_2.G
                aNewCol_2(2) = Input.NewCol_2.B

                ' Dims for loop
                Dim iCC As Integer 'Current pixel's color
                'Dim aCC(3) As Byte
                'aCC(3) = 0
                Dim iCO As Integer 'Current offset

                'Dim SB As New StringBuilder

                For y As Integer = Input.Y1 To Input.Y2
                    For x As Integer = Input.X1 To Input.X2
                        'Array.Copy(buf, CalculateOffset(x, y), aCC, 0, 3)
                        'iCC = BitConverter.ToInt32(aCC, 0)
                        iCO = CalculateOffset(x, y)
                        iCC = RGB(RasterBytes(iCO), RasterBytes(iCO + 1), RasterBytes(iCO + 2))

                        'SB.Append(iCC & vbNewLine)

                        If iCC = iOC1 Then ' Check to see if this pixel is OC1
                            ' copy in new color
                            Array.Copy(aNewCol_1, 0, RasterBytes, iCO, 3)

                        ElseIf iCC = iOC2 Then ' Check to see if this pixel is OC2
                            ' copy in new color
                            Array.Copy(aNewCol_2, 0, RasterBytes, iCO, 3)

                        End If
                    Next
                Next

                'Debug.WriteLine(SB.ToString)

                ' REVERSE THE ARRAY AGAIN SO THE IMAGE IS RIGHT-SIDE UP
                Array.Reverse(RasterBytes)

                ' RECONSTITUTE BITMAP
                Array.Copy(RasterBytes, 0, buf, 54, RasterSize)
                If File.Exists(OutputPath) Then File.Delete(OutputPath)
                FS = New FileStream(OutputPath, FileMode.Create)
                FS.Write(buf, 0, buf.Length)
                FS.Close()
                FS.Dispose()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with ToggleSubpictures(). Error: " & ex.Message)
            End Try
        End Function

        Public Function SwapTwoColorsArray(ByVal TwentyFourBitSubpicturePath As String, ByVal OutputPath As String, ByVal Input() As sTwoColorSwap, ByVal NTSC As Boolean) As Boolean
            Try

                ' VID STD SETUP
                Dim W As Integer = 720
                Dim H As Integer = 0
                If NTSC Then
                    H = 480
                Else
                    H = 576
                End If

                ' READ DATA FROM FILE
                Dim FS As New FileStream(TwentyFourBitSubpicturePath, FileMode.Open)
                Dim buf(FS.Length - 1) As Byte
                FS.Read(buf, 0, FS.Length)
                FS.Close()

                ' CALCULATE BIH INFO AS NEEDED
                Dim RasterSize As Integer = H * W * 3

                ' COPY RASTER BYTES TO NEW ARRAY FOR MANIPULATION
                Dim RasterBytes(RasterSize - 1) As Byte
                Array.Copy(buf, 54, RasterBytes, 0, RasterSize)

                ' REVERSE THE ARRAY (makes math much easier later)
                Array.Reverse(RasterBytes)

                ' REALITY CHECK
                For Each TI As sTwoColorSwap In Input
                    If TI.X2 < TI.X1 Or TI.Y2 < TI.Y1 Then
                        Throw New Exception("Invalid coordinate value(s).")
                    End If
                Next

                ' EXECUTE USER'S WISHES
                For Each TI As sTwoColorSwap In Input

                    '    ' REALITY CHECK
                    '    If TI.X2 < TI.X1 Or TI.Y2 < TI.Y1 Then
                    '        Throw New Exception("Invalid coordinate value(s).")
                    '    End If

                    '    SetBitmapRegionColor(RasterBytes, TI)

                    Dim iOC1 As Integer = RGB(TI.OldCol_1.R, TI.OldCol_1.G, TI.OldCol_1.B)
                    Dim iOC2 As Integer = RGB(TI.OldCol_2.R, TI.OldCol_2.G, TI.OldCol_2.B)

                    Dim aNewCol_1(2) As Byte 'RGB (due to the above array.reverse)
                    aNewCol_1(0) = TI.NewCol_1.R
                    aNewCol_1(1) = TI.NewCol_1.G
                    aNewCol_1(2) = TI.NewCol_1.B

                    Dim aNewCol_2(2) As Byte 'RGB (due to the above array.reverse)
                    aNewCol_2(0) = TI.NewCol_2.R
                    aNewCol_2(1) = TI.NewCol_2.G
                    aNewCol_2(2) = TI.NewCol_2.B

                    ' Dims for loop
                    Dim iCC As Integer 'Current pixel's color
                    'Dim aCC(3) As Byte
                    'aCC(3) = 0
                    Dim iCO As Integer 'Current offset

                    'Dim SB As New StringBuilder

                    For y As Integer = TI.Y1 To TI.Y2
                        For x As Integer = TI.X1 To TI.X2
                            'Array.Copy(buf, CalculateOffset(x, y), aCC, 0, 3)
                            'iCC = BitConverter.ToInt32(aCC, 0)
                            iCO = CalculateOffset(x, y)
                            iCC = RGB(RasterBytes(iCO), RasterBytes(iCO + 1), RasterBytes(iCO + 2))

                            'SB.Append(iCC & vbNewLine)

                            If iCC = iOC1 Then ' Check to see if this pixel is OC1
                                ' copy in new color
                                Array.Copy(aNewCol_1, 0, RasterBytes, iCO, 3)

                            ElseIf iCC = iOC2 Then ' Check to see if this pixel is OC2
                                ' copy in new color
                                Array.Copy(aNewCol_2, 0, RasterBytes, iCO, 3)

                            End If
                        Next
                    Next
                Next
                'Debug.WriteLine(SB.ToString)

                ' REVERSE THE ARRAY AGAIN SO THE IMAGE IS RIGHT-SIDE UP
                Array.Reverse(RasterBytes)

                ' RECONSTITUTE BITMAP
                Array.Copy(RasterBytes, 0, buf, 54, RasterSize)
                If File.Exists(OutputPath) Then File.Delete(OutputPath)
                FS = New FileStream(OutputPath, FileMode.Create)
                FS.Write(buf, 0, buf.Length)
                FS.Close()
                FS.Dispose()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with ToggleSubpictures(). Error: " & ex.Message)
            End Try
        End Function

        Private Sub SetBitmapRegionColor(ByRef BM() As Byte, ByVal RI As sSubpictureToggleInfo)
            Try
                Dim NewCol(3) As Byte 'RGB (due to the above array.reverse)
                NewCol(0) = RI.NewCol.R
                NewCol(1) = RI.NewCol.G
                NewCol(2) = RI.NewCol.B

                For y As Integer = RI.Y1 To RI.Y2
                    For x As Integer = RI.X1 To RI.X2
                        'now copy in three bytes
                        Array.Copy(NewCol, 0, BM, CalculateOffset(x, y), 3)
                    Next
                Next

            Catch ex As Exception
                Throw New Exception("Problem with SetBitmapRegionColor(). Error: " & ex.Message)
            End Try
        End Sub

        Private Function CalculateOffset(ByVal X As Integer, ByVal Y As Integer) As Integer
            Try
                Dim Offset As Integer
                Offset = (Y * 720) * 3
                Offset += Math.Abs((720 - X)) * 3
                Return Offset

                '' OLD WAY
                'Dim Offset As Integer
                'Offset = (Y * 720) * 3
                'Offset += Math.Abs((720 - X)) * 3
                'Return Offset
            Catch ex As Exception
                Throw New Exception("Problem with CalculateOffset(). Error: " & ex.Message)
            End Try
        End Function

        Public Structure sSubpictureToggleInfo
            Public Toggle As Boolean
            Public X1, Y1, X2, Y2 As Short
            Public NewCol As Color
        End Structure

        Public Structure sTwoColorSwap
            Public X1, Y1, X2, Y2 As Short
            Public OldCol_1, OldCol_2, NewCol_1, NewCol_2 As Color
        End Structure

    End Module

End Namespace
