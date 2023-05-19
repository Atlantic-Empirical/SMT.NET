Imports System.IO
Imports System.Data
Imports System.Math

Namespace FileSystems.ISO9660

    Public Class cISO9660_SectorReader

        Public Enum Origin
            FILE_BEGIN
            FILE_CURRENT
            FILE_END
        End Enum

#Region "PROPERTIES"

        Public sectorData() As Byte
        Private _FirstFileStream As FileStream
        Private _SecondFileStream As FileStream
        Public _FirstFileSize, _SecondFileSize As Long
        Public _SectorSize, _SectorPointer, _CPM, _CPSEC, _CGMS, _LBN As Integer  'LBN starts from 1
        Public _FilePointer As Long
        Public _UseFirstFile As Boolean = True
        Public _FileSizeFitsBothFormat As Boolean = False

        Public ReadOnly Property SourceAPath() As String
            Get
                Return _SourceAPath
            End Get
        End Property
        Private _SourceAPath As String

        Public ReadOnly Property SourceBPath() As String
            Get
                Return _SourceBPath
            End Get
        End Property
        Private _SourceBPath As String

        Private Property FirstFileStream() As FileStream
            Get
                Return _FirstFileStream
            End Get
            Set(ByVal value As FileStream)
                _FirstFileStream = value
            End Set
        End Property

        Private Property SecondFileStream() As FileStream
            Get
                Return _SecondFileStream
            End Get
            Set(ByVal value As FileStream)
                _SecondFileStream = value
            End Set
        End Property

        Property FirstFileSize() As Long
            Get
                Return _FirstFileSize
            End Get
            Set(ByVal value As Long)
                _FirstFileSize = value
            End Set
        End Property

        Property SecondFileSize() As Long
            Get
                Return _SecondFileSize
            End Get
            Set(ByVal value As Long)
                _SecondFileSize = value
            End Set
        End Property

        Property SectorSize() As Integer
            Get
                Return _SectorSize
            End Get
            Set(ByVal value As Integer)
                _SectorSize = value
            End Set
        End Property

        Property SectorPointer() As Integer
            Get
                Return _SectorPointer
            End Get
            Set(ByVal value As Integer)
                _SectorPointer = value
            End Set
        End Property

        Property CPM() As Integer
            Get
                Return _CPM
            End Get
            Set(ByVal value As Integer)
                _CPM = value
            End Set
        End Property

        Property CPSEC() As Integer
            Get
                Return _CPSEC
            End Get
            Set(ByVal value As Integer)
                _CPSEC = value
            End Set
        End Property

        Property CGMS() As Integer
            Get
                Return _CGMS
            End Get
            Set(ByVal value As Integer)
                _CGMS = value
            End Set
        End Property

        Property LBN() As Integer
            Get
                Return _LBN
            End Get
            Set(ByVal value As Integer)
                If value <= GetLBNCount() OrElse value = 1 Then
                    _LBN = value
                Else
                    Console.WriteLine("The designed LBN is larger than the total LBN count, designed LBN = " + value)
                End If
            End Set
        End Property

        Property FilePointer() As Long
            Get
                If IsEndOfFile() Then
                    Return -1
                End If
                Return _FilePointer
            End Get
            Set(ByVal value As Long)
                _FilePointer = value
            End Set
        End Property

        Property UseFirstFile() As Boolean
            Get
                Return _UseFirstFile
            End Get
            Set(ByVal value As Boolean)
                _UseFirstFile = value
            End Set
        End Property

        Property FileSizeFitsBothFormat() As Boolean
            Get
                Return _FileSizeFitsBothFormat
            End Get
            Set(ByVal value As Boolean)
                _FileSizeFitsBothFormat = value
            End Set
        End Property

#End Region 'PROPERTIES

#Region "CONSTRUCTOR"

        Public Sub New(ByVal SourceA As String, Optional ByVal nSectorSize As Integer = eSectorSizeEnum.WithHeader, Optional ByVal SourceB As String = "")
            _SourceAPath = SourceA
            _SourceBPath = SourceB
            ClearSettings()
            OpenImageFile(SourceA, SourceB, nSectorSize)
        End Sub

#End Region 'CONSTRUCTOR

