Imports System.IO
Imports System.Windows.Forms
Imports SMT.FileSystems.ISO9660

Namespace Multimedia.Formats.DDP

#Region "CONTROL.DAT"

    Public Class cCONTROL_DAT

#Region "DECLARTIONS"

        Public _CPS_TY, _RMA As Byte
        Public _discManufInfo, _contentProviderInfo As String
        Public SectorSize As Integer
        Private ControlDat_Size As Integer = 32768 'Control_Sectors_Count * 2048
        Private ControlDatWithHeader_Size As Integer = 32864
        Private Control_Sectors_Count As Integer = 16

#End Region 'DECLARTIONS

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByVal ControlDatPath As String, Optional ByVal desiredSectorSize As Integer = eSectorSizeEnum.WithHeader)
            Try
                _ControlDatPath = ControlDatPath
                Dim fileStream As FileStream = File.Open(Me.ControlDatPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                SetSectorSize(fileStream.Length)
                Dim controlData() As Byte

                If Me.SectorSize = 2048 Then
                    ReDim controlData(2048 * Control_Sectors_Count - 1)
                    CopyDataFromStreamToArray(fileStream, controlData)
                    GetMainDataControlDat(controlData)
                Else
                    ReDim controlData(2054 * Control_Sectors_Count - 1)
                    CopyDataFromStreamToArray(fileStream, controlData)
                    GetCPR_MAIData(controlData)
                    GetMainDataControlDat(controlData, True)
                End If

                fileStream.Close()

            Catch ex As Exception
                Throw New Exception("Problem with New() cDDPID. Error: " & ex.Message, ex)
                'Throw New Exception("Cannot open image file: " & ControlDatPath & ". Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub Dispose()

        End Sub

#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "FUNCTIONALITY"

        Private Sub GetMainDataControlDat(ByVal controlData() As Byte, Optional ByVal sectorWithHeader As Boolean = False)
            'Disc Manufacturing Info. RSN = 1, RBN = 0-2047
            Dim tempArray(2047) As Byte
            Dim offset As Integer = 2048
            If sectorWithHeader Then offset = 2054 + 6 'Skip the header

            Array.ConstrainedCopy(controlData, offset, tempArray, 0, 2048)
            Me.DiscManufInfo = GetString(tempArray)

            'Content Provider Info. RSN = 2-15, RBN = 0-2047
            Me.CPS_TY = 0
            Array.Clear(tempArray, 0, 2048)

            offset = offset * 2
            If sectorWithHeader Then offset = 2054 * 2 + 6 'Skip the header
            Array.ConstrainedCopy(controlData, offset, tempArray, 0, 2048)
            Me.ContentProviderInfo = GetString(tempArray)

        End Sub

        Private Sub GetCPR_MAIData(ByVal controlData() As Byte)
            'Disc Manufacturing Info. RSN = 1, RBN = 0-5
            Dim tempArray() As Byte = {controlData(2054), controlData(2055), controlData(2056), controlData(2057), controlData(2058), controlData(2059)}
            Me.DiscManufInfo = GetString(tempArray)

            'Content Provider Info. RSN = 2-15, RBN = 0
            Me.CPS_TY = controlData(2054 * 2)
            Me.RMA = controlData(2054 * 2 + 5)

        End Sub

        Private Sub SetSectorSize(ByVal fileLength As Integer)
            If fileLength = ControlDat_Size Then
                SectorSize = 2048
            Else
                SectorSize = 2054
            End If
        End Sub

#End Region 'FUNCTIONALITY

#Region "PROPERTIES"

        Public ReadOnly Property ControlDatPath() As String
            Get
                Return _ControlDatPath
            End Get
        End Property
        Private _ControlDatPath As String

        Property CPS_TY() As Byte
            Get
                Return _CPS_TY
            End Get
            Set(ByVal value As Byte)
                _CPS_TY = value
            End Set
        End Property

        Property RMA() As Byte
            Get
                Return _RMA
            End Get
            Set(ByVal value As Byte)
                _RMA = value
            End Set
        End Property

        'Property DiscManufInfo() As Byte()
        '    Get
        '        Return _discManufInfo
        '    End Get
        '    Set(ByVal value As Byte())
        '        _discManufInfo = value
        '    End Set
        'End Property
        Property DiscManufInfo() As String
            Get
                Return _discManufInfo
            End Get
            Set(ByVal value As String)
                _discManufInfo = value
            End Set
        End Property

        Property ContentProviderInfo() As String
            Get
                Return _contentProviderInfo
            End Get
            Set(ByVal value As String)
                _contentProviderInfo = value
            End Set
        End Property

#End Region 'PROPERTIES

    End Class

#End Region 'CONTROL.DAT

#Region "MAIN.DAT"

    Public Class cMAIN_DAT
        'TRPF: the ISO9660 code gets most of the information from this file.
        'but if we want to get some of the info from the tail of the file we'd do that here.

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByVal SourceA As String, Optional ByVal desiredSectorSize As Integer = eSectorSizeEnum.WithHeader, Optional ByVal SourceB As String = "")
            _SourceADir = SourceA
            _SourceBDir = SourceB
        End Sub

        Public Sub Dispose()

        End Sub

