Imports System.IO
Imports System.Data
Imports System.Collections.ArrayList

Namespace FileSystems.ISO9660

    Public Class cISO9660_FileSystem

#Region "DECLARTIONS"

        Public Const CISOVolumeDescriptorID As String = "CD001"
        Public Const CISOVolumeDescriptorLBN As Integer = 16
        Public sr As cISO9660_StructureReader

        Public VolumeDescriptor As TISOVolumeDescriptor

#End Region

#Region "CONSTRUCTOR"

        Public Sub New(ByRef StructureReader As cISO9660_StructureReader)
            sr = StructureReader
            Me.VolumeDescriptor = New TISOVolumeDescriptor()
            Me.VolumeDescriptor.Read(Me)
        End Sub

#End Region 'CONSTRUCTOR

#Region "PUBLIC METHODS"

        ''' <summary>
        ''' Jump to a specific sector based on LBN and sector size
        ''' </summary>
        ''' <param name="pSector">LBN</param>
        ''' <param name="pSectorSize">Sector Size</param>
        ''' <returns>True if it's not EOF</returns>
        ''' <remarks>Starting from the beginning of the file</remarks>
        Public Function SeekToSector(ByVal pSector As Integer, ByVal pSectorSize As Integer, Optional ByVal origin As Integer = cISO9660_SectorReader.Origin.FILE_BEGIN) As Boolean
            Return sr.LongFileSeek(pSector * pSectorSize, origin)
        End Function

#End Region 'PUBLIC METHODS

#Region "CLASSES"

        Public Class TISOTimeStamp

#Region "DECLARTIONS"

            Private fDateTime As DateTime
            Private _isValid As Boolean = True
            Public _month, _day, _year, _hour, _minute, _second, _msecond As UShort
            Public _GMTOffset As Byte = 0

#End Region 'DECLARTIONS

#Region "CONSTRUCTOR"
            Public Sub New()
                fDateTime = Date.Now()
                Me.IsValid = True
                Me.Year = fDateTime.Year
                Me.Month = fDateTime.Month
                Me.Day = fDateTime.Day
                Me.Hour = fDateTime.Hour
                Me.Minute = fDateTime.Minute
                Me.Second = fDateTime.Second
                Me.MSecond = fDateTime.Millisecond
            End Sub
#End Region 'CONSTRUCTOR

#Region "PROPERTIES"

            Property IsValid() As Boolean
                Get
                    Return _isValid
                End Get
                Set(ByVal value As Boolean)
                    _isValid = value
                End Set
            End Property

            Property Month() As UShort
                Get
                    Return _month
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, value, Me.Day, Me.Hour, Me.Minute, Me.Second, Me.MSecond)
                    _month = value
                End Set
            End Property

            Property Day() As UShort
                Get
                    Return _day
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, Me.Month, value, Me.Hour, Me.Minute, Me.Second, Me.MSecond)
                    _day = value
                End Set
            End Property

            Property Year() As UShort
                Get
                    Return _year
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(value, Me.Month, Me.Day, Me.Hour, Me.Minute, Me.Second, Me.MSecond)
                    _year = value
                End Set
            End Property

            Property Hour() As UShort
                Get
                    Return _hour
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, Me.Month, Me.Day, value, Me.Minute, Me.Second, Me.MSecond)
                    _hour = value
                End Set
            End Property

            Property Minute() As UShort
                Get
                    Return _minute
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, Me.Month, Me.Day, Me.Hour, value, Me.Second, Me.MSecond)
                    _minute = value
                End Set
            End Property

            Property Second() As UShort
                Get
                    Return _second
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, Me.Month, Me.Day, Me.Hour, Me.Minute, value, Me.MSecond)
                    _second = value
                End Set
            End Property

            Property MSecond() As UShort
                Get
                    Return _msecond
                End Get
                Set(ByVal value As UShort)
                    'SetDateTime(Me.Year, Me.Month, Me.Day, Me.Hour, Me.Minute, Me.Second, value)
                    _msecond = value
                End Set
            End Property

            Property GMTOffset() As Byte
                Get
                    Return _GMTOffset
                End Get
                Set(ByVal value As Byte)
                    _GMTOffset = value
                End Set
            End Property

#End Region 'PROPERTIES

#Region "PUBLIC METHODS"

            Public Sub GetDate(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort)
                pYear = fDateTime.Year
                pMonth = fDateTime.Month
                pDay = fDateTime.Day
            End Sub

            Public Function GetDate()
                Return fDateTime.Month.ToString() + "/" + fDateTime.Day.ToString() + "/" + fDateTime.Year.ToString()
            End Function

            Public Sub SetDate(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort)
                Dim y, m, d As UShort
                Dim hh, mm, ss, ms As UShort
                GetDateTime(y, m, d, hh, mm, ss, ms)
                SetDateTime(pYear, pMonth, pDay, hh, mm, ss, ms)
            End Sub

            Public Sub GetTime(ByVal pHour As UShort, ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort)
                pHour = fDateTime.Hour
                pMinute = fDateTime.Minute
                pSecond = fDateTime.Second
                pMSecond = fDateTime.Millisecond
            End Sub

            Public Function GetTime()
                Return fDateTime.Hour.ToString() + ":" + fDateTime.Minute.ToString() + ":" + fDateTime.Second.ToString() + ":" + fDateTime.Millisecond.ToString()
            End Function

            Public Sub SetTime(ByVal pHour As UShort, ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort)
                Dim y, m, d As UShort
                Dim hh, mm, ss, ms As UShort
                GetDateTime(y, m, d, hh, mm, ss, ms)
                SetDateTime(y, m, d, pHour, pMinute, pSecond, pMSecond)
            End Sub

            Public Sub GetTimeGMT(ByVal pHour As UShort, ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort, _
                                  ByVal pGMTOffset As Byte)
                GetTime(pHour, pMinute, pSecond, pMSecond)
                pGMTOffset = Me.GMTOffset
            End Sub

            Public Sub SetTimeGMT(ByVal pHour As UShort, ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort, _
                                  ByVal pGMTOffset As Byte)

                Me.GMTOffset = pGMTOffset
                SetTime(pHour, pMinute, pSecond, pMSecond)
            End Sub

            Public Sub GetDateTime(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort, ByVal pHour As UShort, _
                                  ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort)
                GetDate(pYear, pMonth, pDay)
                GetTime(pHour, pMinute, pSecond, pMSecond)
            End Sub

            Public Function GetDateTime()
                Return GetDate() + " " + GetTime()
            End Function

            Public Sub SetDateTime(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort, ByVal pHour As UShort, _
                                  ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort)
                Try
                    Dim dt As DateTime = New DateTime(pYear, pMonth, pDay, pHour, pMinute, pSecond, pMSecond)
                    Me.fDateTime = dt
                    Me.IsValid = True
                Catch ex As Exception
                    Debug.WriteLine("Error while set dateTime: " & ex.Message)
                    Me.IsValid = False
                    Me.GMTOffset = 0
                End Try
            End Sub

            Public Sub GetDateTimeGMT(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort, ByVal pHour As UShort, _
                                  ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort, ByVal pGMTOffset As Byte)
                pGMTOffset = Me.GMTOffset
                GetDateTime(pYear, pMonth, pDay, pHour, pMinute, pSecond, pMSecond)
            End Sub

            Public Sub SetDateTimeGMT(ByVal pYear As UShort, ByVal pMonth As UShort, ByVal pDay As UShort, ByVal pHour As UShort, _
                                  ByVal pMinute As UShort, ByVal pSecond As UShort, ByVal pMSecond As UShort, ByVal pGMTOffset As Byte)
                Me.GMTOffset = pGMTOffset
                SetDateTime(pYear, pMonth, pDay, pHour, pMinute, pSecond, pMSecond)
            End Sub

#End Region 'PUBLIC METHODS

        End Class

        Public Class TISOPathItem

#Region "DECLARTIONS"
            Public _name As String
            Public _index As UShort
            Public _sector As Integer
#End Region 'DECLARTIONS

#Region "PROPERTIES"
            Property Name() As UShort
                Get
                    Return _name
                End Get
                Set(ByVal value As UShort)
                    _name = value
                End Set
            End Property

            Property Index() As UShort
                Get
                    Return _index
                End Get
                Set(ByVal value As UShort)
                    _index = value
                End Set
            End Property

            Property Sector() As UShort
                Get
                    Return _sector
                End Get
                Set(ByVal value As UShort)
                    _sector = value
                End Set
            End Property
#End Region 'PROPERTIES

        End Class

        Public Class TISOPathTable

#Region "DECLARTIONS"
            Private _items As ArrayList
            Private _isValid As Boolean
#End Region 'DECLARATIONS

#Region "CONSTRUCTOR"
            Public Sub New()
                Me.Items = New ArrayList()
            End Sub
#End Region 'CONSTRUCTOR

#Region "PROPERTIES"
            Property IsValid() As Boolean
                Get
                    Return _isValid
                End Get
                Set(ByVal value As Boolean)
                    _isValid = value
                End Set
            End Property

            Property Items() As ArrayList
                Get
                    Return _items
                End Get
                Set(ByVal value As ArrayList)
                    _items = value
                End Set
            End Property
#End Region 'PROPERTIES

#Region "PRIVATE METHODS"
            Private Function GetCount() As Integer
                Return Me.Items.Count
            End Function

            Private Function GetItem(ByVal index As Integer) As TISOPathItem
                Return CType(Me.Items.Item(index), TISOPathItem)
            End Function
#End Region 'PRIVATE METHODS

#Region "PUBLIC METHODS"
            'Public Sub Read(ByVal fs As cISO9660_FileSystem)
            '    Dim recSize, count As Byte
            '    Dim pos As Integer
            '    Dim ret As Integer
            '    Dim c As Char
            '    'Dim ts As New TISOTimeStamp()

            '    Me.IsValid = False
            '    'pos := FileSeek(fh, 0, 1);

            '    'fs.SeekToSector(0, 0, cISO9660_SectorReader.Origin.FILE_CURRENT)
            '    'pos = fs.sr.GetLongFilePosition()

            '    If fs.sr.IsEndOfFile() Then
            '        Debug.WriteLine("It reaches the EOF")
            '        Return
            '    End If

            '    pos = fs.sr.GetSectorPointer()


            '    recSize = fs.sr.FileRead64ByteB8()

            '    If recSize >= 33 Then
            '        'Original code: FileSeek(fh, 1, 1) I assume it skips one byte
            '        fs.sr.FileRead64ByteB8()
            '        '{Read Sector Location of Data}
            '        Me.Sector = fs.sr.FileRead64IntegerBB32()
            '        '{Read Data Length}
            '        Me.Length = fs.sr.FileRead64IntegerBB32()
            '        '{Read Time Stamp}
            '        Me.TimeStamp = ReadTimeStamp(fs)
            '        '{Read Flags}
            '        ReadFlags(fs)
            '        '{Read File Unit Size}
            '        Me.FileUnitSize = fs.sr.FileRead64ByteB8()
            '        '{Read Interleave Gap Size}
            '        Me.InterLeaveGap = fs.sr.FileRead64ByteB8()
            '        '{Read Volume Sequence Number}
            '        Me.VolumeSequence = fs.sr.FileRead64WordBB16()
            '        '{Read Name}
            '        count = fs.sr.FileRead64ByteB8()

            '        If count = 1 Then
            '            c = fs.sr.FileRead64CharB8()

            '            Select Case c
            '                Case "0"
            '                    Me.Name = "."
            '                Case "1"
            '                    Me.Name = ".."
            '                Case Else
            '                    Me.Name = CType(c, String)
            '            End Select
            '        Else
            '            Me.Name = fs.sr.FileRead64String(count)
            '        End If

            '        'Jump to end of record. 
            '        'Original function: Inc(pos, recsize), FileSeek(fh, pos, 0)
            '        'Increase pos (pos = pos + recsize)
            '        If Not fs.sr.LongFileSeek(pos + recSize, cISO9660_SectorReader.Origin.FILE_BEGIN) Then
            '            Debug.WriteLine("Error reading file")
            '        Else
            '            Me.IsValid = True
            '        End If
            '    End If
            'End Sub

            Public Function Add(ByVal item As TISOPathItem) As Integer
                Return Me.Items.Add(item)
            End Function

            Public Sub Clear()
                Me.Items.Clear()
            End Sub

            Public Sub Delete(ByVal index As Integer)
                If index >= 0 And index < GetCount() Then
                    Me.Items.RemoveAt(index)
                Else
                    Debug.WriteLine("The index is out of boundary, index = " & CType(index, String))
                End If
            End Sub

            Public Function IndexOf(ByVal item As TISOPathItem) As Integer
                'Return -1 if item cannot be found
                Return Me.Items.IndexOf(item)
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As TISOPathItem)
                'if index = Me.Items.Count, the item will be appended to the list
                If index >= 0 And index <= GetCount() Then
                    Me.Items.Insert(index, item)
                End If
            End Sub

            Public Sub Move(ByVal currentIndex As Integer, ByVal newIndex As Integer)
                If currentIndex >= 0 And newIndex >= 0 And currentIndex < GetCount() And newIndex < GetCount() Then
                    Dim temp As TISOPathItem = CType(Me.Items.Item(currentIndex), TISOPathItem)
                    Dim removeIndex = currentIndex
                    If currentIndex > newIndex Then
                        removeIndex = removeIndex + 1
                    End If

                    If currentIndex <> newIndex Then
                        Me.Items.Insert(newIndex, temp)
                        Me.Items.RemoveAt(removeIndex)
                    End If
                End If
            End Sub

            Public Function Remove(ByVal item As TISOPathItem) As Integer
                Me.Items.Remove(item)
            End Function

#End Region 'PUBLIC METHODS

        End Class

        Public Class TISOFileDescriptor

#Region "DECLARTIONS"

            Public Enum TISOFileDescriptorFlag
                fdHidden
                fdDirectory
                fdAssociatedFile
                fdRecordFormat
                fdPermissions
                fdFinalRecord
            End Enum

            Private _isValid As Boolean
            Private _sectorSize, _actualSectorSize, _sector, _length As Integer
            Private _timeStamp As TISOTimeStamp
            Private _flags As ArrayList
            Private _fileUnitSize As Byte
            Private _interLeaveGap As Byte
            Private _volumeSequence As UShort
            Private _name As String
            Public _items As ArrayList '{if this is a directory, file items go here}

#End Region

#Region "CONSTRUCTOR"

            Public Sub New(ByVal pSectorSize As Integer)
                Me.Items = New ArrayList()
                Me.TimeStamp = New TISOTimeStamp()
                Me.SectorSize = pSectorSize
            End Sub

#End Region 'CONSTRUCTOR

#Region "PROPERTIES"

            Property IsValid() As Boolean
                Get
                    Return _isValid
                End Get
                Set(ByVal value As Boolean)
                    _isValid = value
                End Set
            End Property
            Property SectorSize() As Integer
                Get
                    Return _sectorSize
                End Get
                Set(ByVal value As Integer)
                    _sectorSize = value
                End Set
            End Property
            Property ActualSectorSize() As UShort
                Get
                    Return _actualSectorSize
                End Get
                Set(ByVal value As UShort)
                    _actualSectorSize = value
                End Set
            End Property
            Property Sector() As Integer
                Get
                    Return _sector
                End Get
                Set(ByVal value As Integer)
                    _sector = value
                End Set
            End Property
            Property Length() As Integer
                Get
                    Return _length
                End Get
                Set(ByVal value As Integer)
                    _length = value
                End Set
            End Property
            Property TimeStamp() As TISOTimeStamp
                Get
                    Return _timeStamp
                End Get
                Set(ByVal value As TISOTimeStamp)
                    _timeStamp = value
                End Set
            End Property
            Property Flags() As ArrayList
                Get
                    Return _flags
                End Get
                Set(ByVal value As ArrayList)
                    _flags = value
                End Set
            End Property
            Property FileUnitSize() As Byte
                Get
                    Return _fileUnitSize
                End Get
                Set(ByVal value As Byte)
                    _fileUnitSize = value
                End Set
            End Property
            Property InterLeaveGap() As Byte
                Get
                    Return _interLeaveGap
                End Get
                Set(ByVal value As Byte)
                    _interLeaveGap = value
                End Set
            End Property
            Property VolumeSequence() As UShort
                Get
                    Return _volumeSequence
                End Get
                Set(ByVal value As UShort)
                    _volumeSequence = value
                End Set
            End Property
            Property Name() As String
                Get
                    Return _name
                End Get
                Set(ByVal value As String)
                    _name = value
                End Set
            End Property
            Property Items() As ArrayList
                Get
                    Return _items
                End Get
                Set(ByVal value As ArrayList)
                    _items = value
                End Set
            End Property

#End Region 'PROPERTIES

#Region "PRIVATE METHODS"

            Private Function GetCount() As Integer
                Return Me.Items.Count
            End Function

            Private Function GetItem(ByVal index As Integer) As TISOFileDescriptor
                Return CType(Me.Items(index), TISOFileDescriptor)
            End Function

#End Region 'PRIVATE METHODS

#Region "PUBLIC METHODS"

            Public Sub Read(ByVal fs As cISO9660_FileSystem, ByVal isRoot As Boolean)
                Dim recSize, count As Byte
                Dim pos As Integer
                Dim prePos As Integer
                Dim c As Char
                Dim ts As New TISOTimeStamp()

                Me.IsValid = False
                'pos := FileSeek(fh, 0, 1);
                If fs.sr.IsEndOfFile() Then
                    Debug.WriteLine("It reaches the EOF")
                    Return
                End If

                If Not isRoot AndAlso fs.VolumeDescriptor.ActualSectorSize = eSectorSizeEnum.WithHeader AndAlso fs.sr.GetSectorPointer() = 0 Then
                    fs.sr.SetSectorPointer(6)
                End If

                pos = fs.sr.GetSectorPointer()
                prePos = fs.sr.GetSectorPointer()
                recSize = fs.sr.FileRead64ByteB8()

                If recSize >= 33 Then
                    'Original code: FileSeek(fh, 1, 1) I assume it skips one byte
                    fs.sr.FileRead64ByteB8()
                    '{Read Sector Location of Data}
                    Me.Sector = fs.sr.FileRead64IntegerBB32()
                    '{Read Data Length}
                    Me.Length = fs.sr.FileRead64IntegerBB32()
                    '{Read Time Stamp}
                    Me.TimeStamp = ReadTimeStamp(fs)
                    '{Read Flags}
                    ReadFlags(fs)
                    '{Read File Unit Size}
                    Me.FileUnitSize = fs.sr.FileRead64ByteB8()
                    '{Read Interleave Gap Size}
                    Me.InterLeaveGap = fs.sr.FileRead64ByteB8()
                    '{Read Volume Sequence Number}
                    Me.VolumeSequence = fs.sr.FileRead64WordBB16()
                    '{Read Name}
                    count = fs.sr.FileRead64ByteB8()

                    If count = 1 Then
                        c = fs.sr.FileRead64CharB8()

                        Select Case c
                            Case "0"
                                Me.Name = "."
                            Case "1"
                                Me.Name = ".."
                            Case Else
                                Me.Name = CType(c, String)
                        End Select
                    Else
                        Dim tempName As String = fs.sr.FileRead64String(count)
                        If tempName.IndexOf(";") >= 0 Then
                            tempName = tempName.Substring(0, tempName.IndexOf(";"))
                        End If
                        Me.Name = tempName
                    End If

                    'Jump to end of record. 
                    'Original function: Inc(pos, recsize), FileSeek(fh, pos, 0)
                    'Increase pos (pos = pos + recsize)
                    fs.sr.SetSectorPointer(prePos + recSize)

                    If fs.sr.IsEndOfFile() AndAlso pos >= fs.VolumeDescriptor.ActualSectorSize Then
                        Debug.WriteLine("Error reading file")
                    Else
                        Me.IsValid = True
                    End If
                    'pos = fs.sr.GetSectorPointer()
                End If
            End Sub

            Public Function ReadTimeStamp(ByVal fs As cISO9660_FileSystem) As TISOTimeStamp
                Dim Out As New TISOTimeStamp
                Dim y, m, d, hh, mm, ss As UShort
                Dim gmt As Byte

                y = CType(fs.sr.FileRead64ByteB8() + 1900, UShort)
                m = CType(fs.sr.FileRead64ByteB8(), UShort)
                d = CType(fs.sr.FileRead64ByteB8(), UShort)
                hh = CType(fs.sr.FileRead64ByteB8(), UShort)
                mm = CType(fs.sr.FileRead64ByteB8(), UShort)
                ss = CType(fs.sr.FileRead64ByteB8(), UShort)
                gmt = fs.sr.FileRead64ByteB8()
                Out.SetDateTimeGMT(y, m, d, hh, mm, ss, 0, gmt)
                Return Out
            End Function

            Public Sub ReadFlags(ByVal fs As cISO9660_FileSystem)
                Dim b As Byte
                b = fs.sr.FileRead64ByteB8()

                If Me.Flags Is Nothing Then
                    Me.Flags = New ArrayList()
                End If
                If (b And &H1) = &H1 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdHidden)
                End If

                If (b And &H2) = &H2 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdDirectory)
                End If

                If (b And &H4) = &H4 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdAssociatedFile)
                End If

                If (b And &H8) = &H8 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdRecordFormat)
                End If

                If (b And &H10) = &H10 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdPermissions)
                End If

                If (b And &H80) = &H80 Then
                    Me.Flags.Add(TISOFileDescriptorFlag.fdFinalRecord)
                End If
            End Sub

            Public Sub ReadSubdirectories(ByVal fs As cISO9660_FileSystem)
                Dim fd As TISOFileDescriptor
                Dim pos, endpos As Long
                Dim done As Boolean
                Dim al As ArrayList = Me.Flags
                'Me.SectorSize = fs.sr.GetSectorSize()
                Me.ActualSectorSize = fs.sr.GetSectorSize()


                If Me.IsValid AndAlso al.Contains(TISOFileDescriptorFlag.fdDirectory) Then
                    '{First clear any existing items}
                    Clear()
                    '{Read directory listing that this object points to}
                    'Don't have to minus one in this case
                    If Not fs.SeekToSector(Me.Sector + 1, Me.ActualSectorSize) Then
                        Debug.WriteLine("Error reading file")
                        Exit Sub
                    End If

                    '{Calculate end position}
                    endpos = (Me.Sector + 1) * Me.ActualSectorSize + Me.Length
                    done = False

                    Do
                        fd = New TISOFileDescriptor(Me.ActualSectorSize)
                        fd.Read(fs, False)

                        If fd.IsValid Then
                            If fd.Name <> "." AndAlso fd.Name <> ".." Then
                                Me.Items.Add(fd)
                            End If

                            pos = fs.sr.GetSectorPointer()
                            If fs.sr.IsEndOfFile() Then
                                Debug.WriteLine("End of File")
                                Exit Sub
                            End If
                            'sector Pointer starts from 0
                            'Count the current offset
                            pos = pos + 1 + (fs.sr.GetCurrentLBN() - 1) * Me.ActualSectorSize
                            If (pos >= endpos) Then done = True
                        Else
                            done = True
                        End If
                    Loop While Not done

                    'Read subdirectories
                    Dim count = GetCount()
                    For i As Integer = 0 To GetCount() - 1
                        fd = CType(Me.Items(i), TISOFileDescriptor)
                        If Not fd Is Nothing Then
                            fd.ReadSubdirectories(fs)
                        End If
                    Next
                End If
            End Sub

            Public Function Add(ByVal item As TISOFileDescriptor) As Integer
                Me.Items.Add(item)
            End Function

            Public Sub Clear()
                Me.Items.Clear()
            End Sub

            Public Sub Delete(ByVal index As Integer)
                If index >= 0 And index < GetCount() Then
                    Me.Items.RemoveAt(index)
                Else
                    Debug.WriteLine("The index is out of boundary, index = " & CType(index, String))
                End If
            End Sub

            Public Function IndexOf(ByVal item As TISOFileDescriptor) As Integer
                'Return -1 if item cannot be found
                Return Me.Items.IndexOf(item)
            End Function

            Public Sub Insert(ByVal index As Integer, ByVal item As TISOFileDescriptor)
                'if index = Me.Items.Count, the item will be appended to the list
                If index >= 0 And index <= GetCount() Then
                    Me.Items.Insert(index, item)
                End If
            End Sub

            Public Sub Move(ByVal currentIndex As Integer, ByVal newIndex As Integer)

                If currentIndex >= 0 And newIndex >= 0 And currentIndex < GetCount() And newIndex < GetCount() Then
                    Dim temp As TISOPathItem = CType(Me.Items.Item(currentIndex), TISOPathItem)
                    Dim removeIndex = currentIndex
                    If currentIndex > newIndex Then
                        removeIndex = removeIndex + 1
                    End If

                    If currentIndex <> newIndex Then
                        Me.Items.Insert(newIndex, temp)
                        Me.Items.RemoveAt(removeIndex)
                    End If
                End If
            End Sub

            Public Function Remove(ByVal item As TISOFileDescriptor) As Integer
                Me.Items.Remove(item)
            End Function

#End Region 'PUBLIC METHODS

        End Class

        Public Class TISOVolumeDescriptor

#Region "DECLARATIONS"

            Public Enum TISOVolumeDescriptorType
                vtNone
                vtPrimary
                vtUnknown
                vtTerminator
            End Enum

            Public _volumeType As TISOVolumeDescriptorType
            Public _systemID, _volumeID As String
            Public _nSectors As Integer
            Public _volumeSetSize, _volumeSequence, _sectorSize, _actualSectorSize, _pathTableLocation As UShort
            Public _pathTableLength As Integer
            Public _rootDir As TISOFileDescriptor
            Public _volumeSetID, _publisherID, _dataPreparerID, _applicationID, _copyrightFileID, _abstractFileID, _biblioFileID As String
            Public _creationDate, _modificationDate, _expirationDate, _effectiveDate As TISOTimeStamp

#End Region 'DECLARATIONS

#Region "CONSTRUCTOR"

            Public Sub New()
                Me.SectorSize = 2048
                Me.RootDir = New TISOFileDescriptor(Me.SectorSize)
                Me.CreationDate = New TISOTimeStamp()
                Me.ModificationDate = New TISOTimeStamp()
                Me.ExpirationDate = New TISOTimeStamp()
                Me.EffectiveDate = New TISOTimeStamp()
            End Sub

#End Region 'CONSTRUCTOR

#Region "PROPERTIES"

            Property VolumeType() As TISOVolumeDescriptorType
                Get
                    Return _volumeType
                End Get
                Set(ByVal value As TISOVolumeDescriptorType)
                    _volumeType = value
                End Set
            End Property
            Property SystemID() As String
                Get
                    Return _systemID
                End Get
                Set(ByVal value As String)
                    _systemID = value
                End Set
            End Property
            Property VolumeID() As String
                Get
                    Return _volumeID
                End Get
                Set(ByVal value As String)
                    _volumeID = value
                End Set
            End Property
            Property NSectors() As Integer
                Get
                    Return _nSectors
                End Get
                Set(ByVal value As Integer)
                    _nSectors = value
                End Set
            End Property
            Property VolumeSetSize() As UShort
                Get
                    Return _volumeSetSize
                End Get
                Set(ByVal value As UShort)
                    _volumeSetSize = value
                End Set
            End Property
            Property VolumeSequence() As UShort
                Get
                    Return _volumeSequence
                End Get
                Set(ByVal value As UShort)
                    _volumeSequence = value
                End Set
            End Property
            Property SectorSize() As UShort
                Get
                    Return _sectorSize
                End Get
                Set(ByVal value As UShort)
                    _sectorSize = value
                End Set
            End Property
            Property ActualSectorSize() As UShort
                Get
                    Return _actualSectorSize
                End Get
                Set(ByVal value As UShort)
                    _actualSectorSize = value
                End Set
            End Property
            Property PathTableLength() As Integer
                Get
                    Return _pathTableLength
                End Get
                Set(ByVal value As Integer)
                    _pathTableLength = value
                End Set
            End Property
            Property PathTableLocation() As UShort
                Get
                    Return _pathTableLocation
                End Get
                Set(ByVal value As UShort)
                    _pathTableLocation = value
                End Set
            End Property
            Property RootDir() As TISOFileDescriptor
                Get
                    Return _rootDir
                End Get
                Set(ByVal value As TISOFileDescriptor)
                    _rootDir = value
                End Set
            End Property
            Property VolumeSetID() As String
                Get
                    Return _volumeSetID
                End Get
                Set(ByVal value As String)
                    _volumeSetID = value
                End Set
            End Property
            Property PublisherID() As String
                Get
                    Return _publisherID
                End Get
                Set(ByVal value As String)
                    _publisherID = value
                End Set
            End Property
            Property DataPreparerID() As String
                Get
                    Return _dataPreparerID
                End Get
                Set(ByVal value As String)
                    _dataPreparerID = value
                End Set
            End Property
            Property ApplicationID() As String
                Get
                    Return _applicationID
                End Get
                Set(ByVal value As String)
                    _applicationID = value
                End Set
            End Property
            Property CopyrightFileID() As String
                Get
                    Return _copyrightFileID
                End Get
                Set(ByVal value As String)
                    _copyrightFileID = value
                End Set
            End Property
            Property AbstractFileID() As String
                Get
                    Return _abstractFileID
                End Get
                Set(ByVal value As String)
                    _abstractFileID = value
                End Set
            End Property
            Property BiblioFileID() As String
                Get
                    Return _biblioFileID
                End Get
                Set(ByVal value As String)
                    _biblioFileID = value
                End Set
            End Property
            Property CreationDate() As TISOTimeStamp
                Get
                    Return _creationDate
                End Get
                Set(ByVal value As TISOTimeStamp)
                    _creationDate = value
                End Set
            End Property
            Property ModificationDate() As TISOTimeStamp
                Get
                    Return _modificationDate
                End Get
                Set(ByVal value As TISOTimeStamp)
                    _modificationDate = value
                End Set
            End Property
            Property ExpirationDate() As TISOTimeStamp
                Get
                    Return _expirationDate
                End Get
                Set(ByVal value As TISOTimeStamp)
                    _expirationDate = value
                End Set
            End Property
            Property EffectiveDate() As TISOTimeStamp
                Get
                    Return _effectiveDate
                End Get
                Set(ByVal value As TISOTimeStamp)
                    _effectiveDate = value
                End Set
            End Property

#End Region 'PROPERTIES

#Region "PRIVATE METHODS"

            Private Sub ReadPrimaryDescriptor(ByVal fs As cISO9660_FileSystem)
                '{Read System & Volume Identifiers}
                'Magic (1, "CD001", 1, 0)
                'Skip the 8th byte
                fs.sr.FileRead64ByteB8()

                'System identifier, length = 32 (2054: 14th - 45th)
                Me.SystemID = fs.sr.FileRead64String(32)
                'Volume identifier, length = 32 (2054: 46th - 77th)
                Me.VolumeID = fs.sr.FileRead64String(32)

                '{Read Total Number of Sectors}
                'Zero, length = 8 (2054: 78th - 85th)
                fs.sr.FileRead64String(8)
                'Number of sectors (both endian dword), length = 8 (2054: 86th - 93th)
                'Volume Space Size
                Me.NSectors = fs.sr.FileRead64IntegerBB32()

                '{Read Volume Set Size, Volume Sequence Number, & Sector Size}
                'Zero, length = 32 (2054: 93th - 125th)
                fs.sr.FileRead64String(32)

                'Volume set size (1, both endian word), length = 4 (2054: 126th - 129th)
                Me.VolumeSetSize = fs.sr.FileRead64WordBB16()
                'Volume sequence number (1, both endian word), length = 4 (2054: 130th - 133th)
                Me.VolumeSequence = fs.sr.FileRead64WordBB16()

                'Sector size (2048, both endian word), length = 4 (2054: 134th - 137th)
                Me.SectorSize = fs.sr.FileRead64WordBB16()
                Me.RootDir.SectorSize = Me.SectorSize
                Me.ActualSectorSize = fs.sr.GetSectorSize()
                Me.RootDir.ActualSectorSize = Me.ActualSectorSize

                '{Read Path Table Length & Location}
                'Path table length in bytes, length = 8 (2054: 138th - 145th)
                Me.PathTableLength = fs.sr.FileRead64IntegerBB32()

                'Number of first sector in first little endian path table, length = 4
                'Location of Occurrence of Type L Path Table
                fs.sr.FileRead64IntegerLB32()
                'Number of first sector in second little endian path table or 0, length = 4
                'Location of Optional Occurrence of Type L Path Table
                fs.sr.FileRead64IntegerLB32()
                'Number of first sector in first big endian path table, length = 4
                fs.sr.FileRead64IntegerMB32()
                'Number of first sector in second big endian path table or 0, length = 4
                fs.sr.FileRead64IntegerMB32()


                ''Me.PathTableLocation = fs.sr.FileRead64IntegerLB32()
                'fs.sr.FileRead64String(34)

                '{Read Root Directory Record}
                'FileSeek(fh, 12, 1);
                'fs.sr.FileRead64String(12)

                Me.RootDir.Read(fs, True)
                '{Read Volume & File Identifiers}

                Me.VolumeSetID = fs.sr.FileRead64String(128)
                Me.PublisherID = fs.sr.FileRead64String(128)
                Me.DataPreparerID = fs.sr.FileRead64String(128)
                Me.ApplicationID = fs.sr.FileRead64String(128)
                Me.CopyrightFileID = fs.sr.FileRead64String(37)
                Me.AbstractFileID = fs.sr.FileRead64String(37)
                Me.BiblioFileID = fs.sr.FileRead64String(37)
                '{Read Creation, Modification, Expiration, & Effective Dates}
                ReadTimeStamp(fs, Me.CreationDate)
                ReadTimeStamp(fs, Me.ModificationDate)
                ReadTimeStamp(fs, Me.ExpirationDate)
                ReadTimeStamp(fs, Me.EffectiveDate)
                '{Skip to end of sector}
                fs.sr.IncreaseLBN(0)
            End Sub

            Private Sub ReadTimeStamp(ByVal fs As cISO9660_FileSystem, ByVal ts As TISOTimeStamp)
                Dim y, m, d, hh, mm, ss, ms As UShort
                Dim gmt As Byte
                Dim s As String

                s = fs.sr.FileRead64String(16, True)
                y = CType(s.Substring(0, 4), UShort)
                m = CType(s.Substring(4, 2), UShort)
                d = CType(s.Substring(6, 2), UShort)
                hh = CType(s.Substring(8, 2), UShort)
                mm = CType(s.Substring(10, 2), UShort)
                ss = CType(s.Substring(12, 2), UShort)
                ms = CType(s.Substring(14, 2), UShort) * 10
                gmt = fs.sr.FileRead64ShortintB8()
                ts.SetDateTimeGMT(y, m, d, hh, mm, ss, ms, gmt)
            End Sub

#End Region 'PRIVATE METHODS

#Region "PUBLIC METHODS"

            Public Sub Read(ByVal fs As cISO9660_FileSystem)
                Dim b, b1 As Byte
                Dim s As String
                Me.ActualSectorSize = fs.sr.GetSectorSize()

                Dim result As Boolean = False
                result = fs.SeekToSector(cISO9660_FileSystem.CISOVolumeDescriptorLBN + 1, Me.ActualSectorSize)
                Dim pos As Long = fs.sr.GetCurrentLBN()


                Me.VolumeType = TISOVolumeDescriptorType.vtNone

                If (fs.sr.GetSectorSize() = eSectorSizeEnum.WithHeader) Then
                    fs.sr.SetSectorPointer(fs.sr.GetSectorPointer() + 6)
                End If

                '{Read Volume Descriptor ID}
                'Magic (1, "CD001", 1, 0)
                b = fs.sr.FileRead64ByteB8()
                s = fs.sr.FileRead64String(5)
                b1 = fs.sr.FileRead64ByteB8()

                '{Was an ISO9660 Volume Descriptor read?}
                If s.Equals(cISO9660_FileSystem.CISOVolumeDescriptorID) Then
                    '{Determine Volume Descriptor Type}
                    Select Case b
                        Case &H1
                            Me.VolumeType = TISOVolumeDescriptorType.vtPrimary
                            ReadPrimaryDescriptor(fs)
                            RootDir.ReadSubdirectories(fs)
                        Case &HFF
                            Me.VolumeType = TISOVolumeDescriptorType.vtTerminator
                        Case Else
                            Me.VolumeType = TISOVolumeDescriptorType.vtUnknown
                    End Select
                End If

                Me.VolumeType = b
            End Sub

            Public Function GetAFile(ByVal pFD As TISOFileDescriptor, ByVal pName As String) As TISOFileDescriptor
                If Not pFD Is Nothing Then
                    For Each fd As TISOFileDescriptor In pFD.Items
                        Dim name As String = fd.Name
                        If name.Equals(pName) Then
                            Return fd
                        End If
                        If fd.Items.Count > 0 Then
                            Dim tempFD = GetAFile(fd, pName)
                            If Not tempFD.Equals(Me.RootDir) Then
                                Return tempFD
                            End If
                        End If
                    Next
                End If
                Return Me.RootDir
            End Function

#End Region 'PUBLIC METHODS

        End Class

        Public Class TISO9660Volume

#Region "DELCARATIONS"
            Private _volumeDescriptor As TISOVolumeDescriptor
#End Region 'DELCARATIONS

#Region "CONSTRUCTOR"
            Public Sub New()
                Me.VolumeDescriptor = New TISOVolumeDescriptor()
            End Sub

#End Region 'CONSTRUCTOR

#Region "PROPERTIES"
            Property VolumeDescriptor() As TISOVolumeDescriptor
                Get
                    Return _volumeDescriptor
                End Get
                Set(ByVal value As TISOVolumeDescriptor)
                    _volumeDescriptor = value
                End Set
            End Property

#End Region 'PROPERTIES

#Region "PUBLIC METHODS"

            Public Sub ReadVolumeInfo(ByVal fs As cISO9660_FileSystem)
                Dim sector As Integer = 16

                Try
                    fs.SeekToSector(sector, Me.VolumeDescriptor.SectorSize)
                    Me.VolumeDescriptor.Read(fs)

                    If Me.VolumeDescriptor.VolumeType <> TISOVolumeDescriptor.TISOVolumeDescriptorType.vtPrimary Then
                        Debug.WriteLine("Primary Volume Descriptor not found.")
                    End If
                Catch ex As Exception
                    Debug.WriteLine("Error: Unable to read ISO9660 Primary Volume Descriptor. File may be corrupt or is not a disc image")
                End Try
            End Sub

            Public Sub ReadPathTable(ByVal fh As Integer)
                'not implemented yet
            End Sub

#End Region 'PUBLIC METHODS

        End Class

#End Region 'CLASSES

    End Class

End Namespace
