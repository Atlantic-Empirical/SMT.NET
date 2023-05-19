Imports System.Management

'#Region "IMPORTS"

'Imports Elecard.ModuleConfigInterface
'Imports Microsoft.Win32
''Imports SMT.HardwareControl.Serial
'Imports SMT.Win.WinAPI.Constants.WindowEvents
'Imports SMT.Multimedia.Filters.BlackMagic
'Imports SMT.Multimedia.Filters.SMT.Keystone
'Imports SMT.Multimedia.Filters.SMT.AMTC
'Imports SMT.Multimedia.Filters.SMT.L21G
'Imports SMT.Multimedia.Filters.SMT.Deinterlacer
'Imports SMT.Multimedia.Filters.MainConcept
'Imports SMT.Multimedia.DirectShow
'Imports SMT.Multimedia.Enums
'Imports SMT.Multimedia.Filters.nVidia
'Imports SMT.DotNet.Utility
'Imports SMT.Multimedia.Formats.DVD.Globalization
'Imports SMT.Multimedia.Players.DVD.Structures
'Imports SMT.Multimedia.Players.DVD.Enums
'Imports SMT.Multimedia.Players.DVD.Modules.mSharedMethods
'Imports SMT.Win.Registry
'Imports System.Drawing
'Imports System.IO
'Imports System.Runtime.InteropServices

'#End Region 'IMPORTS

Namespace Multimedia.Hardware.Blackmagic

    Public Module DeviceDetection

        Public Function BMD_Detection() As eBMDDetectionResult
            '            Try
            '                Dim DLVideo As IBaseFilter
            '                Dim iDLIO As IDecklinkIOControl
            '                Dim DeckControlSupported As Boolean = False
            '                DLVideo = CType(DsBugWO.CreateDsInstance(New Guid("CEB13CC8-3591-45A5-BA0F-20E9A1D72F76"), Clsid.IBaseFilter_GUID), IBaseFilter)
            '                iDLIO = CType(DLVideo, IDecklinkIOControl)
            'iDLIO.GetIOFeatures(
            '                Marshal.FinalReleaseComObject(DLVideo)
            '                Marshal.FinalReleaseComObject(iDLIO)

            '                Return eBMDDetectionResult.DECKLINK_AND_INTENSITY_INSTALLED
            '            Catch ex As Exception
            '                Return eBMDDetectionResult.NOT_FOUND
            '            End Try

            Try
                'DEBUGGING
                'Return eBMDDetectionResult.DECKLINK_INSTALLED
                'DEBUGGING

                Dim DecklinkExists As Boolean = False
                Dim IntensityExists As Boolean = False
                Dim search As ManagementObjectSearcher
                Dim col As ManagementObjectCollection

                search = New ManagementObjectSearcher("SELECT * From Win32_PnPEntity WHERE Caption = 'Blackmagic DeckLink'")

                col = search.Get
                If col.Count > 0 Then DecklinkExists = True

                search = New ManagementObjectSearcher("SELECT * From Win32_PnPEntity WHERE Caption = 'Blackmagic Intensity'")
                col = search.Get
                If col.Count > 0 Then IntensityExists = True

                If DecklinkExists And IntensityExists Then Return eBMDDetectionResult.DECKLINK_AND_INTENSITY_INSTALLED
                If DecklinkExists Then Return eBMDDetectionResult.DECKLINK_INSTALLED
                If IntensityExists Then Return eBMDDetectionResult.INTENSITY_INSTALLED
                Return eBMDDetectionResult.NOT_FOUND


                'Dim deviceName As string
                'Dim info As ManagementObject
                'For Each info In search.Get()
                '    ' Go through each device detected.
                '    deviceName = CType(info("Caption"), String) ' Get the name of this particular device.
                '    Debug.WriteLine(deviceName)
                '    'If InStr(deviceName, "Fujifilm", CompareMethod.Text) > 0 Then

                '    'End If
                'Next
            Catch ex As Exception
                Throw New Exception("Problem with BMD_Detection(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Enum eBMDDetectionResult
            NOT_FOUND
            INTENSITY_INSTALLED
            DECKLINK_INSTALLED
            DECKLINK_AND_INTENSITY_INSTALLED
        End Enum

    End Module

End Namespace
