
Namespace DotNet.Utility

    Public Class cEndianBitConverter

        Public Shared Function CreateForLittleEndian() As cEndianBitConverter
            Return New cEndianBitConverter(Not BitConverter.IsLittleEndian)
        End Function

        Public Shared Function CreateForBigEndian() As cEndianBitConverter
            Return New cEndianBitConverter(BitConverter.IsLittleEndian)
        End Function

        Private swap As Boolean

        Private Sub New(ByVal swapBytes As Boolean)
            swap = swapBytes
        End Sub

        Public Function ToUInt16(ByRef data() As Byte, ByVal Offset As Integer) As UInt16
            Dim corrected(1) As Byte
            Array.Copy(data, Offset, corrected, 0, 2)
            If swap Then
                Array.Reverse(corrected, 0, 2)
            End If
            Return BitConverter.ToUInt16(corrected, 0)
        End Function

        Public Function ToUInt32(ByRef data() As Byte, ByVal Offset As Integer) As UInt32
            Dim corrected(3) As Byte
            Array.Copy(data, Offset, corrected, 0, 4)
            If swap Then
                Array.Reverse(corrected, 0, 4)
            End If
            Return BitConverter.ToUInt32(corrected, 0)
        End Function

        Public Function ToUInt64(ByRef data() As Byte, ByVal Offset As Integer) As UInt64
            Dim corrected(7) As Byte
            Array.Copy(data, Offset, corrected, 0, 8)
            If swap Then
                Array.Reverse(corrected, 0, 8)
            End If
            Return BitConverter.ToUInt64(corrected, 0)
        End Function

        ' And similar methods for GetBytes(Int32) and all other types needed.

    End Class

End Namespace
