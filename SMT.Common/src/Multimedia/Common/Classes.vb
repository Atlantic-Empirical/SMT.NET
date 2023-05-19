Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Utility
Imports SMT.Multimedia.Utility.Timecode
Imports System.Runtime.InteropServices
Imports System.Xml.Serialization
Imports Elecard.ModuleConfigInterface
Imports SMT.Multimedia.GraphConstruction

Namespace Multimedia.Classes

    <XmlInclude(GetType(cStreamInfo.cStreamInfo_Ex)), XmlInclude(GetType(cStreamInfo.cStreamInfo_Ex_Vid)), XmlInclude(GetType(cStreamInfo.cStreamInfo_Ex_Aud)), XmlInclude(GetType(cStreamInfo.cStreamInfo_Ex_Grp))> _
    Public Class cStreamInfo

#Region "CONSTRUCTOR"

        Public Sub New()
        End Sub

        Public Sub New(ByRef Pin As IPin, ByRef Graph As cSMTGraph)
            Try
                Dim HR As Integer = Pin.QueryId(PinName)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'Debug.WriteLine("PinId=" & PinName)
                Dim s() As String = Split(PinName, " (")
                If s.Length < 2 Then Exit Sub
                FormatString = s(0)
                s = Split(s(1), " ")
                PID = s(1)
                DetermineStation(PID)
                DemuxerPin = Pin
            Catch ex As Exception
                Throw New Exception("Problem with New() cStreamInfo. Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'CONSTRUCTOR

#Region "PROPERTIES"

        Public IsActive As Boolean

        Public PID As Integer
        Public Format As eStreamFormat
        Public FormatString As String
        Public Station As String
        Public PinName As String
        Public Category As eCategories
        Public StreamNumber As Byte

        Public EX As cStreamInfo_Ex

        <XmlIgnore()> _
        Public Control As Object

        <XmlIgnore()> _
        Public Renderer As cSMTFilter

        <XmlIgnore()> _
        Public Decoder As cSMTFilter

        <XmlIgnore()> _
        Public Scaler As cSMTFilter

        <XmlIgnore()> _
        Public AMTC As cSMTFilter

        <XmlIgnore()> _
        Public DemuxerPin As IPin

        Public ReadOnly Property Color() As System.Drawing.Color
            Get
                Select Case Category
                    Case eCategories.PrimaryVideo
                        'Return Drawing.Color.MidnightBlue
                        Return System.Drawing.Color.FromArgb(255, 25, 25, 112)
                    Case eCategories.SecondaryVideo
                        'Return Drawing.Color.Azure
                        Return System.Drawing.Color.FromArgb(180, 25, 25, 112)
                    Case eCategories.PrimaryAudio
                        'Return Drawing.Color.SeaGreen
                        Return System.Drawing.Color.FromArgb(255, 46, 139, 87)
                    Case eCategories.SecondaryAudio
                        'Return Drawing.Color.SpringGreen
                        Return System.Drawing.Color.FromArgb(180, 46, 139, 87)
                    Case eCategories.InteractiveGraphics
                        Return Drawing.Color.Firebrick
                    Case eCategories.PresentationGraphics
                        Return Drawing.Color.Goldenrod
                    Case eCategories.TextSubtitles
                        Return Drawing.Color.Orange
                End Select
            End Get
        End Property

        Public ReadOnly Property Station_Safe() As String
            Get
                Dim out As String = Replace(Station, " ", "_")
                out = Replace(out, "-", "")
                Return out
            End Get
        End Property

        Public ReadOnly Property GeneralCategory() As eCategories_General
            Get
                Select Case Category
                    Case eCategories.PrimaryVideo, eCategories.SecondaryVideo
                        Return eCategories_General.Video
                    Case eCategories.PrimaryAudio, eCategories.SecondaryAudio
                        Return eCategories_General.Audio
                    Case eCategories.InteractiveGraphics, eCategories.PresentationGraphics, eCategories.TextSubtitles
                        Return eCategories_General.Graphics
                End Select
            End Get
        End Property

        Public ReadOnly Property Format_FriendlyString() As String
            Get
                Select Case Format
                    Case eStreamFormat.dd
                        Return "Dolby Digital"
                    Case eStreamFormat.dd_plus
                        Return "Dolby Digital+"
                    Case eStreamFormat.dd_plus_pro
                        Return "Dolby Digital+ Pro"
                    Case eStreamFormat.dts
                        Return "DTS"
                    Case eStreamFormat.dts_hd
                        Return "DTS-HD"
                    Case eStreamFormat.h264
                        Return "h264"
                    Case eStreamFormat.ig
                        Return "Interactive Graphics"
                    Case eStreamFormat.m2v
                        Return "MPEG-2"
                    Case eStreamFormat.mpa
                        Return "MPEG Audio"
                    Case eStreamFormat.pg
                        Return "Presentation Graphics"
                    Case eStreamFormat.tst
                        Return "Text Subtitle"
                    Case eStreamFormat.vc1
                        Return "VC-1"
                    Case Else
                        Return ""
                End Select
            End Get
        End Property

        Public ReadOnly Property PID_FriendlyString() As String
            Get
                Return "0x" & Hex(PID)
            End Get
        End Property

#End Region 'PROPERTIES

#Region "METHODS"

        Private Sub DetermineStation(ByVal Value As Short)
            Try
                If Value = &H1011 Then
                    Me.Category = eCategories.PrimaryVideo
                    Station = "Primary Video"
                    Exit Sub
                End If

                If Value = &H1800 Then
                    Me.Category = eCategories.TextSubtitles
                    Station = "Text Subtitle"
                    Exit Sub
                End If

                Me.StreamNumber = (Value And 255)

                If Value >= &H1100 And Value <= &H111F Then
                    Me.Category = eCategories.PrimaryAudio
                    Station = "Primary Audio - " & StreamNumber
                End If
                If Value >= &H1200 And Value <= &H121F Then
                    Me.Category = eCategories.PresentationGraphics
                    Station = "Presentation Graphics - " & StreamNumber
                End If
                If Value >= &H1400 And Value <= &H141F Then
                    Me.Category = eCategories.InteractiveGraphics
                    Station = "Interactive Graphics - " & StreamNumber
                End If
                If Value >= &H1A00 And Value <= &H1A1F Then
                    Me.Category = eCategories.SecondaryAudio
                    Station = "Secondary Audio - " & StreamNumber
                End If
                If Value >= &H1B00 And Value <= &H1B1F Then
                    Me.Category = eCategories.SecondaryVideo
                    Station = "Secondary Video - " & StreamNumber
                End If

            Catch ex As Exception
                Throw New Exception("Problem with DetermineStation(). Error: " & ex.Message, ex)
            End Try
        End Sub

        ''' <summary>
        ''' Call this after the decoder's output pin has been connected
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub SetupEx()
            Try
                Select Case Me.GeneralCategory
                    Case eCategories_General.Video
                        EX = New cStreamInfo_Ex_Vid(Me.Decoder.Pin_Out)
                    Case eCategories_General.Audio
                        EX = New cStreamInfo_Ex_Aud(Me.Decoder.Pin_Out)
                    Case eCategories_General.Graphics
                        EX = New cStreamInfo_Ex_Grp(Me.Decoder.Pin_Out)
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with SetupEx(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'METHODS

#Region "CLASSES & ENUMS"

        Public Enum eStreamFormat
            m2v
            h264
            vc1
            dts
            dts_hd
            dd
            dd_plus
            dd_plus_pro
            mpa
            ig
            pg
            tst
        End Enum

        Public Enum eCategories
            PrimaryVideo
            PrimaryAudio
            InteractiveGraphics
            PresentationGraphics
            SecondaryVideo
            SecondaryAudio
            TextSubtitles
        End Enum

        Public Enum eCategories_General As Byte
            Video
            Audio
            Graphics
            Not_Specified = 255
        End Enum

        Public Enum ePrimarySecondary
            Primary
            Secondary
        End Enum

        Public MustInherit Class cStreamInfo_Ex

            Public Sub New()
            End Sub

        End Class

        Public Class cStreamInfo_Ex_Vid
            Inherits cStreamInfo_Ex

            Public ImageSize As System.Drawing.Size
            Public FrameRate As Double
            Public Interlaced As Boolean

            Public Sub New()
            End Sub

            Public Sub New(ByRef Pin As IPin)
                Dim fmt As cSMTGraph.cFullMediaType = cSMTGraph.GetPinConnectedMediaType(Pin)
                Me.ImageSize = New System.Drawing.Size(fmt.VIH2.rcSource.Right, fmt.VIH2.rcSource.Bottom)
                Me.FrameRate = Math.Round(10000000 / fmt.VIH2.AvgTimePerFrame, 3)
                Me.Interlaced = (fmt.VIH2.dwInterlaceFlags = AM_INTERLACE.AMINTERLACE_IsInterlaced)
            End Sub

            Public ReadOnly Property FrameRate_FriendlyString() As String
                Get
                    Return FrameRate & " fps"
                End Get
            End Property

            Public ReadOnly Property ImageResolution_FriendlyString() As String
                Get
                    Return ImageSize.Height & IIf(Interlaced, "i", "p")
                End Get
            End Property

        End Class

        Public Class cStreamInfo_Ex_Aud
            Inherits cStreamInfo_Ex

            Public Sub New()
            End Sub

            Public Sub New(ByRef Pin As IPin)
            End Sub

        End Class

        Public Class cStreamInfo_Ex_Grp
            Inherits cStreamInfo_Ex

            Public Sub New()
            End Sub

            Public Sub New(ByRef Pin As IPin)
            End Sub

        End Class

#End Region 'CLASSES & ENUMS

    End Class

    Public Class cSMTFilter
        Implements IDisposable

        Public Filter As IBaseFilter
        Public Pin_In, Pin_Out As IPin
        Public IMC As ModuleConfig
        Public NV_AudAtts As NvSharedLib.INvAttributes

        Public Function SetupOutPin(Optional ByVal PinName As String = "Out") As Boolean
            Try
                Dim HR As Integer = Filter.FindPin(PinName, Pin_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with SetupOutPin(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private disposedValue As Boolean = False        ' To detect redundant calls

        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: free other state (managed objects).
                End If
                If Filter IsNot Nothing Then Marshal.FinalReleaseComObject(Filter)
                Filter = Nothing
                If Pin_In IsNot Nothing Then Marshal.FinalReleaseComObject(Pin_In)
                Pin_In = Nothing
                If Pin_Out IsNot Nothing Then Marshal.FinalReleaseComObject(Pin_Out)
                Pin_Out = Nothing
                If NV_AudAtts IsNot Nothing Then Marshal.FinalReleaseComObject(NV_AudAtts)

            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

    Public Class cSourceProperties

        Public FrameRate As Double
        Public ATPF As UInt32
        Public Height As Short
        Public Width As Short
        Public AspectX As Short
        Public AspectY As Short
        Public CopyProtectFlags As UInt32
        Public InterlacedFlags As AM_INTERLACE
        Public ControlFlags As UInt32

        Public ReadOnly Property FrameRateEn() As eFramerate
            Get
                If FrameRate > 29 And FrameRate < 31 Then Return eFramerate.NTSC
                If FrameRate = 25 Then Return eFramerate.PAL
                If FrameRate > 23 And FrameRate < 25 Then Return eFramerate.FILM
                Return Nothing
            End Get
        End Property

    End Class

    Public Class cGuides
        Public Top, Bottom, Left, Right As Short
    End Class

End Namespace
