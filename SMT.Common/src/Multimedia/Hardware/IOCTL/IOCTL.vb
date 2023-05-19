Imports System.Runtime.InteropServices
Imports SMT.DotNet.Utility

Namespace Multimedia.Hardware.IOCTL

    Public Class cDVDIOCTL

#Region "DVD_REGION"

        <DllImport("IOCTL_DVD.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function ReadDVDRegion(ByVal drive As Char, ByRef dvd_region As DVD_REGION) As Integer
        End Function

        Public Function GetDVD_REGION(ByVal Drive As Char) As DVD_REGION
            Try
                Dim out As New DVD_REGION
                Dim i As Integer = Me.ReadDVDRegion(Drive, out)
                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetDVD_REGION. Error: " & ex.Message)
            End Try
        End Function

        <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
        Public Structure DVD_REGION
            Public CopySystem As Byte
            Public RegionData As Byte
            Public SystemRegion As Byte
            Public ResetCount As Byte
        End Structure

        Public Enum eCopySystem
            NoCopyProtection = 0
            CSS___CPPM = 1
            CPRM = 2
        End Enum

#End Region 'DVD_REGION

#Region "DVD_LAYER_DESCRIPTOR"

        <DllImport("IOCTL_DVD.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function ReadStructure_Layer(ByVal drive As Char, ByRef layer As DVD_LAYER_DESCRIPTOR_Q) As Integer
        End Function

        Public Function GetDVD_LAYER_DESCRIPTOR(ByVal Drive As Char) As DVD_LAYER_DESCRIPTOR
            Try
                Dim Q As New DVD_LAYER_DESCRIPTOR_Q
                Dim i As Long = ReadStructure_Layer(Drive, Q)
                Dim out As New DVD_LAYER_DESCRIPTOR

                out.BookVersion = (Q.BookVersionAndBookType >> 0) And 15
                out.BookType = (Q.BookVersionAndBookType >> 4) And 15

                out.MinimumRate = (Q.MinimumRateAndDiskSize >> 0) And 15
                out.DiskSize = (Q.MinimumRateAndDiskSize >> 4) And 15

                out.LayerType = (Q.LayerTypeAndTrackPathAndNumberOfLayersAndReserved1 >> 0) And 15
                out.TrackPath = (Q.LayerTypeAndTrackPathAndNumberOfLayersAndReserved1 >> 4) And 1
                out.NumberOfLayers = (Q.LayerTypeAndTrackPathAndNumberOfLayersAndReserved1 >> 5) And 3
                out.Reserved1 = (Q.LayerTypeAndTrackPathAndNumberOfLayersAndReserved1 >> 7) And 1

                out.TrackDensity = (Q.TrackDensityAndLinearDensity >> 0) And 15
                out.LinearDensity = (Q.TrackDensityAndLinearDensity >> 4) And 15

                out.StartingDataSector = Q.StartingDataSector
                out.EndLayerZeroSector = Q.EndLayerZeroSector
                out.EndDataSector = Q.EndDataSector

                out.Reserved5 = (Q.Reserved5AndBCAFlag >> 0) And 15
                out.BCAFlag = (Q.Reserved5AndBCAFlag >> 4) And 15

                out.Reserved6 = Q.Reserved6

                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetDVD_LAYER_DESCRIPTOR. Error: " & ex.Message)
            End Try
        End Function

        <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
        Public Structure DVD_LAYER_DESCRIPTOR_Q
            Public BookVersionAndBookType As Byte
            Public MinimumRateAndDiskSize As Byte
            Public LayerTypeAndTrackPathAndNumberOfLayersAndReserved1 As Byte
            Public TrackDensityAndLinearDensity As Byte
            Public StartingDataSector As Int32
            Public EndDataSector As Int32
            Public EndLayerZeroSector As Int32
            Public Reserved5AndBCAFlag As Byte
            Public Reserved6 As Byte
        End Structure

        Public Class DVD_LAYER_DESCRIPTOR
            Public BookVersion As Byte

            Private _BookType As String
            Public Property BookType() As String
                Get
                    Return _BookType
                End Get
                Set(ByVal Value As String)
                    Select Case CShort(Value)
                        Case 0
                            Me._BookType = "DVD-ROM"
                        Case 1
                            Me._BookType = "DVD-RAM"
                        Case 2
                            Me._BookType = "DVD-R"
                        Case 3
                            Me._BookType = "DVD-RW"
                        Case 9
                            Me._BookType = "DVD+RW"
                        Case Else
                            Me._BookType = "* Unknown Value *"
                    End Select
                End Set
            End Property

            Private _MinumumRate As String
            Public Property MinimumRate() As String
                Get
                    Return _MinumumRate
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._MinumumRate = "2.52 Mbps"
                        Case 1
                            Me._MinumumRate = "5.04 Mbps"
                        Case 2
                            Me._MinumumRate = "10.08 Mbps"
                        Case 15
                            Me._MinumumRate = "Not Specified"
                        Case Else
                            Me._MinumumRate = "* Unknown Value *"
                    End Select
                End Set
            End Property

            Private _DiscSize As String
            Public Property DiskSize() As String
                Get
                    Return _DiscSize
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._DiscSize = "120mm"
                        Case 1
                            Me._DiscSize = "80mm"
                        Case Else
                            Me._DiscSize = "* Unknown Value * "
                    End Select
                End Set
            End Property

            Private _LayerType As String
            Public Property LayerType() As String
                Get
                    Return _LayerType
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 1
                            Me._LayerType = "ReadOnly"
                        Case 2
                            Me._LayerType = "Recordable"
                        Case 4
                            Me._LayerType = "Rewritable"
                        Case Else
                            Me._LayerType = "* Unknown Value *"""
                    End Select
                End Set
            End Property

            Private _TrackPath As String
            Public Property TrackPath() As String
                Get
                    Return Me._TrackPath
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._TrackPath = "Parallel"
                        Case 1
                            Me._TrackPath = "Opposite"
                        Case Else
                            Me._TrackPath = "* Unknown Value *"
                    End Select
                End Set
            End Property

            Private _NumberOfLayers As String
            Public Property NumberOfLayers() As String
                Get
                    Return Me._NumberOfLayers
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._NumberOfLayers = "One"
                        Case 1
                            Me._NumberOfLayers = "Two"
                        Case Else
                            Me._NumberOfLayers = "* Unknown Value *"

                    End Select
                End Set
            End Property

            Public Reserved1 As Byte

            Private _TrackDensity As String
            Public Property TrackDensity() As String
                Get
                    Return _TrackDensity
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._TrackDensity = "0.74 um/track"
                        Case 1
                            Me._TrackDensity = "0.80 um/track"
                        Case 2
                            Me._TrackDensity = "0.615 um/track"
                        Case Else
                            Me._TrackDensity = "* Unknown Value *"
                    End Select
                End Set
            End Property

            Private _LinearDensity As String
            Public Property LinearDensity() As String
                Get
                    Return _LinearDensity
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._LinearDensity = "0.267 um/bit"
                        Case 1
                            Me._LinearDensity = "0.293 um/bit"
                        Case 2
                            Me._LinearDensity = "0.409 to 0.435 um/bit"
                        Case 4
                            Me._LinearDensity = "0.280 to 0.291 um/bit"
                        Case 8
                            Me._LinearDensity = "0.353 um/bit"
                        Case Else
                            Me._LinearDensity = "* Unknown Value *"
                    End Select
                End Set
            End Property

            Private _StartingDataSector As String
            Public Property StartingDataSector() As String
                Get
                    Return Me._StartingDataSector
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case &H30000
                            Me._StartingDataSector = "0x30000 (ROM, -R, -RW, +RW)"
                        Case &H31000
                            Me._StartingDataSector = "0x31000 (RAM)"
                        Case Else
                            Me._StartingDataSector = "Reserved"
                    End Select
                End Set
            End Property

            Private _EndDataSector As String
            Public Property EndDataSector() As String
                Get
                    Return "0x" & _EndDataSector
                End Get
                Set(ByVal Value As String)
                    Me._EndDataSector = DecToHex(Value)
                End Set
            End Property

            Private _EndLayerZeroSector As String
            Public Property EndLayerZeroSector() As String
                Get
                    Return "0x" & _EndLayerZeroSector
                End Get
                Set(ByVal Value As String)
                    Me._EndLayerZeroSector = DecToHex(Value)
                End Set
            End Property

            Public Reserved5 As Byte

            Private _BCAFlag As String
            Public Property BCAFlag() As String
                Get
                    Return Me._BCAFlag
                End Get
                Set(ByVal Value As String)
                    Select Case Value
                        Case 0
                            Me._BCAFlag = "False"
                        Case 1
                            Me._BCAFlag = "True"
                    End Select
                End Set
            End Property

            Public Reserved6 As Byte

        End Class

#End Region 'DVD_LAYER_DESCRIPTOR

#Region "DVD_COPYRIGHT_DESCRIPTOR"

        <DllImport("IOCTL_DVD.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function ReadStructure_Copyright(ByVal drive As Char, ByRef cd As DVD_COPYRIGHT_DESCRIPTOR) As Integer
        End Function

        Public Function GetDVD_COPYRIGHT_DESCRIPTOR(ByVal Drive As Char) As DVD_COPYRIGHT_DESCRIPTOR
            Try
                Dim out As New DVD_COPYRIGHT_DESCRIPTOR
                Dim i As Integer = Me.ReadStructure_Copyright(Drive, out)
                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetDVD_COPYRIGHT_DESCRIPTOR. Error: " & ex.Message)
            End Try
        End Function

        <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
        Public Structure DVD_COPYRIGHT_DESCRIPTOR
            Public CopyrightProtectionType As Byte
            Public RegionManagementInformation As Byte
            Public Reserved As Byte
        End Structure

#End Region 'DVD_COPYRIGHT_DESCRIPTOR

#Region "DVD_BCA_DESCRIPTOR"

        <DllImport("IOCTL_DVD.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function ReadStructure_BCA(ByVal drive As Char, ByRef bca As DVD_BCA_DESCRIPTOR) As Integer
        End Function

        Public Function GetDVD_BCA_DESCRIPTOR(ByVal Drive As Char) As DVD_BCA_DESCRIPTOR
            Try
                Dim out As New DVD_BCA_DESCRIPTOR
                ReDim out.BCAInformation(255)
                Dim i As Integer = Me.ReadStructure_BCA(Drive, out)
                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetDVD_BCA_DESCRIPTOR. Error: " & ex.Message)
            End Try
        End Function

        <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
        Public Structure DVD_BCA_DESCRIPTOR
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=256)> _
            Public BCAInformation As Byte()
        End Structure

#End Region 'DVD_BCA_DESCRIPTOR

#Region "DVD_MANUFACTURER_DESCRIPTOR"

        <DllImport("IOCTL_DVD.dll", CallingConvention:=CallingConvention.Cdecl)> _
        Private Shared Function ReadStructure_Manufacturer(ByVal drive As Char, ByRef md As DVD_MANUFACTURER_DESCRIPTOR) As Integer
        End Function

        Public Function GetDVD_MANUFACTURER_DESCRIPTOR(ByVal Drive As Char) As DVD_MANUFACTURER_DESCRIPTOR
            Try
                Dim out As New DVD_MANUFACTURER_DESCRIPTOR
                ReDim out.ManufacturingInformation(2047)
                Dim i As Integer = Me.ReadStructure_Manufacturer(Drive, out)
                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetDVD_MANUFACTURER_DESCRIPTOR. Error: " & ex.Message)
            End Try
        End Function

        <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
        Public Structure DVD_MANUFACTURER_DESCRIPTOR
            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2048)> _
            Public ManufacturingInformation() As Byte
        End Structure

#End Region 'DVD_MANUFACTURER_DESCRIPTOR

#Region "UTILITY CODE"

        Public Function FormatString(ByVal InS As String) As String
            InS = Replace(InS, "______", "-")
            InS = Replace(InS, "_____", " ")
            InS = Replace(InS, "____", "_")
            InS = Replace(InS, "___", "/")
            InS = Replace(InS, "__", ".")
            InS = Replace(InS, "_", "")
            InS = Replace(InS, "plus", "+")
            Return InS
        End Function

#End Region 'UTILITY CODE

        '.net DeviceIOControl
        '<System.Runtime.InteropServices.DllImport("kernel32")> _
        'Private Shared Function DeviceIoControl(ByVal hDevice As Integer, ByVal dwIoControlCode As Integer, ByVal lpInBuffer As Object, ByVal nInBufferSize As Integer, ByVal lpOutBuffer As Object, ByVal nOutBufferSize As Integer, ByVal lpBytesReturned As Integer, ByVal lpOverlapped As Object) As Integer
        'End Function

        ''0x000007f4 - hDrive
        ''0x00335140	= IOCTL_DVD_READ_STRUCTURE
        ''0x00335000	= IOCTL_DVD_START_SESSION
        ''0x0033500c	= IOCTL_DVD_END_SESSION
        ''0x00335014	= IOCTL_DVD_GET_REGION

        'Public Enum eIOCTL_DVD_ControlCodes
        '    IOCTL_DVD_READ_STRUCTURE = &H335140
        '    IOCTL_DVD_START_SESSION = &H335000
        '    IOCTL_DVD_END_SESSION = &H33500C
        '    IOCTL_DVD_GET_REGION = &H335014
        'End Enum

        'Declare Function CreateFile Lib "kernel32" Alias "CreateFileA" (ByVal lpFileName As String, ByVal dwDesiredAccess As Integer, ByVal dwShareMode As Integer, ByVal lpSecurityAttributes As Integer, ByVal dwCreationDisposition As Integer, ByVal dwFlagsAndAttributes As Integer, ByVal hTemplateFile As Integer) As Integer

        '''http://www.mentalis.org/apilist/DeviceIoControl.shtml
        '''DeviceIoControl(hDrive, IOCTL_DVD_GET_REGION, NULL, 0, &s, sizeof(s), &BytesReturned, NULL)
        ''Public Function DeviceIoControl(ByVal Drive As String) As Integer
        ''    Try
        ''        Dim hDev As Integer

        ''        'Return Me.DeviceIoControl()

        ''    Catch ex As Exception

        ''    End Try
        ''End Function

        ''Public Function TestGetRegion() As DVD_REGION
        ''    Try
        ''        Dim BytesReturned As Integer = 0
        ''        Dim out As New DVD_REGION
        ''        Dim b(3) As Byte
        ''        Dim ip As IntPtr = Me.GetPointerFromObject(b)
        ''        Dim ip2 As IntPtr = Me.GetPointerFromObject(BytesReturned)
        ''        Dim hD As Integer = Me.CreateFile("d:", &H80000000, 1, 0, 3, 1 Or &H8000000, 0)
        ''        DeviceIoControl(&H7F4, Me.eIOCTL_DVD_ControlCodes.IOCTL_DVD_GET_REGION, 0, 0, ip.ToInt32, 4, ip2.ToInt32, 0)
        ''        'DeviceIoControl(hDrive, IOCTL_DVD_GET_REGION, NULL, 0, &s, sizeof(s), &BytesReturned, NULL)
        ''        'return BytesReturned == sizeof(s);
        ''        Return out
        ''    Catch ex As Exception
        ''        Debug.WriteLine("Problem with TestGetRegion. Error: " & ex.Message)
        ''    End Try
        ''End Function

    End Class

End Namespace


