Imports System.Drawing

Namespace Multimedia.Formats.StillImage.ColorSpaceConversions

    Public Class cColorSpaceConversions

#Region "ARGB8ToRGB24"

        Public Function ConvertARGB8RasterToRGB24Raster(ByVal InBuff() As Byte) As Byte()
            Try
                'This routine ignores the alpha bits in the source buffer.
                Dim Out((InBuff.Length * 3) - 1) As Byte
                For i As Integer = 0 To UBound(InBuff) Step 2
                    Out(i * 3) = HalfByteTo255Int(InBuff(i), False) 'R
                    Out((i * 3) + 1) = HalfByteTo255Int(InBuff(i + 1), True) 'G
                    Out((i * 3) + 2) = HalfByteTo255Int(InBuff(i + 1), False) 'B
                Next
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with ConvertARGB8RasterToRGB24Raster. Error: " & ex.Message)
            End Try
        End Function

        Public Function ConvertARGB8PixelToRGB24Color(ByVal InBuff() As Byte) As Color
            Try
                Dim A As Short = HalfByteTo255Int(InBuff(0), True)
                Dim R As Short = HalfByteTo255Int(InBuff(0), False)
                Dim G As Short = HalfByteTo255Int(InBuff(1), True)
                Dim B As Short = HalfByteTo255Int(InBuff(1), False)
                Return Color.FromArgb(A, R, G, B)
            Catch ex As Exception
                Throw New Exception("Problem with ConvertARGB8PixelToRGB24Color. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Function HalfByteTo255Int(ByVal InByte As Byte, ByVal ReturnUpper As Boolean) As Byte
            Try
                Dim str As String = Hex(InByte)
                If ReturnUpper Then
                    Dim s As String = Microsoft.VisualBasic.Left(str, 1)
                    Return CByte(s) * 17
                Else
                    Dim s As String = Microsoft.VisualBasic.Right(str, 1)
                    Return CByte(s) * 17
                End If
            Catch ex As Exception
                Throw New Exception("Problem with HalfByteTo255Int. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

#End Region 'ARGB8ToRGB24

        Public Function IsEven(ByVal Num As Integer) As Boolean
            Try
                Dim Remainder As Integer
                Math.DivRem(Num, 2, Remainder)
                If Remainder > 0 Then
                    Return False
                Else
                    Return True
                End If
            Catch ex As Exception
                Throw New Exception("Problem with IsEven. Error: " & ex.Message)
            End Try
        End Function

#Region "Non-Op"

        Public Sub YUV2RGB(ByRef Y As Integer, ByRef U As Integer, ByRef V As Integer, ByRef R As Integer, ByRef G As Integer, ByRef B As Integer)
            Try
                R = Y + (V * 1.436)
                G = Y - ((U * 0.394) - (V * 0.581))
                B = Y + (U * 2.032)
                If R < 0 Then R = 0
                If R > 255 Then R = 255
                If B < 0 Then B = 0
                If B > 255 Then B = 255
                If G < 0 Then G = 0
                If G > 255 Then G = 255
            Catch ex As Exception
                Throw New Exception("Problem with YUV2RGB. Error: " & ex.Message)
            End Try
        End Sub

        '    //void
        '//uyvy2rgb ( LPBYTE src,  LPBYTE dest,   int NumPixels)
        '//{
        '//  register int i = (NumPixels << 1)-1;
        '//  register int j = NumPixels + ( NumPixels << 1 ) -1;
        '//  register int y0, y1, u, v;
        '//  register int r, g, b;
        '//
        '//  while (i >= 0) {
        '//    y1 = (char) src[i--];
        '//    v  = (char) src[i--] - 128;
        '//    y0 = (char) src[i--];
        '//    u  = (char) src[i--] - 128;
        '//    YUV2RGB (y1, u, v, r, g, b);
        '//    dest[j--] = b;
        '//    dest[j--] = g;
        '//    dest[j--] = r;
        '//    YUV2RGB (y0, u, v, r, g, b);
        '//    dest[j--] = b;
        '//    dest[j--] = g;
        '//     dest[j--] = r;
        '//  }
        '//}

        Public Function ConvertUYVYtoRGB24(ByVal Source() As Byte) As Byte()
            Try
                Dim Target((Source.Length / 2) + Source.Length - 1) As Byte
                Dim Y0, Y1, U, V As Short
                Dim R, G, B As Short
                Dim TargetAddress As Integer = UBound(Target)
                For SourceAddress As Integer = UBound(Source) To 0 Step -4
                    U = Source(SourceAddress)
                    Y0 = Source(SourceAddress - 1)
                    V = Source(SourceAddress - 2)
                    Y1 = Source(SourceAddress - 3)

                    Me.YUV2RGB(Y1, U, V, R, G, B)
                    Target(TargetAddress) = B
                    TargetAddress -= 1
                    Target(TargetAddress) = G
                    TargetAddress -= 1
                    Target(TargetAddress) = R
                    TargetAddress -= 1

                    Me.YUV2RGB(Y0, U, V, R, G, B)
                    Target(TargetAddress) = B
                    TargetAddress -= 1
                    Target(TargetAddress) = G
                    TargetAddress -= 1
                    Target(TargetAddress) = R
                    TargetAddress -= 1
                Next
                Return Target


                ''Dim UV As Byte
                ''Dim Y As Byte


                ''    TRGBTripleArray  = ARRAY[0..YUVwidth-1] OF TRGBTriple;
                ''    pRGBTripleArray = ^TRGBTripleArray;

                ''  VAR
                ''    BitmapRGB  :  TBitmap;
                ''    BufferYUV  :  ARRAY[0..YUVwidth-1] OF TYUVWord;
                ''    i          :  INTEGER;
                ''    j          :  INTEGER;
                ''    Row        :  pRGBTripleArray;
                ''    StreamYUV  :  TFileStream;
                ''    U          :  INTEGER;
                ''    V          :  INTEGER;
                ''    Y          :  INTEGER;

                'Dim U0, Y0, V0 As Integer
                'Dim Y1, U1, Y2 As Integer
                'Dim V1, Y3, U2 As Integer
                'Dim Y4, V2, Y5 As Integer

                'Dim out((Buffer.Length / 2) + Buffer.Length) As Byte
                'For j As Integer = 0 To UBound(Buffer) Step 9

                '    U0 = Buffer(j)
                '    Y0 = Buffer(j + 1)
                '    V0 = Buffer(j + 2)

                '    Y1 = Buffer(j + 3)
                '    U1 = Buffer(j + 4)
                '    Y2 = Buffer(j + 5)

                '    V1 = Buffer(j + 6)
                '    Y3 = Buffer(j + 7)
                '    U2 = Buffer(j + 8)

                '    Y4 = Buffer(j + 9)
                '    V2 = Buffer(j + 10)
                '    Y5 = Buffer(j + 11)



                'Next

                ''      FOR j := 0 TO YUVheight-1 DO
                ''                            BEGIN()
                ''        StreamYUV.Read(BufferYUV,  SizeOf(BufferYUV));
                ''        Row := BitmapRGB.Scanline[j];

                ''        FOR i := 0 TO YUVwidth-1 DO
                ''                                BEGIN()
                ''          IF  i MOD 2 = 0   // Even
                ''          THEN BEGIN
                ''            Y := BufferYUV[i].Y;
                ''            U := BufferYUV[i].UV;
                ''            V := BufferYUV[i+1].UV     // get V from next word
                ''                                    End
                ''          ELSE BEGIN        // Odd
                ''            Y := BufferYUV[i].Y;
                ''            U := BufferYUV[i-1].UV;    // get U from last word
                ''            V := BufferYUV[i].UV
                ''          END;

                ''          WITH Row[i] DO
                ''                                        BEGIN()
                ''              // See "HELP! YUV-RGB conversion", 12/12/97 by
                ''              // lweng@analogic.com for this formula:
                ''              // http://www.deja.com/getdoc.xp?AN=297574991
                ''              rgbtRed   := FixValue(1.164*(Y-16) + 1.596*(V-128));
                ''              rgbtGreen := FixValue(1.164*(Y-16) - 0.392*(U-128) - 0.813*(V-128));
                ''              rgbtBlue  := FixValue(1.164*(Y-16) + 2.017*(U-128))
                ''                                        End
                ''                                        End

                ''      END;

                ''      BitmapRGB.SaveToFile('UYVY.BMP');
                ''      ImageYUV.Picture.Graphic := BitmapRGB
                ''    FINALLY
                ''                        BitmapRGB.Free()
                ''                        End

                ''  FINALLY
                ''                        StreamYUV.Free()
                ''                        End


            Catch ex As Exception
                Throw New Exception("Problem with ConvertUYVYtoRGB24. Error: " & ex.Message)
            End Try
        End Function

#End Region 'Non-Op

    End Class

End Namespace
