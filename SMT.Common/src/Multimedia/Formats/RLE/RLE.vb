Imports System.IO

Namespace Multimedia.Formats.RLE

    ''' <summary>
    ''' 5.4.3.2 - NOT NEAR COMPLETE
    ''' </summary>
    ''' <remarks>This is a sonofabich</remarks>
    Public Module DVD_Subpicture_RLE

        Public Function DecompressRLE(ByRef buf() As Byte) As Byte()
            Try
                Dim MS As New MemoryStream

                'queue it up
                Dim Q As New Queue(Of UInt16)
                For i As Integer = 0 To UBound(buf) Step 2
                    Q.Enqueue(BitConverter.ToUInt16(buf, i))
                Next

                Dim tB_1 As UInt16
                Dim PD As Byte

                'one lap through for each unit
                While Q.Count > 0
                    tB_1 = Q.Dequeue

                    'identify a unit
                    If (tB_1 >> 14 And 3) > 0 Then
                        '1-3 pixels

                    ElseIf ((tB_1 >> 10) And 15) > 0 Then
                        '4-15 pixels

                    ElseIf ((tB_1 >> 6) And 63) > 0 Then
                        '16-63 pixels

                    ElseIf ((tB_1 >> 2) And 255) > 0 Then
                        '64-255 pixels

                    ElseIf ((tB_1 >> 2) And 16383) = 0 Then
                        'to the end of the line

                    End If

                End While

                Return MS.ToArray
            Catch ex As Exception
                Throw New Exception("Problem with DecompressRLE(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Enum ePixelType
            Background
            Pattern
            Emphasis_1
            Emphasis_2
        End Enum

    End Module

End Namespace
