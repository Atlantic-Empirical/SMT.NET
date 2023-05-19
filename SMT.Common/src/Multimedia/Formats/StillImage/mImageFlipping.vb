
Namespace Multimedia.Formats.StillImage

    Public Module mImageFlipping

        Public Function FlipImageBuffer_Vertically(ByRef Buff() As Byte) As Byte()
            Try
                '    Dim out(UBound(Buff)) As Byte
                '    Dim ix As Integer = 0
                '    For i As Integer = UBound(Buff) To 0 Step -1
                '        out(ix) = Buff(i)
                '        ix += 1
                '    Next
                '    Return out
                Array.Reverse(Buff)
                Return Buff
            Catch ex As Exception
                Throw New Exception("Problem with FlipRGBImageBuffer_Vertically. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function FlipRGB24ImageBuffer_Horizontally(ByRef Buff() As Byte, ByVal Width As Short, ByVal Height As Short) As Byte()
            Try
                Dim Stride As Short = Width * 3
                Dim out(UBound(Buff)) As Byte
                Dim Line(Stride - 1) As Byte
                Dim LineFirstByte As Integer = 0
                Dim ix As Short
                For v As Short = 0 To Height - 1
                    'OLD WAY
                    'Put the line into the line array
                    'Array.Copy(Buff, LineFirstByte, Line, 0, Stride)

                    ''Copy the line to the out array, starting from right to left to flip image.
                    'ix = Stride - 1
                    'For i As Short = 0 To Stride - 1
                    '    out(LineFirstByte + i) = Line(ix)
                    '    ix -= 1
                    'Next

                    'NEW WAY 040724
                    Array.Reverse(Buff, LineFirstByte, Stride)

                    LineFirstByte += Stride
                Next
                Return Buff
            Catch ex As Exception
                Throw New Exception("Problem with FlipRGBImageBuffer_Horizontally. Error: " & ex.Message, ex)
            End Try
        End Function

    End Module

End Namespace
