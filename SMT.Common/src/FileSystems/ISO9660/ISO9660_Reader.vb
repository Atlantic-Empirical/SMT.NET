Imports System.IO
Imports System.Windows.Forms

Namespace FileSystems.ISO9660

    Public Class cISO9660_Reader

#Region "PROPERTIES"

        Public ReadOnly Property SourceAPath() As String
            Get
                If Me.CoreSectorReader Is Nothing Then Return ""
                Return Me.CoreSectorReader.SourceAPath
            End Get
        End Property

        Public ReadOnly Property SourceBPath() As String
            Get
                If Me.CoreSectorReader Is Nothing Then Return ""
                Return Me.CoreSectorReader.SourceBPath
            End Get
        End Property

        Public ReadOnly Property FileStructure() As TreeNode
            Get
                Dim Out As New TreeNode("Root")
                Dim pRootDir As cISO9660_FileSystem.TISOFileDescriptor
                ''use these while iterating the cISO9660_FileSystem object
                If Not CoreFileSystem Is Nothing Then
                    pRootDir = CoreFileSystem.VolumeDescriptor.RootDir
                    AddNodes(pRootDir, Out)
                End If
                Return Out
            End Get
        End Property

        Public CoreSectorReader As cISO9660_SectorReader
        Public CoreStructureReader As cISO9660_StructureReader
        Public CoreFileSystem As cISO9660_FileSystem
        'Public CoreFileNavigator As cISO9660_FileNavigator

#End Region 'PROPERTIES

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByVal SourceA As String, Optional ByVal desiredSectorSize As Integer = eSectorSizeEnum.WithHeader, Optional ByVal SourceB As String = "")
            Me.CoreSectorReader = New cISO9660_SectorReader(SourceA, desiredSectorSize, SourceB)
            Me.CoreStructureReader = New cISO9660_StructureReader(Me.CoreSectorReader)
            Me.CoreFileSystem = New cISO9660_FileSystem(Me.CoreStructureReader)
            'Me.CoreFileNavigator = New cISO9660_FileNavigator(Me.CoreFileSystem)
        End Sub

        Public Sub Dispose()
            Me.CoreFileSystem.sr.sr.CloseImageFile()
        End Sub

#End Region 'CONSTRUCTOR