#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "PROPERTIES"

        Public ReadOnly Property SourceADir() As String
            Get
                Return _SourceADir
            End Get
        End Property
        Private _SourceADir As String

        Public ReadOnly Property SourceBDir() As String
            Get
                Return _SourceBDir
            End Get
        End Property
        Private _SourceBDir As String

#End Region 'PROPERTIES

    End Class

#End Region 'MAIN.DAT

#Region "DDPID"

    Public Class cDDPID

#Region "DECLARTIONS"

        Private DDPID_Size As Integer = 384
        Private DDPID_FileName As String = "\DDPID"
        Public _nside, _side, _nlayer, _layer, _dir, _SSCRST As Char
        Public _type As String
        Public _mID, _size, _txt As String

#End Region 'DECLARTIONS

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByVal DDPIDPath As String)
            Try
                _DDPIDPath = DDPIDPath

                Dim fileStream As FileStream = File.Open(_DDPIDPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim DDPID_Data(fileStream.Length - 1) As Byte

                CopyDataFromStreamToArray(fileStream, DDPID_Data)

                Dim tempArray(47) As Byte 'total 48 bytes
                Array.ConstrainedCopy(DDPID_Data, 38, tempArray, 0, 48)

                Me.MID = GetString(tempArray)
                Me.Type = ChrW(DDPID_Data(87)) & ChrW(DDPID_Data(88))
                Me.NSide = ChrW(DDPID_Data(89))
                Me.Side = ChrW(DDPID_Data(90))
                Me.NLayer = ChrW(DDPID_Data(91))
                Me.Layer = ChrW(DDPID_Data(92))
                Me.Dir = ChrW(DDPID_Data(93))
                Me.SSCRST = ChrW(DDPID_Data(95))
                Me.Size = ChrW(DDPID_Data(97)) & ChrW(DDPID_Data(98))

                ReDim tempArray(28) 'total 29 bytes
                Array.ConstrainedCopy(DDPID_Data, 99, tempArray, 0, 29)
                Me.TXT = GetString(tempArray)

                fileStream.Close()

            Catch ex As Exception
                Throw New Exception("Problem with New() cDDPID. Error: " & ex.Message, ex)
                'Throw New Exception("Cannot open image file: " & _DDPIDPath & ". Error: " & ex.Message, ex)
            End Try

        End Sub

        Public Sub Dispose()

        End Sub

#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "FUNCTIONALITY"
        Public Function GetDirectionString(ByVal dir As Char) As String
            If dir = "O" OrElse dir = "o" Then
                Return "OTP"
            Else
                Return "PTP"
            End If
        End Function
#End Region 'FUNCTIONALITY

#Region "PROPERTIES"

        Public ReadOnly Property DDPIDPath() As String
            Get
                Return _DDPIDPath
            End Get
        End Property
        Private _DDPIDPath As String

        Property MID() As String
            Get
                Return _mID
            End Get
            Set(ByVal value As String)
                _mID = value
            End Set
        End Property

        Property Type() As String
            Get
                Return _type
            End Get
            Set(ByVal value As String)
                _type = value
            End Set
        End Property

        Property NSide() As Char
            Get
                Return _nside
            End Get
            Set(ByVal value As Char)
                _nside = value
            End Set
        End Property

        Property Side() As Char
            Get
                Return _side
            End Get
            Set(ByVal value As Char)
                _side = value
            End Set
        End Property

        Property NLayer() As Char
            Get
                Return _nlayer
            End Get
            Set(ByVal value As Char)
                _nlayer = value
            End Set
        End Property

        Property Layer() As Char
            Get
                Return _layer
            End Get
            Set(ByVal value As Char)
                _layer = value
            End Set
        End Property

        Property Dir() As Char
            Get
                Return _dir
            End Get
            Set(ByVal value As Char)
                _dir = value
            End Set
        End Property

        Property SSCRST() As Char
            Get
                Return _SSCRST
            End Get
            Set(ByVal value As Char)
                _SSCRST = value
            End Set
        End Property

        Property Size() As String
            Get
                Return _size
            End Get
            Set(ByVal value As String)
                _size = value
            End Set
        End Property

        Property TXT() As String
            Get
                Return _txt
            End Get
            Set(ByVal value As String)
                _txt = value
            End Set
        End Property

#End Region 'PROPERTIES

    End Class

#End Region 'DDPID

#Region "DDPMS"
    Public Class cDDPMS

#Region "DECLARTIONS"

        Private DDPMS_FileName As String = "\DDPMS"
        Public _SCR, _MainSCR As Char
        Public _DST, _MainDST, _DSL, _MainDSL As String

#End Region 'DECLARTIONS

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByVal DDPMSPath As String)
            Try
                _DDPMSPath = DDPMSPath
                Dim greatThan128 As Boolean = True

                Dim fileStream As FileStream = File.Open(_DDPMSPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim DDPMS_Data(fileStream.Length - 1) As Byte

                If fileStream.Length <= 128 Then
                    greatThan128 = False
                End If
                CopyDataFromStreamToArray(fileStream, DDPMS_Data)

                If greatThan128 Then
                    'Data Stream Type. Leading Control Data: D2. Image File Data: D0
                    Me.DST = ChrW(DDPMS_Data(4)) & ChrW(DDPMS_Data(5))

                    Dim tempArray(7) As Byte
                    Array.ConstrainedCopy(DDPMS_Data, 14, tempArray, 0, 8)
                    Me.DSL = GetString(tempArray)

                    'Byte 169 on spec, 169 - 128(DDPID size) = 41
                    Me.SCR = ChrW(DDPMS_Data(41))

                    'Data Stream Type. Leading Control Data: D2. Image File Data: D0
                    Me.MainDST = ChrW(DDPMS_Data(132)) & ChrW(DDPMS_Data(133))

                    'Byte 270-277, 270 - 128 = 142
                    Array.Clear(tempArray, 0, tempArray.Length)
                    Array.ConstrainedCopy(DDPMS_Data, 142, tempArray, 0, 8)
                    Me.MainDSL = GetString(tempArray)

                    'Byte 169 on spec, 169 - 128(DDPID size) = 41
                    Me.MainSCR = ChrW(DDPMS_Data(169))

                Else
                    'Data Stream Type. Leading Control Data: D2. Image File Data: D0
                    Me.MainDST = ChrW(DDPMS_Data(4)) & ChrW(DDPMS_Data(5))

                    Dim tempArray(7) As Byte
                    Array.ConstrainedCopy(DDPMS_Data, 14, tempArray, 0, 8)
                    Me.MainDSL = GetString(tempArray)

                    'Byte 169 on spec, 169 - 128(DDPID size) = 41
                    Me.MainSCR = ChrW(DDPMS_Data(41))

                End If

                fileStream.Close()

            Catch ex As Exception
                Throw New Exception("Problem with New() cDDPMS. Error: " & ex.Message, ex)
                'Throw New Exception("Cannot open image file: " & _DDPMSPath & ". Error: " & ex.Message, ex)
            End Try

        End Sub

        Public Sub Dispose()

        End Sub


#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "FUNCTIONALITY"

#End Region 'FUNCTIONALITY

#Region "PROPERTIES"

        Public ReadOnly Property DDPMSPath() As String
            Get
                Return _DDPMSPath
            End Get
        End Property
        Private _DDPMSPath As String

        Property SCR() As Char
            Get
                Return _SCR
            End Get
            Set(ByVal value As Char)
                _SCR = value
            End Set
        End Property

        Property DST() As String
            Get
                Return _DST
            End Get
            Set(ByVal value As String)
                _DST = value
            End Set
        End Property

        Property DSL() As String
            Get
                Return _DSL
            End Get
            Set(ByVal value As String)
                _DSL = value
            End Set
        End Property

        Property MainSCR() As Char
            Get
                Return _MainSCR
            End Get
            Set(ByVal value As Char)
                _MainSCR = value
            End Set
        End Property

        Property MainDST() As String
            Get
                Return _MainDST
            End Get
            Set(ByVal value As String)
                _MainDST = value
            End Set
        End Property

        Property MainDSL() As String
            Get
                Return _MainDSL
            End Get
            Set(ByVal value As String)
                _MainDSL = value
            End Set
        End Property

#End Region 'PROPERTIES
    End Class
#End Region 'DDPMS

    Public Module mDDPReaderShared

        Public Function GetByte(ByVal InByte As Byte) As String
            Try
                Dim O As String = Hex(InByte)
                If O.Length = 1 Then O = "0" & O
                Return O
            Catch ex As Exception
                Throw New Exception("Problem with GetByte. Error: " & ex.Message)
                Return ""
            End Try
        End Function

        Public Function GetString(ByVal bytes() As Byte, Optional ByVal expectDigits As Boolean = False) As String
            Dim c As Char
            Dim count As Integer = bytes.Length() - 1
            Dim result As String = ""
            For i As Integer = 0 To count
                c = Microsoft.VisualBasic.ChrW(bytes(i))
                If expectDigits AndAlso c = Nothing Then
                    c = "0"
                End If
                result = result & c
            Next
            Return result
        End Function

        Public Sub CopyDataFromStreamToArray(ByVal fileStream As FileStream, ByVal byteArray() As Byte)
            If Not fileStream Is Nothing Then

                Dim tRemaining, tOffset, count, read As Integer
                tRemaining = byteArray.Length
                tOffset = 0
                count = 0
                read = 0

                Do While tRemaining > 0
                    read = fileStream.Read(byteArray, tOffset, tRemaining)


                    If (read <= 0) Then
                        Throw New EndOfStreamException
                        String.Format("End of stream reached with {0} bytes left to read", tRemaining)
                    End If

                    'in case the file stream is unable to read the whole sector at a time
                    tRemaining -= read
                    tOffset += read
                Loop
            End If

            fileStream.Close()
        End Sub

    End Module

End Namespace