#Region "FUNCTIONALITY"

        Public Sub ClearSettings()

            Me.FirstFileSize = 0
            Me.SecondFileSize = 0
            Me.SectorSize = 0
            Me.SectorPointer = 0
            Me.CPM = 0
            Me.CPSEC = 0
            Me.CGMS = 0
            Me.LBN = 1
            Me.FilePointer = 0
            Me.UseFirstFile = True
            Me.FileSizeFitsBothFormat = False

        End Sub

        Public Sub OpenImageFile(ByVal firstFilePath As String, ByVal secondFilePath As String, ByVal desiredSectorSize As Integer)
            Try
                Me.FirstFileStream = File.Open(firstFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Me.FirstFileSize = Convert.ToInt32(Me.FirstFileStream.Length)

                SetSectorSize(FirstFileSize, desiredSectorSize)

                If Me.SectorSize = 0 AndAlso desiredSectorSize > 0 Then
                    Me.SectorSize = desiredSectorSize
                End If

                If Not secondFilePath = "" Then
                    Me.SecondFileStream = File.Open(secondFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Me.SecondFileSize = Convert.ToInt32(Me.SecondFileStream.Length)
                End If

            Catch ex As Exception
                Throw New Exception("Cannot open image file: " + secondFilePath & ". Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub SetSectorSize(ByVal pFileSize As Integer, ByVal pDesiredSectorSize As Integer)
            Dim verifyBothFormat As Boolean
            Dim tempSize As Integer
            verifyBothFormat = False
            tempSize = CType(eSectorSizeEnum.Standard, Integer)

            If pFileSize Mod tempSize = 0 Then
                verifyBothFormat = True
            End If

            If pFileSize Mod CType(eSectorSizeEnum.WithHeader, Integer) = 0 Then
                If verifyBothFormat Then
                    FileSizeFitsBothFormat = True
                End If

                tempSize = CType(eSectorSizeEnum.WithHeader, Integer)
            End If

            Me.SectorSize = tempSize
            ReDim sectorData(Me.SectorSize - 1)

        End Sub

        Public Sub AnalyzeHeader()
            If Me.SectorSize = CType(eSectorSizeEnum.WithHeader, Integer) Then
                If sectorData Is Nothing OrElse sectorData.Length = 0 Then Return
                Dim tByte(0) As Byte
                tByte(0) = sectorData(0)


                Dim bits As New BitArray(tByte)
                Me.CPM = bits(0)
                Me.CPSEC = bits.Item(1)
                Me.CGMS = FormCGMS(bits.Item(2), bits.Item(3))
            End If
        End Sub

        Public Function SeekASector(ByVal pLBN As Integer, ByVal increaseFilePointer As Boolean) As Boolean
            Dim result As Boolean = False
            Dim tcount = GetLBNCount()
            If pLBN > GetLBNCount() Then
                Console.WriteLine("The LBN is out of boundary, LBN = " & CType(pLBN, String))
                Return False
            End If

            Dim fcont As Integer = GetFirstFileLBNCount()

            If pLBN > GetFirstFileLBNCount() Then
                Me.UseFirstFile = False
                If Not Me.SecondFileStream Is Nothing Then
                    Me.SecondFileStream.Position = CType(((pLBN - GetFirstFileLBNCount() - 1) * SectorSize), Long)
                    'Console.WriteLine("second position = " + Me.SecondFileStream.Position)
                Else
                    Console.WriteLine("The second file is null, the desinated LBN = " & CType(pLBN, String))
                    Return False
                End If
            Else
                If Not Me.FirstFileStream Is Nothing Then
                    Me.FirstFileStream.Position = CType(((pLBN - 1) * SectorSize), Long)
                    'Console.WriteLine("first position = " + CType(Me.FirstFileStream.Position, String))
                Else
                    Console.WriteLine("The first file is null, the desinated LBN = " & CType(pLBN, String))
                    Return False
                End If
            End If

            Me.SectorPointer = 0

            Dim tRemaining, tOffset, count, read As Integer
            Dim hasHeaderBeenRead As Boolean
            tRemaining = SectorSize
            tOffset = 0
            count = 0
            read = 0
            hasHeaderBeenRead = False

            Do While tRemaining > 0
                If (UseFirstFile) Then
                    read = FirstFileStream.Read(sectorData, tOffset, tRemaining)
                Else
                    read = SecondFileStream.Read(sectorData, tOffset, tRemaining)
                End If

                If (read <= 0) Then
                    Throw New EndOfStreamException
                    String.Format("End of stream reached with {0} bytes left to read", tRemaining)
                End If


                If Not hasHeaderBeenRead Then
                    count = count + read
                    'currently we only check the first 4 bytes for header information
                    'while 6 bytes were assigned to the header
                    If (count >= 4) Then hasHeaderBeenRead = True
                End If
                'in case the file stream is unable to read the whole sector at a time
                tRemaining -= read
                tOffset += read
            Loop


            If (hasHeaderBeenRead) Then
                AnalyzeHeader()
                'reset the stream position
                If Not increaseFilePointer Then
                    If (UseFirstFile) Then

                        'Console.WriteLine("after analyze, first position = " + CType(Me.FirstFileStream.Position, String))
                        FirstFileStream.Position = CType(Me.LBN * SectorSize, Long)
                        'Console.WriteLine("after set first position = " + CType(Me.FirstFileStream.Position, String))
                    Else
                        'Console.WriteLine("after analyze, second position = " + CType(Me.SecondFileStream.Position, String))
                        SecondFileStream.Position = CType((GetLBNCount() - GetFirstFileLBNCount()) * Me.SectorSize, Long)
                        'Console.WriteLine("after set first position = " + CType(Me.SecondFileStream.Position, String))
                    End If
                Else
                    If (UseFirstFile) Then
                        Me.FilePointer = Me.FirstFileStream.Position
                    Else
                        Me.FilePointer = Me.FirstFileSize + Me.SecondFileStream.Position
                    End If

                    'Console.WriteLine("IN seekASector, pLBN = " + CType(pLBN, String) + ", lbn = " + CType(Me.LBN, String))
                    Me.LBN = pLBN + 1

                End If
                result = True

            End If
            Return result
        End Function

        Public Function GetByte() As Byte
            If (Me.sectorData Is Nothing OrElse Me.sectorData.Length = 0) Then
                SeekASector(Me.LBN, True)
            End If

            If (Me.SectorPointer < 0) Then Me.SectorPointer = 0

            Dim currentByte As Byte
            currentByte = Me.sectorData(SectorPointer)
            If (SectorPointer = SectorSize - 1) Then
                Me.LBN = Me.LBN + 1
                SeekASector(Me.LBN, True)
                Me.SectorPointer = 0
            Else
                Me.SectorPointer = Me.SectorPointer + 1
            End If
            IncreaseFilePointer(1)

            Return currentByte
        End Function

        Public Sub GetBytes(ByVal returnSectorArray() As Byte)
            'The size of the array passed in can be larger than one sector
            Dim totalSeekingSize As Integer
            totalSeekingSize = returnSectorArray.Length

            If (totalSeekingSize = 0) Then
                Console.WriteLine("the size of the return array must be larger than 0")
                Return
            End If

            Console.WriteLine("in sectorReader, array size = " + CType(totalSeekingSize, String))

            'determine the number of the sectors will be retrieved
            Dim sectorCount As Integer
            sectorCount = Math.Ceiling(CType(totalSeekingSize / SectorSize, Decimal))
            'sectorPointer will be used when the size of the returned array is not the mutiplier of sector size.
            'here also assume the size of the return array is the multiplier of the sector size.
            'so sectorPointer always starts from 0
            Me.SectorPointer = 0

            For i As Integer = 0 To sectorCount - 1
                'which step is required to increase LBN? 
                'make sure array copyTo is working with different size
                SeekASector(Me.LBN, True)
                Me.sectorData.CopyTo(returnSectorArray, i * SectorSize)
                Me.LBN = Me.LBN + 1
            Next
            IncreaseFilePointer(totalSeekingSize)
        End Sub

        Public Sub GetBytes(ByVal returnSectorArray() As Byte, ByVal fromCurrentFilePointer As Boolean)
            Dim bytesRemaining As Integer
            bytesRemaining = returnSectorArray.Length

            If (bytesRemaining = 0) Then
                Console.WriteLine("the size of the return array must be larger than 0")
                Return
            End If

            If Not fromCurrentFilePointer Then
                'Read from the stream from the beginning of the sector where the current LBN pointed to
                GetBytes(returnSectorArray)
                Return
            End If

            Dim bytesCopyFromSectorData As Integer
            bytesCopyFromSectorData = returnSectorArray.Length

            If (bytesRemaining >= (SectorSize - SectorPointer)) Then
                bytesCopyFromSectorData = Me.SectorSize - Me.SectorPointer
            End If

            bytesRemaining = bytesRemaining - bytesCopyFromSectorData
            'copy the rest of the bytes in sectorData to the returnSectorArray
            Array.ConstrainedCopy(Me.sectorData, Me.SectorPointer, returnSectorArray, 0, bytesCopyFromSectorData)
            Me.SectorPointer = Me.SectorPointer + bytesCopyFromSectorData

            'Console.WriteLine("in getBytes, after get, sectorPointer = " + CType(Me.SectorPointer, String))
            'Console.WriteLine("in getBytes, first byteRemaining = " + CType(bytesRemaining, String))

            'fill out the rest of the returnSectorArray
            If (bytesRemaining > 0) Then

                'determine the number of the sectors will be retrieved
                'int sectorCount = (int)Math.Ceiling((decimal)bytesRemaining / sectorSize);
                Dim sectorCount As Integer
                sectorCount = bytesRemaining / Me.SectorSize

                Me.SectorPointer = 0

                For i As Integer = 0 To sectorCount - 1
                    'which step is required to increase LBN? 
                    SeekASector(Me.LBN, True)
                    sectorData.CopyTo(returnSectorArray, i * SectorSize)
                    Me.LBN = Me.LBN + 1
                    bytesRemaining = bytesRemaining - Me.SectorSize
                Next

                'Console.WriteLine("in getBytes, second byteRemaining = " + CType(bytesRemaining, String))
                If (bytesRemaining > 0) Then
                    SeekASector(Me.LBN, False)
                    Array.ConstrainedCopy(Me.sectorData, Me.SectorPointer, returnSectorArray, (returnSectorArray.Length - bytesRemaining), bytesRemaining)
                    Me.SectorPointer = bytesRemaining
                    Me.LBN = Me.LBN + 1
                    IncreaseFilePointer(bytesRemaining)
                End If
            End If
        End Sub

        Public Function FormCGMS(ByVal firstByte As Integer, ByVal secondByte As Integer) As Integer
            Dim tenths As Integer = 0
            If secondByte Then tenths = 3
            Return tenths + firstByte
        End Function

        Public Sub CloseImageFile()
            If Not Me.FirstFileStream Is Nothing Then
                Me.FirstFileStream.Close()
            End If

            If Not Me.SecondFileStream Is Nothing Then
                Me.SecondFileStream.Close()
            End If
        End Sub

        Public Function SeekLBN(ByVal startingLBN As Integer, ByVal pOffset As Long) As Integer
            If Me.SectorSize > 0 Then
                Return Math.Ceiling((startingLBN * Me.SectorSize + pOffset) / Me.SectorSize)
            End If
        End Function

        Public Function GetFirstFileLBNCount() As Integer
            If Me.SectorSize > 0 Then
                Return Me.FirstFileSize / Me.SectorSize
            End If
        End Function

        Public Function GetLBNCount() As Integer
            If Me.SectorSize > 0 Then
                Return GetFileSize() / Me.SectorSize
            End If
            Return 0
        End Function

        Public Function GetFileSize() As Long
            Return (Me.FirstFileSize + Me.SecondFileSize)
        End Function

        Private Sub IncreaseFilePointer(ByVal offset As Integer)
            If Me.UseFirstFile Then
                Me.FirstFileStream.Position = Me.FirstFileStream.Position + offset
                Me.FilePointer = Me.FirstFileStream.Position
            Else
                Me.SecondFileStream.Position = Me.SecondFileStream.Position + offset
                Me.FilePointer = Me.SecondFileStream.Position
            End If
        End Sub

        Public Function IsEndOfFile() As Boolean
            If (Me.LBN = GetLBNCount()) AndAlso Me.SectorPointer = 0 Then
                Return True
            End If

            If Me.SecondFileStream Is Nothing Then
                If Me.FirstFileStream.Length = Me.FirstFileStream.Position + 1 Then
                    Return True
                End If
            Else
                If Me.SecondFileStream.Length = Me.SecondFileStream.Position + 1 Then
                    Return True
                End If
            End If

            Return False
        End Function

        Public Function GetCurrentFileStream() As FileStream
            If UseFirstFile Then
                Return Me.FirstFileStream
            Else
                Return Me.SecondFileStream
            End If
        End Function

#End Region  'FUNCTIONALITY

    End Class

End Namespace