#Region "FUNCTIONALITY"

        Private Sub AddNodes(ByVal pFD As cISO9660_FileSystem.TISOFileDescriptor, ByVal pOut As TreeNode)
            Dim tempNode As TreeNode
            For Each item As cISO9660_FileSystem.TISOFileDescriptor In pFD.Items
                Dim fi = New FileInfo(item.Sector, item.Length)
                tempNode = pOut.Nodes.Add(item.Name)
                tempNode.Tag = fi
                tempNode.Name = item.Name
                If item.Items.Count > 0 Then
                    AddNodes(item, tempNode)
                End If
            Next
        End Sub

        Public Function FindNode(ByVal pName As String) As cISO9660_FileSystem.TISOFileDescriptor
            Return Me.CoreFileSystem.VolumeDescriptor.GetAFile(Me.CoreFileSystem.VolumeDescriptor.RootDir, pName)
        End Function

        Public Function GetStreamForISO9660File(ByVal SectorNumber As Integer, ByVal FileLength As Long) As cISO9660_FileStream
            Dim stream As Stream = New cISO9660_FileStream(SectorNumber, FileLength, Me.SourceAPath, Me.SourceBPath)
            Return stream
        End Function

        Public Shared Function DetermineSectorSize(ByVal filePath As String) As Integer
            Try
                Dim fileStream As FileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim StandardIDValue As String = "CD001"
                Dim standardID(4) As Byte
                Dim result As String = ""

                'Start with 2048
                GetStandardID(fileStream, standardID, eSectorSizeEnum.Standard)
                result = GetString(standardID)
                If (result.Equals(StandardIDValue)) Then
                    fileStream.Close()
                    Return eSectorSizeEnum.Standard
                End If

                Array.Clear(standardID, 0, standardID.Length)

                'Then 2054
                GetStandardID(fileStream, standardID, eSectorSizeEnum.WithHeader)
                result = GetString(standardID)
                If (result.Equals(StandardIDValue)) Then
                    fileStream.Close()
                    Return eSectorSizeEnum.WithHeader
                End If

                'If it's not 2048 nor 2054, it's an invalid image file
                fileStream.Close()
                Return 0

            Catch ex As Exception
                Throw New Exception("Cannot open file: " & filePath & ". Error: " & ex.Message, ex)
            End Try
        End Function

        Public Shared Sub GetStandardID(ByVal filestream As FileStream, ByVal standardID() As Byte, ByVal sectorSize As Integer)
            Dim remaining, offset, count, read As Integer
            remaining = standardID.Length

            offset = 16 * sectorSize + 1 'Jump to LBN 16 and Skip Volum Descriptor Type

            If sectorSize = eSectorSizeEnum.WithHeader Then
                offset = offset + 6  'Skip the header
            End If

            count = 0
            read = 0

            filestream.Position = offset

            Do While remaining > 0
                read = filestream.Read(standardID, 0, remaining)

                If (read <= 0) Then
                    Throw New EndOfStreamException
                    String.Format("End of stream reached with {0} bytes left to read", remaining)
                End If
                'in case the file stream is unable to read the whole sector at a time
                remaining -= read
                offset += read
            Loop
        End Sub

        Public Shared Function GetString(ByVal tempArray() As Byte) As String
            Dim count As Integer = tempArray.Length
            Dim result As String = ""
            Dim c As Char
            For i As Integer = 0 To count - 1
                c = Microsoft.VisualBasic.ChrW(tempArray(i))
                result = result & c
            Next
            Return result
        End Function

        Public Function FindAFileNameBySectorNo(ByVal pSectorNo As Integer, ByVal pNode As TreeNode) As String
            For Each tempNode As TreeNode In pNode.Nodes()
                Dim fi As FileInfo
                If Not tempNode.Tag Is Nothing Then
                    fi = CType(tempNode.Tag, FileInfo)
                    If fi.Sector = pSectorNo Then
                        Return tempNode.Name
                    End If

                    'Check if we can find the sector within this file
                    If fi.Sector < pSectorNo Then
                        Dim totalSectors = Math.Ceiling(fi.Length / Me.CoreFileSystem.VolumeDescriptor.ActualSectorSize)
                        If (fi.Sector + totalSectors) >= pSectorNo Then
                            Return tempNode.Name
                        End If
                    End If
                End If

                'Keep searching if the node has sub-nodes
                If tempNode.Nodes.Count() > 0 Then
                    FindAFileNameBySectorNo(pSectorNo, tempNode)
                End If
            Next
            Return ""
        End Function

#End Region 'FUNCTIONALITY

#Region "SECTOR_HEADER_DATA"

        Public ReadOnly Property CPM(ByVal SectorNumber As Integer) As Integer
            Get
                If Not SectorNumber = Nothing AndAlso SectorNumber > 0 Then
                    If Not Me.CoreSectorReader Is Nothing Then
                        Me.CoreSectorReader.SeekASector(SectorNumber, False)
                        Return Me.CoreSectorReader.CPM
                    End If
                End If
                Return -1
            End Get
        End Property

        Public ReadOnly Property CPSEC(ByVal SectorNumber As Integer) As Integer
            Get
                If Not SectorNumber = Nothing AndAlso SectorNumber > 0 Then
                    If Not Me.CoreSectorReader Is Nothing Then
                        Me.CoreSectorReader.SeekASector(SectorNumber, False)
                        Return Me.CoreSectorReader.CPSEC
                    End If
                End If
                Return -1
            End Get
        End Property

        Public ReadOnly Property CGMS(ByVal SectorNumber As Integer) As Integer
            Get
                If Not SectorNumber = Nothing AndAlso SectorNumber > 0 Then
                    If Not Me.CoreSectorReader Is Nothing Then
                        Me.CoreSectorReader.SeekASector(SectorNumber, False)
                        Return Me.CoreSectorReader.CGMS
                    End If
                End If
                Return -1
            End Get
        End Property

#End Region 'SECTOR_HEADER_DATA

        Structure FileInfo
            Dim Sector As Integer
            Dim Length As Long

            Sub New(ByVal pSector As Integer, ByVal pLength As Long)
                Me.Sector = pSector
                Me.Length = pLength
            End Sub

            Public Function getSector() As Integer
                Return Sector
            End Function

            Public Function GetLength() As Long
                Return Length
            End Function
        End Structure

    End Class
End Namespace
