Imports System.IO
Imports System.Data
Namespace FileSystems.ISO9660

    Public Class cISO9660_StructureReader
        Public sr As cISO9660_SectorReader

#Region "CONSTRUCTOR"

        Public Sub New(ByRef SectorReader As cISO9660_SectorReader)
            sr = SectorReader
        End Sub

        'Public Sub New(ByVal fileName As String)
        '    sr = New cISO9660_SectorReader(fileName, 2054)
        '    sr.openImageFile(fileName)
        'End Sub

        'Public Sub New(ByVal firstFileName As String, ByVal secondFileName As String, ByVal desiredSectorSize As Integer)
        '    sr = New cISO9660_SectorReader(firstFileName, secondFileName)
        '    sr.openImageFile(firstFileName, secondFileName, desiredSectorSize)
        'End Sub

#End Region 'CONSTRUCTOR

#Region "FUNCTIONALITIES"
        Public Function FileRead64CharB8() As Char

            Dim temp As String
            temp = CType(sr.getByte(), String)
            Return CType(temp, Char)
        End Function

        Public Function FileRead64ByteB8() As Byte
            Return sr.GetByte()
        End Function

        Public Function FileRead64ShortintB8() As Byte
            'Return CType(sr.GetByte(), SByte)
            Return sr.GetByte()
        End Function

        Public Function FileRead64WordLB16() As UShort
            Dim tempArray(1) As Byte
            sr.getBytes(tempArray, True)
            Return FormUShortByLittleEndian(tempArray)
        End Function

        Private Function FormUShortByLittleEndian(ByVal pByteArray() As Byte) As UShort
            Dim firstWord, secondWord As UShort
            firstWord = CType(pByteArray(0), UShort)
            secondWord = CType(pByteArray(1), UShort)
            secondWord = secondWord << 8
            Return (firstWord Or secondWord)
        End Function

        Public Function FileRead64IntegerLB32() As Integer
            Dim tempArray(3) As Byte
            sr.getBytes(tempArray, True)

            'For i As Integer = 0 To 3
            '    Console.WriteLine(CType(i, String) + ", " + CType(tempArray(i), String))
            'Next

            Return FormIntegerByLittleEndian(tempArray)
        End Function

        Private Function FormIntegerByBigEndian(ByVal pByteArray() As Byte) As Integer
            Dim result As Integer = 0
            Dim temp As Integer = 0

            For i As Integer = 0 To 3
                temp = CType(pByteArray(i), Integer)
                result = result Or temp
            Next
            Return result
        End Function

        Public Function FileRead64WordMB16() As UShort
            Dim tempArray(1) As Byte
            sr.getBytes(tempArray, True)
            Return FormUShortByBigEndian(tempArray)
        End Function

        Private Function FormUShortByBigEndian(ByVal pByteArray() As Byte) As UShort
            Dim firstWord, secondWord As UShort
            firstWord = CType(pByteArray(0), UShort)
            secondWord = CType(pByteArray(1), UShort)
            firstWord = firstWord << 8
            Return (firstWord Or secondWord)
        End Function

        Public Function FileRead64IntegerMB32() As Integer
            Dim tempArray(3) As Byte
            sr.getBytes(tempArray, True)
            Return FormIntegerByBigEndian(tempArray)
        End Function

        Private Function FormIntegerByLittleEndian(ByVal pByteArray() As Byte) As Integer
            Dim result As Integer = 0
            Dim temp As Integer = 0

            For i As Integer = 0 To 3
                temp = CType(pByteArray(i), Integer)
                temp = temp << (8 * i)
                result = result Or temp
            Next
            Return result
        End Function

        Public Function FileRead64WordBB16() As Integer
            Dim temp As Integer = FileRead64WordLB16()
            ' skip the following 2 bytes
            'sr.FilePointer = sr.FilePointer + 2
            'sr.SectorPointer = sr.SectorPointer + 2
            FileRead64WordLB16()
            Return temp
        End Function

        Public Function FileRead64IntegerBB32() As Integer
            Dim temp As Integer = FileRead64IntegerLB32()
            ' skip the following 4 bytes
            'sr.FilePointer = sr.FilePointer + 4
            FileRead64IntegerLB32()
            Return temp
        End Function

        Public Function FileRead64String(ByVal count As Integer, Optional ByVal expectDigits As Boolean = False) As String
            Dim tempArray(count - 1) As Byte
            Dim result As String = ""
            sr.GetBytes(tempArray, True)
            Dim c As Char
            For i As Integer = 0 To count - 1
                c = Microsoft.VisualBasic.ChrW(tempArray(i))
                If expectDigits AndAlso c = Nothing Then
                    c = "0"
                End If
                result = result & c
            Next
            Return result
        End Function

        Public Sub IncreaseLBN(Optional ByVal lbnOffset As Integer = 1)
            If lbnOffset > 0 Then
                sr.LBN = sr.LBN + lbnOffset
                sr.SeekASector(sr.LBN, True)
            End If
        End Sub


        'Jump to a specific sector
        Public Function LongFileSeek(ByVal offset As Long, ByVal origin As Integer) As Boolean
            'sr.FilePointer = CType(origin + offsetLo, Long)
            'LBN starts from 1
            Dim newLBN As Integer = sr.SeekLBN(0, offset)

            If origin = CType(cISO9660_SectorReader.Origin.FILE_CURRENT, Integer) Then
                newLBN = sr.SeekLBN(sr.LBN, offset)
            End If
            If origin = CType(cISO9660_SectorReader.Origin.FILE_END, Integer) Then
                newLBN = sr.SeekLBN(sr.GetLBNCount, -(offset))
            End If

            Dim result As Boolean
            result = sr.SeekASector(newLBN, True)
            If offset Mod sr.SectorSize > 0 Then
                sr.SectorPointer = (offset Mod sr.SectorSize) - 1
            End If
            Return result
        End Function

        'Public Sub LongFileSeek64(ByVal offsetLo As Long, ByVal origin As Long)
        '    sr.FilePointer = CType(origin + offsetLo, Long)
        'End Sub

        Public Function GetCurrentLBN() As Integer
            Return sr.LBN
        End Function

        Public Function GetSectorSize() As Integer
            Return sr.SectorSize
        End Function


        Public Function GetSectorPointer() As Integer
            Return sr.SectorPointer
        End Function

        Public Sub SetSectorPointer(ByVal pSectorPointer As Integer)
            sr.SectorPointer = pSectorPointer
        End Sub

        Public Function GetLongFilePosition() As Long
            Return sr.FilePointer
        End Function

        Public Function IsEndOfFile() As Boolean
            Return sr.IsEndOfFile()
        End Function

#End Region 'FUNCTIONALITIES

    End Class

End Namespace
