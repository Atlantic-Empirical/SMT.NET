#Region "IMPORTS"

Imports Elecard.ModuleConfigInterface
Imports Microsoft.Win32
Imports SMT.HardwareControl.Serial
Imports SMT.Win.WinAPI.Constants.WindowEvents
Imports SMT.Multimedia.Filters.BlackMagic
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.Multimedia.Filters.SMT.AMTC
Imports SMT.Multimedia.Filters.SMT.L21G
Imports SMT.Multimedia.Filters.SMT.Deinterlacer
Imports SMT.Multimedia.Filters.MainConcept
Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Filters.nVidia
Imports SMT.DotNet.Utility
Imports SMT.Multimedia.Formats.DVD.Globalization
Imports SMT.Multimedia.Players.DVD.Structures
Imports SMT.Multimedia.Players.DVD.Enums
Imports SMT.Multimedia.Players.DVD.Modules.mSharedMethods
Imports SMT.Win.Registry
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices

#End Region 'IMPORTS

Namespace Multimedia.GraphConstruction

    Public Class cSMTGraph

#Region "PROPERTIES"

        Private HR As Integer

        Public GraphBuilder As IGraphBuilder
        Public MediaCtrl As IMediaControl
        Public MediaEvt As IMediaEventEx
        Public VideoWin As IVideoWindow
        Public VideoStep As IVideoFrameStep
        Public MediaSeek As IMediaSeeking
        Public PGs As DsCAUUID
        Public NotifyWindowHandle As Integer
        Public GBIMediaFilter As IMediaFilter 'needed to set clock
        Public ReferenceClock As IReferenceClock

        Public ReadOnly Property ReferenceTime() As Long
            Get
                If ReferenceClock Is Nothing Then
                    HR = GBIMediaFilter.GetSyncSource(ReferenceClock)
                    Debug.WriteLine("GetSyncSource=" & HR)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
                If ReferenceClock Is Nothing Then
                    Debug.WriteLine("RefClock is nothing")
                    Return -1
                End If
                Dim rTime As Long
                HR = ReferenceClock.GetTime(rTime)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return rTime
            End Get
        End Property

#End Region 'PROPERTIES

#Region "CONSTRUCTOR"

        Public Sub New(ByRef nNotifyWindowHandle As Integer)
            Debug.WriteLine("SMT GRAPH | New()")

            GraphBuilder = DsBugWO.CreateDsInstance(Clsid.FilterGraphManager, Clsid.IGraphBuilder_GUID)

            _Filters = New List(Of cFilter)
            _GraphManagerInterfaces = New List(Of Object)

            GBIMediaFilter = CType(GraphBuilder, IMediaFilter) 'needed to set clock
            _GraphManagerInterfaces.Add(GBIMediaFilter)

            MediaEvt = CType(GraphBuilder, IMediaEvent)
            _GraphManagerInterfaces.Add(MediaEvt)
            Debug.WriteLine("in cSMTGraph: " & nNotifyWindowHandle)
            MediaEvt.SetNotifyWindow(nNotifyWindowHandle, WM_DVD_EVENT, IntPtr.Zero)

            MediaCtrl = CType(GraphBuilder, IMediaControl)
            _GraphManagerInterfaces.Add(MediaCtrl)

            MediaSeek = CType(GraphBuilder, IMediaSeeking)
            _GraphManagerInterfaces.Add(MediaSeek)

            VideoStep = CType(GraphBuilder, IVideoFrameStep)
            _GraphManagerInterfaces.Add(VideoStep)

            NotifyWindowHandle = nNotifyWindowHandle

        End Sub

#End Region 'CONSTRUCTOR

#Region "GRAPH MANAGEMENT"

        Public Function DestroyGraph() As Boolean
            Debug.WriteLine("SMT GRAPH | DestroyGraph()")
            Try
                If Not MediaCtrl Is Nothing Then
                    Threading.Thread.Sleep(2000)
                    HR = MediaCtrl.Stop
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                If Not MediaEvt Is Nothing Then
                    MediaEvt.SetNotifyWindow(IntPtr.Zero, WM_DVD_EVENT, IntPtr.Zero)
                End If

                If Not VideoWin Is Nothing Then
                    VideoWin.put_WindowState(0)
                End If

                If Not GraphBuilder Is Nothing Then
                    If Not Me.RemoveAllFiltersFromGraph() Then Throw New Exception("Problem with RemoveAllFiltersFromGraph. Could not remove all filters.")
                    Try
                        Marshal.FinalReleaseComObject(GraphBuilder)
                        GraphBuilder = Nothing
                        CleanUpROT()
                    Catch ex As Exception
                        Debug.WriteLine("Could not remove the graph from the rot")
                    End Try
                End If

                If VideoWin IsNot Nothing Then
                    'VideoWin.put_Owner(0)
                    'VideoWin.put_Visible(0)
                    HR = Marshal.FinalReleaseComObject(VideoWin)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    VideoWin = Nothing

                    'Graph.VideoWin = CType(Graph.GraphBuilder, IVideoWindow)
                    'Graph.VideoWin.put_Owner(ViewerControl.Handle)
                    'Graph.VideoWin.put_WindowStyle(WS_CHILD Or WS_CLIPSIBLINGS Or WS_CLIPCHILDREN)
                    'Graph.VideoWin.put_MessageDrain(ViewerControl.Handle)
                    'Graph.VideoWin.SetWindowPosition(ViewerControl.ClientRectangle.X, ViewerControl.ClientRectangle.Y, ViewerControl.ClientRectangle.Right, ViewerControl.ClientRectangle.Bottom)

                End If

                'DISPOSE MANAGED OBJECTS
                'RELEASE INTERFACES

                '===========================================================
                ' INTERFACES
                '===========================================================

                If VideoStep IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(VideoStep)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    VideoStep = Nothing
                End If

                If Not MediaCtrl Is Nothing Then
                    HR = Marshal.FinalReleaseComObject(MediaCtrl)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    MediaCtrl = Nothing
                End If

                If Not MediaEvt Is Nothing Then
                    HR = Marshal.FinalReleaseComObject(MediaEvt)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    MediaEvt = Nothing
                End If

                If Not VMRAspectRatio Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRAspectRatio)
                    VMRAspectRatio = Nothing
                End If

                If Not VMRFilterConfig Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRFilterConfig)
                    VMRFilterConfig = Nothing
                End If

                If Not VMRMixerControl Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRMixerControl)
                    VMRMixerControl = Nothing
                End If

                If Not VMRMixerBitmap Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRMixerBitmap)
                    VMRMixerBitmap = Nothing
                End If

                If Not VMRMonitorConfig Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRMonitorConfig)
                    VMRMonitorConfig = Nothing
                End If

                If Not VMRDeinterlaceControl Is Nothing Then
                    Marshal.FinalReleaseComObject(VMRDeinterlaceControl)
                    VMRDeinterlaceControl = Nothing
                End If

                If Not KsPropertySet Is Nothing Then
                    Marshal.FinalReleaseComObject(KsPropertySet)
                    KsPropertySet = Nothing
                End If

                If Not DVDCtrl Is Nothing Then
                    Marshal.FinalReleaseComObject(DVDCtrl)
                    DVDCtrl = Nothing
                End If

                If Not DVDInfo Is Nothing Then
                    Marshal.FinalReleaseComObject(DVDInfo)
                    DVDInfo = Nothing
                End If

                If Not nvVideoAtts Is Nothing Then
                    Marshal.FinalReleaseComObject(nvVideoAtts)
                    nvVideoAtts = Nothing
                End If

                If Not nvAudioAtts Is Nothing Then
                    Marshal.FinalReleaseComObject(nvAudioAtts)
                    nvAudioAtts = Nothing
                End If

                '===========================================================
                ' FILTERS - Released in RemoveAllFiltersFromGraph()
                '===========================================================

                If KeystoneOMNI IsNot Nothing Then KeystoneOMNI = Nothing
                If Not VSDecoder Is Nothing Then VSDecoder = Nothing
                If Not Line21Decoder Is Nothing Then Line21Decoder = Nothing
                If Not AudioRenderer Is Nothing Then AudioRenderer = Nothing
                If Not DLVideo Is Nothing Then DLVideo = Nothing
                If Not DVDNavigator Is Nothing Then DVDNavigator = Nothing
                If Not VMR9 Is Nothing Then VMR9 = Nothing

                '===========================================================
                ' PINS
                '===========================================================

                If KO_CC IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(KO_CC)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    KO_CC = Nothing
                End If
                If KO_In IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(KO_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    KO_CC = Nothing
                End If
                If KO_Out IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(KO_Out)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    KO_CC = Nothing
                End If
                If KO_SP IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(KO_SP)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    KO_CC = Nothing
                End If

                '===========================================================
                ' PROPERTY PAGES
                '===========================================================

                If Not AudioDecoder_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(AudioDecoder_PP)
                    AudioDecoder_PP = Nothing
                End If

                If Not VideoDecoder_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(VideoDecoder_PP)
                    VideoDecoder_PP = Nothing
                End If

                If Not Line21Decoder_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(Line21Decoder_PP)
                    Line21Decoder_PP = Nothing
                End If

                If Not AudioRenderer_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(AudioRenderer_PP)
                    AudioRenderer_PP = Nothing
                End If

                If Not DVDNavigator_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(DVDNavigator_PP)
                    DVDNavigator_PP = Nothing
                End If

                If Not VideoDecoder_PP Is Nothing Then
                    Marshal.FinalReleaseComObject(VideoDecoder_PP)
                    VideoDecoder_PP = Nothing
                End If

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with DestroyGraph(). Error: " & ex.Message)
            End Try
        End Function

        Public Sub DestroyFilter(ByRef Filter As IBaseFilter)
            Try
                Dim iu As IntPtr = Marshal.GetIUnknownForObject(Filter)
                While Marshal.Release(iu) > 0
                    Debug.WriteLine("Releasing Filter")
                End While
                Marshal.FinalReleaseComObject(Filter)
                Filter = Nothing
            Catch ex As Exception
                Throw New Exception("Problem with DestroyFilter(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Function RemoveAllFiltersFromGraph() As Boolean
            Debug.WriteLine("SMT GRAPH | RemoveAllFiltersFromGraph()")
            Try
                Dim Out As Boolean = True
                HR = MediaCtrl.Stop
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim En As IEnumFilters
                HR = GraphBuilder.EnumFilters(En)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim cnt As Integer
                Dim Filters() As Integer
                Dim NoFilters As Short = FilterCount()
                ReDim Filters(NoFilters - 1)
                HR = En.Next(NoFilters, Filters, cnt)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim ptr As IntPtr
                Dim tFilter As IBaseFilter
                Dim TempGuid As Guid
                Dim tObj As Object
                For Each Pointer As Integer In Filters
                    ptr = New IntPtr(Pointer)
                    tObj = Marshal.GetObjectForIUnknown(ptr)
                    tFilter = CType(tObj, IBaseFilter)
                    tFilter.GetClassID(TempGuid)
                    Debug.WriteLine("SMT GRAPH | Removing filter: " & GetFilterName(tFilter))
                    'Debug.WriteLine("SMT GRAPH | Removing filter: " & TempGuid.ToString)
                    Try
                        HR = GraphBuilder.RemoveFilter(tFilter)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        HR = Marshal.FinalReleaseComObject(tFilter)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Catch ex As Exception
                        Debug.WriteLine("SMT GRAPH | Removal of filter failed: " & TempGuid.ToString)
                        Out = False
                    End Try
                Next
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with RemoveAllFiltersFromGraph(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function FilterCount() As Short
            Try
                Dim en As IEnumFilters
                Dim HR As Integer = GraphBuilder.EnumFilters(en)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim C As Short = 0
                Dim cnt As Integer
                Dim Filters() As Integer
                ReDim Filters(0)
                HR = en.Next(1, Filters, cnt)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                While HR = 0 And cnt > 0
                    C += 1
                    HR = en.Next(1, Filters, cnt)
                End While
                Return C
            Catch ex As Exception
                Throw New Exception("Problem counting filters in graph. error: " & ex.Message)
            End Try
        End Function

        Private Function GetFilterState(ByVal Filter As IBaseFilter) As String
            Try
                Dim imf As IMediaFilter = CType(Filter, IMediaFilter)
                Dim fstate As Integer
                HR = imf.GetState(1000, fstate)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Select Case fstate
                    Case 0
                        Debug.WriteLine("Filter state: Stopped")
                        Return "Stopped"
                    Case 1
                        Debug.WriteLine("Filter state: Paused")
                        Return "Paused"
                    Case 2
                        Debug.WriteLine("Filter state: Running")
                        Return "Running"
                End Select
            Catch ex As Exception
                Throw New Exception("GetFilterState failed. Error: " & ex.Message)
            End Try
        End Function

#Region "GRAPH MANAGEMENT:ENUMERATORS"

#Region "GRAPH MANAGEMENT:FILTERS"

        Public Sub EnumerateFilters(ByVal FG As IGraphBuilder)
            Try
                Dim FEnum As IEnumFilters

                HR = FG.EnumFilters(FEnum)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim Filters(3) As Integer
                Dim Cnt As Integer

                HR = FEnum.Next(4, Filters, Cnt)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim ptr As IntPtr
                Dim tFilter As IBaseFilter
                Dim obj As Object
                For k As Short = 0 To Cnt - 1
                    ptr = New IntPtr(Filters(k))
                    'Debug.WriteLine(ptr.ToString)
                    obj = Marshal.GetObjectForIUnknown(ptr)
                    tFilter = CType(obj, IBaseFilter)
                    Debug.WriteLine(GetFilterName(tFilter))
                Next
            Catch ex As Exception
                Throw New Exception("Problem with EnumerateFilters(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Function GetFilterName(ByRef Filter As IBaseFilter) As String
            Try
                Dim i As FilterInfo
                Filter.QueryFilterInfo(i)
                Return i.achName
            Catch ex As Exception
                Throw New Exception("Problem with GetFilterName(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function FindFilterByName(ByVal FilterName As String) As IBaseFilter
            Try
                Dim devs As System.Collections.ArrayList
                DsDev.GetDevicesOfCat(New Guid("E0F158E1-CB04-11D0-BD4E-00A0C911CE86"), devs)

                For i As Short = 0 To devs.Count - 1
                    If devs(i).Name = FilterName Then 'such as "DirectSound: Realtek AC97 Audio"
                        Dim out As IBaseFilter
                        devs(i).Mon.BindToObject(Nothing, Nothing, Clsid.IBaseFilter_GUID, out)
                        Return out
                    End If
                Next
                Throw New Exception(FilterName & " not found.")
            Catch ex As Exception
                Throw New Exception("Problem with FindFilterByName(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'GRAPH MANAGEMENT:FILTERS

#Region "GRAPH MANAGEMENT:PINS"

        Public Shared Function EnumeratePinNames(ByVal Filter As IBaseFilter) As String()
            Try
                Dim HR As Integer
                Dim PinEnum As IEnumPins
                HR = Filter.EnumPins(PinEnum)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim cnt As Integer
                Dim Pins() As Integer
                ReDim Pins(0)
                HR = PinEnum.Next(1, Pins, cnt)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim ptr As IntPtr
                Dim tPin As IPin
                Dim out(-1) As String
                While HR = 0 And cnt > 0
                    ptr = New IntPtr(CInt(Pins(0)))
                    'Debug.WriteLine(ptr.ToString)
                    Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
                    tPin = CType(obj, IPin)
                    Dim PinID As String
                    HR = tPin.QueryId(PinID)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Debug.WriteLine(PinID)
                    ReDim Preserve out(UBound(out) + 1)
                    out(UBound(out)) = PinID
                    HR = PinEnum.Next(1, Pins, cnt)
                End While
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with EnumeratePinNames(). Error: " & ex.Message)
            End Try
        End Function

        Public Function GetPins(ByVal Filter As IBaseFilter) As List(Of IPin)
            Try
                Dim HR As Integer
                Dim PinEnum As IEnumPins
                HR = Filter.EnumPins(PinEnum)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim cnt As Integer
                Dim Pins() As Integer
                ReDim Pins(0)
                HR = PinEnum.Next(1, Pins, cnt)
                HR = PinEnum.Next(1, Pins, cnt) 'skip the input pin (hopefully is always the first returned!!!
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim ptr As IntPtr
                Dim tPin As IPin
                Dim out As New List(Of IPin)
                While HR = 0 And cnt > 0
                    ptr = New IntPtr(CInt(Pins(0)))
                    'Debug.WriteLine(ptr.ToString)
                    Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
                    tPin = CType(obj, IPin)
                    out.Add(tPin)
                    HR = PinEnum.Next(1, Pins, cnt)
                End While
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with EnumeratePins(). Error: " & ex.Message)
            End Try
        End Function

        Public Function FindPinByPartialName(ByVal Filter As IBaseFilter, ByVal PinNamePartial As String) As IPin
            Try
                Dim HR As Integer
                Dim PinEnum As IEnumPins
                HR = Filter.EnumPins(PinEnum)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim cnt As Integer
                Dim Pins() As Integer
                ReDim Pins(0)
                HR = PinEnum.Next(1, Pins, cnt)
                If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim ptr As IntPtr
                Dim tPin As IPin
                Dim out(-1) As String
                While HR = 0 And cnt > 0
                    ptr = New IntPtr(CInt(Pins(0)))
                    'Debug.WriteLine(ptr.ToString)
                    Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
                    tPin = CType(obj, IPin)
                    Dim PinID As String
                    HR = tPin.QueryId(PinID)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If InStr(PinID.ToLower, PinNamePartial.ToLower) Then
                        Return tPin
                    End If
                    'Debug.WriteLine(PinID)
                    ReDim Preserve out(UBound(out) + 1)
                    out(UBound(out)) = PinID
                    HR = PinEnum.Next(1, Pins, cnt)
                End While
                Return Nothing
            Catch ex As Exception
                Throw New Exception("Problem with FindPinByPartialName(). Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function EnumeratePreferedMediaTypes(ByVal P As IPin) As List(Of AM_MEDIA_TYPE)
            Try
                Dim MTEnum As IEnumMediaTypes
                Dim HR As Integer
                HR = P.EnumMediaTypes(MTEnum)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim MTsPtrs(0) As Integer
                Dim cnt As Integer
                HR = MTEnum.Next(1, MTsPtrs, cnt)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim ptr As IntPtr
                Dim tMT As AM_MEDIA_TYPE
                Dim Out As New List(Of AM_MEDIA_TYPE)
                While HR = 0 And cnt > 0
                    ptr = New IntPtr(CInt(MTsPtrs(0)))
                    Dim obj As Object = Marshal.PtrToStructure(ptr, GetType(AM_MEDIA_TYPE))
                    'Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
                    tMT = CType(obj, AM_MEDIA_TYPE)
                    Out.Add(tMT)
                    HR = MTEnum.Next(1, MTsPtrs, cnt)
                End While
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with EnumeratePreferedMediaTypes(). Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetPinPreferredMediaTypes(ByVal Pin As IPin) As List(Of cFullMediaType)
            Try
                Dim Out As New List(Of cFullMediaType)
                Dim Ts As List(Of AM_MEDIA_TYPE) = EnumeratePreferedMediaTypes(Pin)

                Dim tFMT As cFullMediaType

                For Each T As AM_MEDIA_TYPE In Ts
                    tFMT = New cFullMediaType
                    tFMT.AMMT = T

                    Select Case T.formattype
                        Case New Guid("05589F80-C356-11CE-BF01-00AA0055595A") 'FORMAT_VideoInfo
                            tFMT.VIH = GetVIH(tFMT.AMMT)
                        Case New Guid("F72A76A0-EB0A-11D0-ACE4-0000C0CC16BA") 'FORMAT_VideoInfo2
                            tFMT.VIH2 = GetVIH2(tFMT.AMMT)
                        Case New Guid("05589F81-C356-11CE-BF01-00AA0055595A") 'FORMAT_WaveFormatEx
                            Try
                                Dim b(Convert.ToInt32(tFMT.AMMT.cbFormat) - 1) As Byte
                                For i As Integer = 0 To UBound(b)
                                    b(i) = Marshal.ReadByte(tFMT.AMMT.pbFormat, i)
                                Next
                                'Out.WFX = GetWaveFormatEx(b)
                                tFMT.WFXTN = GetWaveFormatExten(b)
                                'Out.WFX = CType(Marshal.PtrToStructure(Out.AMMT.pbFormat, GetType(DVDMedia.WAVEFORMATEX)), DVDMedia.WAVEFORMATEX)
                            Catch ex As Exception
                                'we're not going to worry about this right now
                            End Try
                    End Select
                    Out.Add(tFMT)
                Next

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetPinMediaType(). Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetPinConnectedMediaType(ByVal Pin As IPin) As cFullMediaType
            Try
                Dim Out As New cFullMediaType
                Dim hr As Integer = Pin.ConnectionMediaType(Out.AMMT)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)

                Select Case Out.AMMT.formattype
                    Case New Guid("05589F80-C356-11CE-BF01-00AA0055595A") 'FORMAT_VideoInfo
                        Out.VIH = GetVIH(Out.AMMT)
                    Case New Guid("F72A76A0-EB0A-11D0-ACE4-0000C0CC16BA") 'FORMAT_VideoInfo2
                        Out.VIH2 = GetVIH2(Out.AMMT)
                    Case New Guid("05589F81-C356-11CE-BF01-00AA0055595A") 'FORMAT_WaveFormatEx
                        Try
                            Dim b(Convert.ToInt32(Out.AMMT.cbFormat) - 1) As Byte
                            For i As Integer = 0 To UBound(b)
                                b(i) = Marshal.ReadByte(Out.AMMT.pbFormat, i)
                            Next
                            'Out.WFX = GetWaveFormatEx(b)
                            Out.WFXTN = GetWaveFormatExten(b)
                            'Out.WFX = CType(Marshal.PtrToStructure(Out.AMMT.pbFormat, GetType(DVDMedia.WAVEFORMATEX)), DVDMedia.WAVEFORMATEX)
                        Catch ex As Exception
                            'we're not going to worry about this right now
                        End Try
                End Select

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetPinMediaType(). Error: " & ex.Message)
            End Try
        End Function

        Public Class cFullMediaType
            Public AMMT As AM_MEDIA_TYPE
            Public VIH2 As VIDEOINFOHEADER2
            'Public WFX As DVDMedia.WAVEFORMATEX
            Public WFXTN As WAVEFORMATEXTENSIBLE
            Public VIH As VIDEOINFOHEADER

            Public Sub New()
                AMMT = New AM_MEDIA_TYPE
                VIH2 = New VIDEOINFOHEADER2
                VIH = New VIDEOINFOHEADER
                'WFX = New DVDMedia.WAVEFORMATEX
            End Sub

        End Class

        Public Shared Function GetVIH2(ByVal AMMT As AM_MEDIA_TYPE) As VIDEOINFOHEADER2
            Try
                Dim b(Convert.ToInt32(AMMT.cbFormat) - 1) As Byte
                For i As Integer = 0 To UBound(b)
                    b(i) = Marshal.ReadByte(AMMT.pbFormat, i)
                Next

                Dim VIH2 As New VIDEOINFOHEADER2
                Dim R As DsRECT
                R.Bottom = CInt("&h" & GetByte(b(13)) & GetByte(b(12)))
                R.Right = CInt("&h" & GetByte(b(9)) & GetByte(b(8)))
                R.Top = 0
                R.Left = 0
                VIH2.rcSource = R
                VIH2.rcTarget = R
                VIH2.dwBitRate = CInt("&h" & GetByte(b(35)) & GetByte(b(34)) & GetByte(b(33)) & GetByte(b(32)))
                VIH2.dwBitErrorRate = CInt("&h" & GetByte(b(39)) & GetByte(b(38)) & GetByte(b(37)) & GetByte(b(36)))
                VIH2.AvgTimePerFrame = CInt("&h" & GetByte(b(47)) & GetByte(b(46)) & GetByte(b(45)) & GetByte(b(44)) & GetByte(b(43)) & GetByte(b(42)) & GetByte(b(41)) & GetByte(b(40)))
                VIH2.dwInterlaceFlags = CInt("&h" & GetByte(b(51)) & GetByte(b(50)) & GetByte(b(49)) & GetByte(b(48)))
                VIH2.dwCopyProtectFlags = CInt("&h" & GetByte(b(55)) & GetByte(b(54)) & GetByte(b(53)) & GetByte(b(52)))
                VIH2.dwPictAspectRatioX = CInt("&h" & GetByte(b(59)) & GetByte(b(58)) & GetByte(b(57)) & GetByte(b(56)))
                VIH2.dwPictAspectRatioY = CInt("&h" & GetByte(b(63)) & GetByte(b(62)) & GetByte(b(61)) & GetByte(b(60)))
                VIH2.dwControlFlags = CInt("&h" & GetByte(b(67)) & GetByte(b(66)) & GetByte(b(65)) & GetByte(b(64)))
                VIH2.dwReserved2 = CInt("&h" & GetByte(b(71)) & GetByte(b(70)) & GetByte(b(69)) & GetByte(b(68)))
                VIH2.bmiHeader = GetBMI(b, False, True)
                Return VIH2
            Catch ex As Exception
                Throw New Exception("Problem getting VIH2. Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetVIH(ByVal AMMT As AM_MEDIA_TYPE) As VIDEOINFOHEADER
            Try
                Dim VIH As New VIDEOINFOHEADER
                Dim b(Convert.ToInt32(AMMT.cbFormat) - 1) As Byte
                For i As Integer = 0 To UBound(b)
                    b(i) = Marshal.ReadByte(AMMT.pbFormat, i)
                Next

                VIH.AvgTimePerFrame = CInt("&h" & GetByte(b(23)) & GetByte(b(22)) & GetByte(b(21)) & GetByte(b(20)))
                VIH.BitRate = CInt("&h" & GetByte(b(35)) & GetByte(b(34)) & GetByte(b(33)) & GetByte(b(32)))
                VIH.BitErrorRate = CInt("&h" & GetByte(b(39)) & GetByte(b(38)) & GetByte(b(37)) & GetByte(b(36)))
                VIH.AvgTimePerFrame = CInt("&h" & GetByte(b(47)) & GetByte(b(46)) & GetByte(b(45)) & GetByte(b(44)) & GetByte(b(43)) & GetByte(b(42)) & GetByte(b(41)) & GetByte(b(40)))

                Dim R As New DsRECT
                R.Bottom = CInt("&h" & GetByte(b(13)) & GetByte(b(12)))
                R.Right = CInt("&h" & GetByte(b(9)) & GetByte(b(8)))
                R.Top = 0
                R.Left = 0
                VIH.SrcRect = R
                VIH.TagRect = R

                VIH.BmiHeader = GetBMI(b, True, False)

                Return VIH
            Catch ex As Exception
                Throw New Exception("Problem getting VIH. Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetBMI(ByVal b() As Byte, ByVal ForVIH As Boolean, ByVal ForVIH2 As Boolean) As BITMAPINFOHEADER
            Try
                Dim OS As Short
                If ForVIH Then
                    OS = 48
                End If
                If ForVIH2 Then
                    OS = 72
                End If

                ''debugging
                'OS = 0
                ''debugging

                Dim BMI As BITMAPINFOHEADER
                BMI.biSize = CInt("&h" & GetByte(b(OS + 3)) & GetByte(b(OS + 2)) & GetByte(b(OS + 1)) & GetByte(b(OS)))
                BMI.biWidth = CInt("&h" & GetByte(b(OS + 7)) & GetByte(b(OS + 6)) & GetByte(b(OS + 5)) & GetByte(b(OS + 4)))
                BMI.biHeight = CInt("&h" & GetByte(b(OS + 11)) & GetByte(b(OS + 10)) & GetByte(b(OS + 9)) & GetByte(b(OS + 8)))
                BMI.biPlanes = CInt("&h" & GetByte(b(OS + 13)) & GetByte(b(OS + 12)))
                BMI.biBitCount = CInt("&h" & GetByte(b(OS + 15)) & GetByte(b(OS + 14)))
                BMI.biCompression = CInt("&h" & GetByte(b(OS + 19)) & GetByte(b(OS + 18)) & GetByte(b(OS + 17)) & GetByte(b(OS + 16)))
                BMI.biSizeImage = CInt("&h" & GetByte(b(OS + 23)) & GetByte(b(OS + 22)) & GetByte(b(OS + 21)) & GetByte(b(OS + 20)))
                BMI.biXPelsPerMeter = CInt("&h" & GetByte(b(OS + 27)) & GetByte(b(OS + 26)) & GetByte(b(OS + 25)) & GetByte(b(OS + 24)))
                BMI.biYPelsPerMeter = CInt("&h" & GetByte(b(OS + 31)) & GetByte(b(OS + 30)) & GetByte(b(OS + 29)) & GetByte(b(OS + 28)))
                BMI.biClrUsed = CInt("&h" & GetByte(b(OS + 35)) & GetByte(b(OS + 34)) & GetByte(b(OS + 33)) & GetByte(b(OS + 32)))
                BMI.biClrImportant = CInt("&h" & GetByte(b(OS + 39)) & GetByte(b(OS + 38)) & GetByte(b(OS + 37)) & GetByte(b(OS + 36)))
                Dim CT(UBound(b) - (OS + 40)) As Byte
                Dim cnt As Integer = 0
                For i As Short = OS + 40 To UBound(b)
                    CT(cnt) = b(i)
                    cnt += 1
                Next

                Dim ColorColl((CT.Length / 4) - 1) As Color
                cnt = 0
                For i As Short = 0 To UBound(CT) Step 4
                    ColorColl(cnt) = Color.FromArgb(CInt("&h" & GetByte(CT(i))), CInt("&h" & GetByte(CT(i + 1))), CInt("&h" & GetByte(CT(i + 2))))
                    cnt += 1
                Next

                Return BMI
            Catch ex As Exception
                Throw New Exception("Problem with GetBMI. Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetWaveFormatExten(ByVal B() As Byte) As WAVEFORMATEXTENSIBLE
            Try
                Dim Out As New WAVEFORMATEXTENSIBLE
                Out.Format = GetWaveFormatEx(B)
                If B.Length = 18 Then Return Out
                Out.SubFormat = New Guid(GetByte(B(27)) & GetByte(B(26)) & GetByte(B(25)) & GetByte(B(24)) & "-" & GetByte(B(29)) & GetByte(B(28)) & "-" & GetByte(B(31)) & GetByte(B(30)) & "-" & GetByte(B(33)) & GetByte(B(32)) & "-" & GetByte(B(34)) & GetByte(B(35)) & GetByte(B(36)) & GetByte(B(37)) & GetByte(B(38)) & GetByte(B(39)))
                Out.dwChannelMask = CInt("&h" & GetByte(B(23)) & GetByte(B(22)) & GetByte(B(21)) & GetByte(B(20))) 'This may be key.  Figure out which bytes are the channel mask.
                Out.wValidBitsPerSample = CInt("&h" & GetByte(B(19)) & GetByte(B(18)))
                Out.wReserved = 0
                Out.wSamplesPerBlock = 0
                Return Out
            Catch ex As Exception
                Throw New Exception("problem getting waveformat ex. error: " & ex.Message)
            End Try
        End Function

        Public Shared Function GetWaveFormatEx(ByVal B() As Byte) As WAVEFORMATEX
            Try
                Dim Out As New WAVEFORMATEX
                Out.wFormatTag = CInt("&h" & GetByte(B(1)) & GetByte(B(0)))
                Out.nChannels = CInt("&h" & GetByte(B(3)) & GetByte(B(2)))
                Out.nSamplesPerSec = CInt("&h" & GetByte(B(7)) & GetByte(B(6)) & GetByte(B(5)) & GetByte(B(4)))
                Out.nAvgBytesPerSec = CInt("&h" & GetByte(B(11)) & GetByte(B(10)) & GetByte(B(9)) & GetByte(B(8)))
                Out.nBlockAlign = CInt("&h" & GetByte(B(13)) & GetByte(B(12)))
                Out.wBitsPerSample = CInt("&h" & GetByte(B(15)) & GetByte(B(14)))
                Out.cbSize = CInt("&h" & GetByte(B(17)) & GetByte(B(16)))
                Return Out
            Catch ex As Exception
                Throw New Exception("problem getting waveformat ex. error: " & ex.Message)
            End Try
        End Function

#End Region 'GRAPH MANAGEMENT:PINS

        Private Shared Function GetByte(ByVal InByte As Byte) As String
            Return Hex(InByte).PadLeft(2, "0")
        End Function

#End Region 'GRAPH MANAGEMENT:ENUMERATORS

#Region "ROT Management"

        Public ROTEntries() As String

        Public Sub AddGraphToROT()
            Try
                Dim ROTID As String = Right(DateTime.Now.Ticks.ToString, 7)
                DsROT.AddGraphToRot(Me.GraphBuilder, ROTID)
                If ROTEntries Is Nothing Then ReDim ROTEntries(-1)
                ReDim Preserve ROTEntries(UBound(ROTEntries) + 1)
                ROTEntries(UBound(ROTEntries)) = ROTID
            Catch ex As Exception
                Throw New Exception("Problem with AddGraphtoROT(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub CleanUpROT()
            Try
                If ROTEntries Is Nothing Then Exit Sub
                For Each ROTID As String In ROTEntries
                    DsROT.RemoveGraphFromRot(ROTID)
                Next
                ReDim ROTEntries(-1)
            Catch ex As Exception
                Throw New Exception("Problem with CleanUpROT(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'ROT Management

#Region "GRAPH MANAGEMENT:STATE"



#End Region 'GRAPH MANAGEMENT:STATE

#End Region 'GRAPH MANAGEMENT

#Region "GRAPH STATE"

        Public ReadOnly Property MediaControlState() As eMediaControlState
            Get
                Try
                    Dim s As Integer
                    HR = MediaCtrl.GetState(4000, s)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Select Case s
                        Case 0
                            Return eMediaControlState.Stopped
                        Case 1
                            Return eMediaControlState.Paused
                        Case 2
                            Return eMediaControlState.Running
                    End Select
                Catch ex As Exception
                    Throw New Exception("Problem with MediaControlState. Error: " & ex.Message, ex)
                End Try
            End Get
        End Property

        Public Sub SetSyncSource_NULL()
            Try
                If GraphBuilder Is Nothing Then Exit Sub
                Dim imf As IMediaFilter = CType(GraphBuilder, IMediaFilter)
                HR = imf.SetSyncSource(Nothing)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with SetSyncSource_NULL(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub SetSyncSource_Default()
            Try
                If GraphBuilder Is Nothing Then Exit Sub
                HR = GraphBuilder.SetDefaultSyncSource()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with SetSyncSource_Default(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public ReadOnly Property GraphManagerInterfaces() As List(Of Object)
            Get
                Return _GraphManagerInterfaces
            End Get
        End Property
        Private _GraphManagerInterfaces As List(Of Object)

        Public ReadOnly Property Filters() As List(Of cFilter)
            Get
                Return _Filters
            End Get
        End Property
        Private _Filters As List(Of cFilter)

        Public Class cFilter
            Public BaseFilter As IBaseFilter
            Public Interfaces As List(Of Object)
            Public Pins As List(Of IPin)
            Public PropertyPage As ISpecifyPropertyPages
            Public Sub New(ByRef bf As IBaseFilter)
                BaseFilter = bf
                Interfaces = New List(Of Object)
                Pins = New List(Of IPin)
            End Sub
        End Class

#End Region 'GRAPH STATE

#Region "FILTERS"

#Region "FILTERS:SMT"

#Region "FILTERS:SMT:KEYSTONEOMNI"

        Public KeystoneOMNI As IBaseFilter
        Public KO_In, KO_Out, KO_CC, KO_SP As IPin
        Public KO_IKeystone As IKeystone_OMNI
        Public KO_IKeystoneMixer As IKeystoneMixer_OMNI
        Public KO_IKeystoneProcAmp As IKeystoneProcAmp_OMNI
        Public KO_IKeystoneQuality As IKeystoneQuality_OMNI
        Public KO_PP As ISpecifyPropertyPages

        Public Function AddKeystoneOmni(Optional ByVal Renderer As eAVMode = eAVMode.DesktopVMR) As Boolean
            Try
                KeystoneOMNI = CType(DsBugWO.CreateDsInstance(New Guid("fd501027-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(KeystoneOMNI, "SMT Keystone OMNI")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                KO_PP = CType(KeystoneOMNI, ISpecifyPropertyPages)
                KO_IKeystoneProcAmp = CType(KeystoneOMNI, IKeystoneProcAmp_OMNI)
                KO_IKeystone = CType(KeystoneOMNI, IKeystone_OMNI)
                KO_IKeystoneMixer = CType(KeystoneOMNI, IKeystoneMixer_OMNI)
                KO_IKeystoneQuality = CType(KeystoneOMNI, IKeystoneQuality_OMNI)
                KO_IKeystoneProcAmp = CType(KeystoneOMNI, IKeystoneProcAmp_OMNI)
                KO_IKeystone.UnlockFilter(New Guid("fd501045-8ebe-11ce-8183-00aa00577da1"))

                HR = KeystoneOMNI.FindPin("Video", KO_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = KeystoneOMNI.FindPin("Output", KO_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = KeystoneOMNI.FindPin("Line21", KO_CC)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = KeystoneOMNI.FindPin("Subpicture", KO_SP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''CHECK FOR DONGLEFREEKEYSTONE REGISTRY ENTRY
                'If CheckForDONGLEFREEKEYSTONE() Then
                '    'tell keystone to stand down
                '    KO_IKeystone.SetProperty(New Guid("19655F52-2A4D-40d9-BA93-A6B90C2EDD72"), 32778)
                'End If

                KO_IKeystone.SetRenderer(Renderer)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddKeystoneOmni(). Error: " & ex.Message)
            End Try
        End Function

        Public Sub KeystoneOmni_Unpause()
            Try
                If KeystoneOMNI Is Nothing Then Exit Sub
                If KO_IKeystone Is Nothing Then Exit Sub
                HR = KO_IKeystone.Pause(0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with KeystoneOmni_Unpause(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Function GetFramerate() As Double
            Try
                Dim TargFR As Single = 0
                HR = KO_IKeystoneQuality.get_TargetFR_In(TargFR)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim sTargFR As String = CStr(TargFR)

                'If sTargFR <> VSI.lblCurrentFrameRate.Text Then
                '    Throw New Exception(eConsoleItemType.NOTICE, "Video source frame rate changed", sTargFR)
                '    VSI.lblCurrentFrameRate.Text = sTargFR
                'End If

                'VSI.lblCurrentFrameRate.Text = TargFR
            Catch ex As Exception
                Throw New Exception("Problem with GetFramerate. Error: " & ex.Message)
            End Try
        End Function

        Private Function CheckForDONGLEFREEKEYSTONE() As Boolean
            Try
                Dim o As Object = GetHKCRKey("CLSID\{0E6A9EE4-440A-4cbc-8E38-06FB98615A60}", "lm34")
                If Not o Is Nothing Then Return True
                Return False
            Catch ex As Exception
                Return False
            End Try
        End Function

#End Region 'FILTERS:SMT:KEYSTONEOMNI

#Region "FILTERS:SMT:AMTC"

        Public AMTC As IBaseFilter
        Public AMTC_Interface As IAMTC
        Public AMTC_In, AMTC_Out As IPin

        Public Function AddAMTC() As Boolean
            Try
                AMTC = CType(DsBugWO.CreateDsInstance(New Guid("A6512CF0-A47B-45ba-A054-0DB0D4BB87F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(AMTC, "SMT AMTC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                AMTC_Interface = CType(AMTC, IAMTC)
                HR = AMTC.FindPin("In", AMTC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = AMTC.FindPin("Out", AMTC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim F As New cFilter(AMTC)
                F.Interfaces.Add(AMTC_Interface)
                F.Pins.Add(AMTC_In)
                F.Pins.Add(AMTC_Out)
                Filters.Add(F)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddAMTC(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:SMT:AMTC

#Region "FILTERS:SMT:SONY M2V"

        Public SonyM2V As IBaseFilter
        'Public iSonyM2V As ISonyM2V
        Public SonyM2V_In, SonyM2V_Out As IPin

        Public Function AddSonyM2V() As Boolean
            Try
                SonyM2V = CType(DsBugWO.CreateDsInstance(New Guid("A6512CF9-A47B-45ba-A054-0DB0D4BB87F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(SonyM2V, "Seq. SonyM2V")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'iSonyM2V = CType(SonyM2V, ISonyM2V)
                HR = SonyM2V.FindPin("In", SonyM2V_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = SonyM2V.FindPin("Out", SonyM2V_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddSequoyanSonyM2V. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:SMT:SONYM2V

#Region "FILTERS:SMT:L21G"

        Public L21G As IBaseFilter
        Public iL21G As IL21G
        Public L21G_In, L21G_Out As IPin

        Public Function AddL21G() As Boolean
            Try
                L21G = CType(DsBugWO.CreateDsInstance(New Guid("A2957546-A38D-44b9-834E-096AF622EC3D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(L21G, "Seq. L21G")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                iL21G = CType(L21G, IL21G)
                L21G.FindPin("In", L21G_In)
                L21G.FindPin("Out", L21G_Out)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem adding L21G. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:SMT:L21G

        '#Region "FILTERS:SMT:KEYSTONEHD"

        '        Public KeyHD As IBaseFilter
        '        Public KO_In, KO_Out, KeyHD_CC, KeyHD_SP As IPin
        '        Public KeyHD_PP As ISpecifyPropertyPages
        '        'Public Key_CurrentOutHeight As Short
        '        'Public Key_OutputSizeSet As Boolean = False

        '        Public Function AddKeystoneOMNI_Simple() As Boolean
        '            Try
        '                KeyHD = CType(DsBugWO.CreateDsInstance(New Guid("A6512CFF-A47B-45BA-A054-0DB0D4BB87F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
        '                HR = GraphBuilder.AddFilter(KeyHD, "SMT KeyHD (Simple)")
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                KeyHD.FindPin("In", KO_In)
        '                Return True
        '            Catch ex As Exception
        '                Throw New Exception("Problem with AddKeystoneOMNI_Simple. Error: " & ex.Message)
        '                Return False
        '            End Try
        '        End Function

        '        Public KO_IKeystone As IKeystone_HD
        '        Public KO_IKeystoneMixer As IKeystoneMixer_HD
        '        Public iKeyQualityHD As IKeystoneQuality_HD
        '        Public iKeyProcAmpHD As IKeystoneProcAmp_HD

        '        Public Function AddKeystoneOMNI() As Boolean
        '            Try
        '                KeyHD = CType(DsBugWO.CreateDsInstance(New Guid("FD5010FF-8EBE-11CE-8183-00AA00577DA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
        '                HR = GraphBuilder.AddFilter(KeyHD, "SMT KeyHD")
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                KeyHD_PP = CType(KeyHD, ISpecifyPropertyPages)
        '                KO_IKeystone = CType(KeyHD, IKeystone_HD)
        '                KO_IKeystoneMixer = CType(KeyHD, IKeystoneMixer_HD)
        '                iKeyQualityHD = CType(KeyHD, IKeystoneQuality_HD)
        '                iKeyProcAmpHD = CType(KeyHD, IKeystoneProcAmp_HD)
        '                KO_IKeystone.UnlockFilter(New Guid("fd501045-8ebe-11ce-8183-00aa00577da1"))
        '                HR = KeyHD.FindPin("Video", KO_In)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                HR = KeyHD.FindPin("Output", KO_Out)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                'HR = KeyHD.FindPin("Line21", Key_CC)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                'HR = KeyHD.FindPin("Subpicture", Key_SP)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'CHECK FOR DONGLEFREEKEYSTONE REGISTRY ENTRY
        '                If CheckForDONGLEFREEKEYSTONE() Then
        '                    'tell keystone to stand down
        '                    KO_IKeystone.SetTrialOverride(True)
        '                End If

        '                Return True
        '            Catch ex As Exception
        '                Throw New Exception("Problem with AddKeystoneOMNI. Error: " & ex.Message)
        '                Return False
        '            End Try
        '        End Function

        '#Region "Graph.KeyHD Support Methods"

        '        Public Function GetFramerate() As Double
        '            Try
        '                Dim TargFR As Single = 0
        '                HR = iKeyQualityHD.get_TargetFR_In(TargFR)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                Dim sTargFR As String = CStr(TargFR)

        '                'If sTargFR <> VSI.lblCurrentFrameRate.Text Then
        '                '    Throw New Exception(eConsoleItemType.NOTICE, "Video source frame rate changed", sTargFR)
        '                '    VSI.lblCurrentFrameRate.Text = sTargFR
        '                'End If

        '                'VSI.lblCurrentFrameRate.Text = TargFR
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetFramerate. Error: " & ex.Message)
        '            End Try
        '        End Function

        '#End Region 'Graph.KeySD Support Methods

        '#End Region 'FILTERS:SMT:KEYSTONEHD

        '#Region "FILTERS:SMT:KEYSTONESD"

        '        Public KeySD As IBaseFilter
        '        Public KO_In, KO_Out, KeySD_CC, KeySD_SP As IPin
        '        Public KO_IKeystone As IKeystone_SD
        '        Public KO_IKeystoneMixer As IKeystoneMixer_SD
        '        Public KO_IKeystoneQuality As IKeystoneQuality_SD
        '        Public KO_IKeystoneProcAmp As IKeystoneProcAmp_SD
        '        Public KeySD_PP As ISpecifyPropertyPages
        '        Public KeySD_CurrentOutHeight As Short
        '        Public KO_OutputSizeSet As Boolean = False

        '        Public Function AddKeystoneOMNI() As Boolean
        '            Try
        '                KeySD = CType(DsBugWO.CreateDsInstance(New Guid("fd501043-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
        '                HR = GraphBuilder.AddFilter(KeySD, "SMT KeySD")
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                KeySD_PP = CType(KeySD, ISpecifyPropertyPages)
        '                KO_IKeystone = CType(KeySD, IKeystone_SD)
        '                KO_IKeystoneMixer = CType(KeySD, IKeystoneMixer_SD)
        '                KO_IKeystoneQuality = CType(KeySD, IKeystoneQuality_SD)
        '                KO_IKeystoneProcAmp = CType(KeySD, IKeystoneProcAmp_SD)
        '                KO_IKeystone.UnlockFilter(New Guid("fd501045-8ebe-11ce-8183-00aa00577da1"))
        '                HR = KeySD.FindPin("Video", KO_In)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                HR = KeySD.FindPin("Output", KO_Out)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                HR = KeySD.FindPin("Line21", KeySD_CC)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                HR = KeySD.FindPin("Subpicture", KeySD_SP)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'CHECK FOR DONGLEFREEKEYSTONE REGISTRY ENTRY
        '                If CheckForDONGLEFREEKEYSTONE() Then
        '                    'tell keystone to stand down
        '                    KO_IKeystone.SetProperty(New Guid("19655F52-2A4D-40d9-BA93-A6B90C2EDD72"), 32778)
        '                End If

        '                Return True
        '            Catch ex As Exception
        '                Throw New Exception("Problem with AddKeystoneOMNI(). Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Private Function CheckForDONGLEFREEKEYSTONE() As Boolean
        '            Try
        '                Dim o As Object = GetHKCRKey("CLSID\{0E6A9EE4-440A-4cbc-8E38-06FB98615A60}", "lm34")
        '                If Not o Is Nothing Then Return True
        '                Return False
        '            Catch ex As Exception
        '                Return False
        '            End Try
        '        End Function

        '#End Region 'FILTERS:SMT:KEYSTONESD

#Region "FILTERS:SMT:KEYSTONEFS"

        Public KeyHD_FS As IBaseFilter
        Public KeyHD_FS_EncodeIn, KeyHD_FS_SourceIn, KeyHD_FS_Out As IPin

        Public IKeyFS As IKeystone_FS
        Public IKeyMixerFS As IKeystoneMixer_FS
        Public IKeyQualityFS As IKeystoneQuality_FS
        Public IKeyProcAmpFS As IKeystoneProcAmp_FS
        Public KeyHD_FS_PP As ISpecifyPropertyPages

        Public Function AddKeystoneOMNI_FrameSplitter() As Boolean
            Try
                KeyHD_FS = CType(DsBugWO.CreateDsInstance(New Guid("fd5010fe-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(KeyHD_FS, "SMT KeyHD Frame Splitter")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                KeyHD_FS_PP = CType(KeyHD_FS, ISpecifyPropertyPages)
                IKeyFS = CType(KeyHD_FS, IKeystone_FS)
                IKeyMixerFS = CType(KeyHD_FS, IKeystoneMixer_FS)
                IKeyQualityFS = CType(KeyHD_FS, IKeystoneQuality_FS)
                IKeyProcAmpFS = CType(KeyHD_FS, IKeystoneProcAmp_FS)
                IKeyFS.UnlockFilter(New Guid("fd501045-8ebe-11ce-8183-00aa00577da1"))
                HR = KeyHD_FS.FindPin("VideoTwo", KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = KeyHD_FS.FindPin("VideoOne", KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = KeyHD_FS.FindPin("Output", KeyHD_FS_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddKeystoneOMNI. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:SMT:KEYSTONEFS

#Region "FILTERS:SMT:NVS SD"

        Public NVS As IBaseFilter
        Public NVS_Out As IPin

        Public Function AddNullVideoSource() As Boolean
            Try
                NVS = CType(DsBugWO.CreateDsInstance(New Guid("FD501075-8EBE-11CE-8183-00AA00577DA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                GraphBuilder.AddFilter(NVS, "NVS")
                NVS.FindPin("1", NVS_Out)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddNullVideoSource. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:SMT:NVS SD

#Region "FILTERS:SMT:NVS HD"

        Public NVS_HD As IBaseFilter
        Public NVS_HD_Out As IPin

        Public Function AddNullVideoSource_HD() As Boolean
            Try
                NVS_HD = CType(DsBugWO.CreateDsInstance(New Guid("FD501076-8EBE-11CE-8183-00AA00577DA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                GraphBuilder.AddFilter(NVS_HD, "NVS")
                NVS_HD.FindPin("1", NVS_HD_Out)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddNullVideoSource_HD. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:SMT:NVS HD

#Region "FILTERS:SMT:DAC"

        Public DTSAC3Source As IBaseFilter
        Public IDTSAC3 As IFileSourceFilter
        Public DTSAC3_Out As IPin

        Public Function AddDTSAC3Source(ByVal FilePath As String) As Boolean
            Try
                DTSAC3Source = CType(DsBugWO.CreateDsInstance(New Guid("B4A7BE85-551D-4594-BDC7-832B09185041"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(DTSAC3Source, "FSF")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                IDTSAC3 = CType(DTSAC3Source, IFileSourceFilter)
                HR = IDTSAC3.Load(FilePath, 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DTSAC3Source.FindPin("1", DTSAC3_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDTSAC3Source(). Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:SMT:DAC

#Region "FILTERS:SMT:YUV"

        Public YUVSource As IBaseFilter
        Public iYUVSource As IFileSourceFilterSMT
        Public YUV_Out As IPin
        'Public YUVSeek As IMediaSeeking

        Public Function AddYUVSource() As Boolean
            Try
                YUVSource = CType(DsBugWO.CreateDsInstance(New Guid("9A80E199-3BBA-4821-B18B-21BB496F80F8"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(YUVSource, "YUV")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = YUVSource.FindPin("1", YUV_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'YUVSeek = CType(YUV_Out, IMediaSeeking)
                MediaSeek = CType(YUV_Out, IMediaSeeking)
                iYUVSource = CType(YUV_Out, IFileSourceFilterSMT)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddYUVSource. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:SMT:YUV

#Region "FILTERS:SMT:DINT"

        Public Dint As IBaseFilter
        Public IDint As IDeinterlacer
        Public Dint_In, Dint_out As IPin
        Public Dint_pp As ISpecifyPropertyPages

        Public Function AddDeinterlacer() As Boolean
            Try
                Dint = CType(DsBugWO.CreateDsInstance(New Guid("0205D900-9256-4c67-81C5-0103BDAFF721"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(Dint, "SMT Deinterlacer")
                Dint_pp = CType(Dint, ISpecifyPropertyPages)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                IDint = CType(Dint, IDeinterlacer)
                HR = Dint.FindPin("In", Dint_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Dint.FindPin("Out", Dint_out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDeinterlacer. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:SMT:DINT

#End Region 'FILTERS:SMT

#Region "FILTERS:NVIDIA"

#Region "FILTERS:NVIDIA:AUDIO"

        Public AudioDecoder As IBaseFilter
        Public nvAudioAtts As NvSharedLib.INvAttributes
        Public AudioDecoder_PP As ISpecifyPropertyPages
        Public AudDec_InPin As IPin
        Public AudDec_OutPin As IPin

        Public Function AddNVidiaAudioDecoder(ByVal SetupForMobile As Boolean) As Boolean
            Try
                AudioDecoder = CType(DsBugWO.CreateDsInstance(New Guid("6C0BDF86-C36A-4D83-8BDB-312D2EAF409E"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(AudioDecoder, "NVIDIA Audio Decoder")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If AudioDecoder_PP Is Nothing Then AudioDecoder_PP = CType(AudioDecoder, ISpecifyPropertyPages)
                nvAudioAtts = CType(AudioDecoder, NvSharedLib.INvAttributes)
                InitializeAudioDecoder(SetupForMobile)
                HR = AudioDecoder.FindPin("Audio Input", AudDec_InPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = AudioDecoder.FindPin("Audio Output", AudDec_OutPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim F As New cFilter(AudioDecoder)
                F.Interfaces.Add(nvAudioAtts)
                F.PropertyPage = AudioDecoder_PP
                F.Pins.Add(AudDec_OutPin)
                F.Pins.Add(AudDec_InPin)
                Filters.Add(F)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddNVAudioDecoder. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Sub InitializeAudioDecoder(ByVal SetupForMobile As Boolean)
            Try
                If nvAudioAtts Is Nothing Then Exit Sub

                'make sure the decoder is enabled and not muted
                nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_SKIP_AUDIO_DECODE, 0)
                nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_MUTE, 0)

                If SetupForMobile Then
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_CONNECTED_DEVICE_PROP_CONTROL, nvcommon.ENvidiaAudioDecoderProps_OutputTo.NVAUDDEC_SPEAKERS)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_ENABLE_SPDIF_PASSTHRU, 0)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_AC3_OUTPUT_MODE, 2)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_SPDIF_PROP_CONTROL, 0)
                Else
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_CONNECTED_DEVICE_PROP_CONTROL, nvcommon.ENvidiaAudioDecoderProps_OutputTo.NVAUDDEC_RECEIVER)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_ENABLE_SPDIF_PASSTHRU, 1)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_AC3_OUTPUT_MODE, 2)
                    nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_SPDIF_PROP_CONTROL, 1)
                End If
            Catch ex As Exception
                Throw New Exception("Problem initializing audio decoder. Error: " & ex.Message)
            End Try
        End Sub

        Public Sub ViewAudioDecoderPropertyPage()
            Try
                AudioDecoder_PP.GetPages(PGs)
                Dim FI As New FilterInfo
                DsUtils.OleCreatePropertyFrame(NotifyWindowHandle, 0, 0, FI.achName, 1, AudioDecoder, PGs.cElems, PGs.pElems, 0, 0, Nothing)
            Catch ex As Exception
                Throw New Exception("Problem with ViewAudioDecoderPropertyPage(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub nVidia_AudioMute(ByVal Mute As Boolean)
            Try
                If nvAudioAtts Is Nothing Then Exit Sub
                nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_SKIP_AUDIO_DECODE, IIf(Mute, 1, 0))
            Catch ex As Exception
                Throw New Exception("Problem with nVidia_AudioMute(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public ReadOnly Property nVidiaAudio_IsSetToMobile() As Boolean
            Get
                Dim i As Long = -1
                nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_ENABLE_SPDIF_PASSTHRU, i)
                Return i = 0
            End Get
        End Property

#End Region 'FILTERS:NVIDIA:AUDIO

#Region "FILTERS:NVIDIA:VIDEO"

        Public VSDecoder As IBaseFilter
        Public nvVideoAtts As NvSharedLib.INvAttributes
        Public VideoDecoder_PP As ISpecifyPropertyPages
        Public VidDec_Vid_In As IPin
        Public VidDec_Vid_Out As IPin
        Public VidDec_SP_In As IPin
        Public VidDec_SP_Out As IPin
        Public VidDec_CC_In As IPin
        Public VidDec_CC_Out As IPin
        Public KsPropertySet As IKsPropertySet

        Public Function AddNVidiaVideoDecoder(ByVal ForceHDConnection As Boolean, ByVal SetupForMobile As Boolean) As Boolean
            Try
                VSDecoder = CType(DsBugWO.CreateDsInstance(New Guid("71E4616A-DB5E-452B-8CA5-71D9CC7805E9"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(VSDecoder, "NVIDIA Video Decoder")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                nvVideoAtts = CType(VSDecoder, NvSharedLib.INvAttributes)
                InitializeVideoDecoder(ForceHDConnection, SetupForMobile)
                VideoDecoder_PP = CType(VSDecoder, ISpecifyPropertyPages)
                HR = VSDecoder.FindPin("~Line21 Output", VidDec_CC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = VSDecoder.FindPin("~Subpicture Output", VidDec_SP_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = VSDecoder.FindPin("Video Input", VidDec_Vid_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                KsPropertySet = CType(VidDec_Vid_In, IKsPropertySet)
                HR = VSDecoder.FindPin("Subpicture Input", VidDec_SP_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = VSDecoder.FindPin("Video Output", VidDec_Vid_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim F As New cFilter(VSDecoder)
                F.Interfaces.Add(nvVideoAtts)
                F.Interfaces.Add(KsPropertySet)
                F.PropertyPage = VideoDecoder_PP
                F.Pins.Add(VidDec_CC_Out)
                F.Pins.Add(VidDec_SP_Out)
                F.Pins.Add(VidDec_Vid_In)
                F.Pins.Add(VidDec_SP_In)
                F.Pins.Add(VidDec_Vid_Out)
                Filters.Add(F)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddNVVideoDecoder. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Sub InitializeVideoDecoder(ByVal ForceHDConnection As Boolean, ByVal SetupForMobile As Boolean)
            Try
                If nvVideoAtts Is Nothing Then Exit Sub
                If ForceHDConnection Then
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_FORCE_1088_CONNECTION, 1)
                Else
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_FORCE_1088_CONNECTION, 0)
                End If

                nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DISABLE_TRAY_ICON, 1)
                nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_SET_GOPTC_AS_MEDIA_TIME, 1)
                nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DISABLE_DROP_FRAMES, 1)

                'only argb4444 sp output
                nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_SUBPICTURE_FORMATS, nvcommon.NVVIDDEC_CONFIG_BITS_SP_ARGB4444)

                If SetupForMobile Then
                    'deinterlace mode: best available
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_AUTO)
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_NORMAL)
                    'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_ENABLE_DXVA, 1)
                Else
                    'film/weave
                    'works well for progressive content
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_FILM)
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_FILTERED_WEAVE)
                    'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_ENABLE_DXVA, 0)
                End If

                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONTROL, nvcommon.ENvidiaVideoDecoderProps_ControlTypes.NVVIDDEC_CONTROL_SET_GOPTC_AS_MEDIA_TIME, 1)
                'SetLong(NVVIDDEC_CONTROL, NVVIDDEC_CONTROL_SET_GOPTC_AS_MEDIA_TIME, 1);

                'nvVideoAtts.SetLong(3, 9, 100)

                ''auto
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_AUTO)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_NORMAL)

                ''video - normal
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_VIDEO)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, 0)

                'film
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_FILM)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_FILTERED_WEAVE)

                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_SMART)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_FILTERED_WEAVE)

                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_FILM)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_AUTO)

                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_FORCE_PAL_CONNECTION, 0)

                'Turn off hardware acceleration
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_ENABLE_DXVA, 0)

                ''Set interlacing to Vertical Stretch
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_VMR_DEINTERLACE_TECHNOLOGY, 2)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_SPECIFIC_VMR)
                'nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONTROL, nvcommon.ENvidiaVideoDecoderProps_ControlTypes.NVVIDDEC_CONTROL_SEND_USER_EVENT, 100)

                'load video settings
                'xVid.LoadVidSettingsFromReg()

            Catch ex As Exception
                Throw New Exception("Problem initializing video decoder. Error: " & ex.Message)
            End Try
        End Sub

        Public CurrentBitrate As Integer = 0
        Public Sub GetBitrate()
            Try
                If nvVideoAtts Is Nothing Then Exit Sub
                '  Dim ptr As New Integer
                ' Dim h As GCHandle = GCHandle.Alloc(ptr, GCHandleType.Pinned)
                'Dim iptr As IntPtr = h.AddrOfPinnedObject
                nvVideoAtts.GetLong(0, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_BITRATE, CurrentBitrate)
                'CurrentBitrate = CInt(GetObjectFromPointer(New IntPtr(CurrentBitrate)))

                'VSI.UpdateBitrate(CurrentBitrate)

                'Dim tBR As Short = Math.Round((CurrentBitrate * 100) / (10 * 1024 * 1024) * 100, 0)
                'If tBR > 10000 Or tBR < 0 Then Exit Sub
                'If tBR > 0 Then
                '    VSI.lblCurrentBitrate.Text = tBR
                '    If CurrentUserProfile.AppOptions.AnimateBitrate Then VSI.pbBitrate.Value = tBR
                'Else
                '    VSI.lblCurrentBitrate.Text = 0
                '    If CurrentUserProfile.AppOptions.AnimateBitrate Then VSI.pbBitrate.Value = 0
                'End If
                ''Throw New Exception(CurrentBitrate)
            Catch ex As Exception
                If InStr(ex.Message.ToLower, "overflow") Then Exit Sub
                Throw New Exception("problem getting current bitrate. error: " & ex.Message)
            End Try
        End Sub

        Public CurrentDroppedFrames As Integer = 0
        Public Sub GetDroppedFrames()
            Try
                If nvVideoAtts Is Nothing Then Exit Sub
                nvVideoAtts.GetLong(0, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_FRAME_DROPS, CurrentDroppedFrames)
                If CurrentDroppedFrames = 1 Then CurrentDroppedFrames = 0
                'VSI.lblDroppedFrames.Text = CurrentDroppedFrames
            Catch ex As Exception
                Throw New Exception("problem getting dropped frames. error: " & ex.Message)
            End Try
        End Sub

        Public ReadOnly Property VideoEncrypted() As Boolean
            Get
                Dim i As Integer
                nvVideoAtts.GetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_STATS, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_ENCRYPTED, i)
                If i > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public Sub SetNvidiaDeinterlacing(ByVal Control As nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL, ByVal Mode As nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE)
            nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_CONTROL, Control)
            nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_DEINTERLACE_MODE, Mode)
        End Sub

#Region "Video Format Selection"

        Public Function SetVideoStandardViaRegistry(ByVal SetToNTSC As Boolean) As Boolean
            Try
                Dim rKeyA, rKeyV As RegistryKey
                rKeyV = Registry.LocalMachine.OpenSubKey("Software\NVIDIA Corporation\Filters\Video", True)
                If SetToNTSC Then
                    rKeyV.SetValue("ForcePALConnection", 0)
                Else
                    rKeyV.SetValue("ForcePALConnection", 1)
                End If
                rKeyV.Close()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetVideoStandardViaRegistry. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function SetVideoStandardViaInterface(ByVal SetToNTSC As Boolean) As Boolean
            Try
                If nvVideoAtts Is Nothing Then
                    Throw New Exception("nvVideoAtts is Nothing.")
                End If
                If SetToNTSC Then
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_FORCE_PAL_CONNECTION, 0)
                Else
                    nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_FORCE_PAL_CONNECTION, 1)
                End If
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetVideoStandardViaInterface. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'Video Standard Selection

#End Region 'FILTERS:NVIDIA:VIDEO

#End Region 'FILTERS:NVIDIA

#Region "FILTERS:JAVELIN"

#Region "FILTERS:JAVELIN:JACKET PICTURE CONVERTER"

        Public JPC As IBaseFilter
        Public JPC_iKeystone As IKeystone_JPC

        Public Function AddJacketPictureConverter() As Boolean
            Try
                JPC = CType(DsBugWO.CreateDsInstance(New Guid("fd501075-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(JPC, "JPC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                JPC_iKeystone = CType(JPC, IKeystone_JPC)
                JPC_iKeystone.UnlockFilter(New Guid("fd501045-8ebe-11ce-8183-00aa00577da1"))
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddJacketPictureConverter(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:JAVELIN:JACKET PICTURE CONVERTER

#End Region 'FILTERS:JAVELIN

#Region "FILTERS:BLACKMAGIC"

#Region "FILTERS:BLACKMAGIC:DLA"

        Public AudioRenderer As IBaseFilter
        Public AudioRenderer_PP As ISpecifyPropertyPages
        Public AudRen_In As IPin

        Public Function AddDeckLinkAudio() As Boolean
            Try
                AudioRenderer = CType(DsBugWO.CreateDsInstance(New Guid("19FA8CC3-56CE-46AB-825D-5CE1A39B137A"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(AudioRenderer, "DeckLink Audio Renderer")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                AudioRenderer.FindPin("In", AudRen_In)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDeckLinkAudio(). Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:BLACKMAGIC:DLA

#Region "FILTERS:BLACKMAGIC:DLV"

        Public DLVideo As IBaseFilter
        Public VideoRenderer_PP As ISpecifyPropertyPages
        Public DLV_In As IPin
        Public IDLVTR As IDecklinkRawDeviceControl
        Public iDLIO As IDecklinkIOControl
        Public DeckControl As cDeckControl
        Public DeckControlSupported As Boolean = False
        Public DLV_Clock As IReferenceClock

        Public Function AddDeckLinkVideo() As Boolean
            Try
                DLVideo = CType(DsBugWO.CreateDsInstance(New Guid("CEB13CC8-3591-45A5-BA0F-20E9A1D72F76"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(DLVideo, "DeckLink Video Renderer")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'DLV_Clock = CType(DLVideo, IReferenceClock) 'needed to set clock
                IDLVTR = CType(DLVideo, IDecklinkRawDeviceControl)
                iDLIO = CType(DLVideo, IDecklinkIOControl)
                DeckControl = New cDeckControl(IDLVTR)
                HR = DLVideo.FindPin("In", DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                VideoRenderer_PP = CType(DLVideo, ISpecifyPropertyPages)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDeckLinkVideo. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function SetDecklinkAnalogVideoOutputType(ByVal Component As Boolean) As Boolean
            Try
                If iDLIO Is Nothing Then Return False

                If Not DecklinkSupportsAnalogVideo Then
                    Throw New Exception("The DeckLink card in this system does not support analog video.")
                End If

                Try
                    HR = iDLIO.SetAnalogueOutput(Component, True)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return True
                Catch ex As Exception
                    Throw New Exception("The Decklink card in this system does not support analog output.")
                End Try

                'Dim i As ULong = 0
                'HR = iDLIO.GetIOFeatures(i)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'If i And BlackMagic.eIOFeatures.DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOOUTPUT Then
                '    HR = iDLIO.SetAnalogueOutput(Composite, True)
                '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                '    Return True
                'Else
                '    Throw New Exception("The Decklink card in this system does not support composite output.")
                '    Return False
                'End If
            Catch ex As Exception
                Throw New Exception("Problem with SetDecklinkAnalogVideoOutputType(). Error: " & ex.Message)
            End Try
        End Function

        Public ReadOnly Property DecklinkSupportsAnalogVideo() As Boolean
            Get
                If iDLIO Is Nothing Then Return True
                Dim i As Integer = 0
                HR = iDLIO.GetIOFeatures(i)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If (i And eIOFeatures.DECKLINK_IOFEATURES_HASCOMPONENTVIDEOOUTPUT) Or (i And eIOFeatures.DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOINPUT) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property DecklinkSupportsHD() As Boolean
            Get
                If iDLIO Is Nothing Then Return True
                Dim i As Integer = 0
                HR = iDLIO.GetIOFeatures(i)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If (i And eIOFeatures.DECKLINK_IOFEATURES_SUPPORTSHD) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public Shared ReadOnly Property DecklinkSupportsHD_Shared() As Boolean
            Get
                Try
                    Dim HR As Integer
                    Dim DLVideo As IBaseFilter
                    Dim iDLIO As IDecklinkIOControl
                    DLVideo = CType(DsBugWO.CreateDsInstance(New Guid("CEB13CC8-3591-45A5-BA0F-20E9A1D72F76"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    iDLIO = CType(DLVideo, IDecklinkIOControl)
                    If iDLIO Is Nothing Then Throw New Exception("iDLIO is nothing")
                    Dim i As ULong = 0
                    HR = iDLIO.GetIOFeatures(i)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Marshal.FinalReleaseComObject(DLVideo)
                    DLVideo = Nothing

                    'Dim bin As String = DecToBin(i)
                    'bin = StrReverse(bin)
                    'i = BinToDec(bin)

                    If (i And eIOFeatures2.DECKLINK_IOFEATURES_SUPPORTSHD) Then
                        Return True
                    Else
                        Return False
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with DecklinkSupportsHD_Shared(). Error: " & ex.Message, ex)
                End Try
            End Get
        End Property

        Public Function SetDecklinkHDThreeTwoPulldown(ByVal Active As Boolean) As Boolean
            Try
                If iDLIO Is Nothing Then Return False
                HR = iDLIO.SetHDTVPulldownOnOutput(Active)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem setting HD 32 output. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:BLACKMAGIC:DLV

#End Region 'FILTERS:BLACKMAGIC

#Region "FILTERS:MAINCONCEPT"

#Region "FILTERS:MAINCONCEPT:DMXA"

        Public MCE_DMXA As IBaseFilter
        Public MCE_DMXA_In, MCE_DMXA_Out, MCE_DMXA_Out_Aud As IPin
        Public MCE_IMC_DMXA As ModuleConfig
        Public MCE_DMXA_IMS As IMediaSeeking

        Public Function AddMCE_DMXA() As Boolean
            Try
                MCE_DMXA = CType(DsBugWO.CreateDsInstance(New Guid("136DCBF5-3874-4B70-AE3E-15997D6334F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_DMXA, "MCE-DMX")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = MCE_DMXA.FindPin("Input", MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_DMXA)
                MCE_IMC_DMXA = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCEDemuxA. Error: " & ex.Message)
            End Try
        End Function

        Public Shared Function MCE_DMX_GetStreamDurationInfo(ByRef MCE_IMC As ModuleConfig) As Object
            Try
                If MCE_IMC Is Nothing Then Return Nothing
                Dim o As Object = MCE_IMC.GetParamValue(ModuleConfig_Consts.EMPGPDMX_STREAMS_DURATION)

                Return o
            Catch ex As Exception
                Throw New Exception("Problem with MCE_DMX_GetStreamDurationInfo(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:DMXA

#Region "FILTERS:MAINCONCEPT:DMXB"

        Public MCE_DMXB As IBaseFilter
        Public MCE_DMXB_In, MCE_DMXB_Out, MCE_DMXB_Out_Aud As IPin

        Public Function AddMCE_DMXB() As Boolean
            Try
                MCE_DMXB = CType(DsBugWO.CreateDsInstance(New Guid("136DCBF5-3874-4B70-AE3E-15997D6334F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_DMXB, "MCE-DMX")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_DMXB.FindPin("Input", MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCEDemuxB. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:DMXB

#Region "FILTERS:MAINCONCEPT:H264"

        Public MCE_AVC As IBaseFilter
        Public MCE_AVC_In, MCE_AVC_Out As IPin
        Public MCE_AVC_PP As ISpecifyPropertyPages
        Public MCE_IMC As ModuleConfig

        Public Function AddMCE_AVC() As Boolean
            Try

                '4A69B442-28BE-4991-969C-B500ADF5D8A8 = DMO
                'E59A98C5-60E7-4702-983C-8C9A1168086E = AX
                '6A270473-9994-4AEB-801F-BB2C4E56EE38 = AX - non-demo
                '09377888-BB3A-4FE7-8953-4815B02F758F = post 8/31/06
                '96B9D0ED-8D13-4171-A983-B84D88D627BE = post 070420

                Dim o As Object = DsBugWO.CreateDsInstance(New Guid("96B9D0ED-8D13-4171-A983-B84D88D627BE"), Clsid.IBaseFilter_GUID)
                'Dim o As Object = DsBugWO.CreateDsInstance(New Guid("6A270473-9994-4AEB-801F-BB2C4E56EE38"), ClsId.IBaseFilter_GUID)
                MCE_AVC = CType(o, IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_AVC, "MCE_AVC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_AVC)
                MCE_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = MCE_IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing) 'UNLOCK
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.AVC_ErrorResilienceMode, ModuleConfig_Consts.eErrorResilienceMode_AVC.ErrorResilienceMode_Decode_Anyway)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.AVC_Deblock, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HQUpsample_VC1AVC, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Synchronizing, ModuleConfig_Consts.eEM2VD_Synchronizing.PTS)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.AVC_RateMode, ModuleConfig_Consts.eRateMode.RateMode_Field)
                MCE_IMC.CommitChanges()

                HR = MCE_AVC.FindPin("In", MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                MCE_AVC_PP = CType(MCE_AVC, ISpecifyPropertyPages)

                Return True
            Catch ex As Exception
                Debug.WriteLine("Problem with AddMCE_AVC. Error: " & ex.Message & " " & ex.StackTrace)
                Throw New Exception("Problem with AddMCE_AVC. Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:H264

#Region "FILTERS:MAINCONCEPT:MPEG2A"

        Public MCE_MP2A As IBaseFilter
        Public MCE_MP2A_In, MCE_MP2A_Out As IPin
        Public MCE_MP2A_PP As ISpecifyPropertyPages

        Public Function AddMCE_MP2A() As Boolean
            Try
                MCE_MP2A = CType(DsBugWO.CreateDsInstance(New Guid("BC4EB321-771F-4E9F-AF67-37C631ECA106"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_MP2A, "MCE_MP2A")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_MP2A.FindPin("In", MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_MP2A.FindPin("Out", MCE_MP2A_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                MCE_MP2A_PP = CType(MCE_MP2A, ISpecifyPropertyPages)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_MP2A)
                MCE_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = MCE_IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.EMC_ErrorConcealment, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_HQUpsample, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_IDCTPrecision, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_Brightness, 128)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_Resolution, ModuleConfig_Consts.eResolutionMode.Resolution_Full)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_PostProcess, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_DeinterlaceCondition, ModuleConfig_Consts.eDeinterlaceCondition.DeinterlaceCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_CCubeDecodeOrder, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_MediaTimeSource, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_SetMediaTime, 1)
                MCE_IMC.CommitChanges()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCE_MP2. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:MPEG2A

#Region "FILTERS:MAINCONCEPT:MPEG2B"

        Public MCE_MP2B As IBaseFilter
        Public MCE_MP2B_In, MCE_MP2B_Out As IPin
        Public MCE_MP2B_PP As ISpecifyPropertyPages

        Public Function AddMCE_MP2B() As Boolean
            Try
                MCE_MP2B = CType(DsBugWO.CreateDsInstance(New Guid("BC4EB321-771F-4E9F-AF67-37C631ECA106"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_MP2B, "MCE_AVC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_MP2B.FindPin("In", MCE_MP2B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_MP2B.FindPin("Out", MCE_MP2B_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                MCE_MP2B_PP = CType(MCE_MP2B, ISpecifyPropertyPages)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_MP2B)
                MCE_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean = MCE_IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.EMC_ErrorConcealment, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_HQUpsample, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_IDCTPrecision, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_Brightness, 128)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_Resolution, ModuleConfig_Consts.eResolutionMode.Resolution_Full)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_PostProcess, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_DeinterlaceCondition, ModuleConfig_Consts.eDeinterlaceCondition.DeinterlaceCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_CCubeDecodeOrder, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.M2VD_MediaTimeSource, 0)
                MCE_IMC.CommitChanges()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCE_MP2. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:MPEG2B

#Region "FILTERS:MAINCONCEPT:VC1_ES_Parser"

        Public MCE_VC12VC1 As IBaseFilter
        Public MCE_VC12VC1_In, MCE_VC12VC1_Out As IPin

        Public Function AddMCE_VC12VC1() As Boolean
            Try
                MCE_VC12VC1 = CType(DsBugWO.CreateDsInstance(New Guid("7BB4AFF4-E262-4796-BF64-EDECEA5E0A2A"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_VC12VC1, "VC-1 To VC-1")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_VC12VC1.FindPin("In", MCE_VC12VC1_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_VC12VC1.FindPin("Out", MCE_VC12VC1_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddYUVSource. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:VC1_ES_Parser

#Region "FILTERS:MAINCONCEPT:VC1"

        Public MCE_VC1A As IBaseFilter
        Public MCE_VC1A_In, MCE_VC1A_Out As IPin
        Public MCE_VC1A_PP As ISpecifyPropertyPages

        Public Function AddMCE_VC1A() As Boolean
            Try
                MCE_VC1A = CType(DsBugWO.CreateDsInstance(New Guid("C0046C92-D654-4B34-92D6-C6AC34B7346D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_VC1A, "MCE_VC1")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_VC1A.FindPin("In", MCE_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_VC1A.FindPin("Out", MCE_VC1A_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                MCE_VC1A_PP = CType(MCE_VC1A, ISpecifyPropertyPages)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_VC1A)
                MCE_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = MCE_IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing) 'UNLOCK
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.VC1_SkipMode, ModuleConfig_Consts.eMCVC1VD_SkipMode.none)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.VC1_ErrorResilienceMode, ModuleConfig_Consts.eErrorResilienceMode_VC1.Error_resilience_mode_proceed_anyway)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.HQUpsample_VC1AVC, 1)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.Synchronizing, ModuleConfig_Consts.eEM2VD_Synchronizing.PTS)
                b = MCE_IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                MCE_IMC.CommitChanges()

                'MCE_IMC = CType(MCE_VC1A, IModuleConfig)
                'HR = MCE_IMC.SetValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), 0)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCE_VC1A(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:VC1

#Region "FILTERS:MAINCONCEPT:SCALER"

        Public MCE_ImgSiz As IBaseFilter
        Public MCE_ImgSiz_In, MCE_ImgSiz_Out As IPin
        Public MCE_ImgSiz_PP As ISpecifyPropertyPages
        Public MCE_ImgSiz_IMC As ModuleConfig

        Public Function AddMCE_ImgSiz() As Boolean
            Try
                Dim o As Object = DsBugWO.CreateDsInstance(New Guid("BEB7FFE8-37BA-4849-AE26-7A10EF20A303"), Clsid.IBaseFilter_GUID)
                MCE_ImgSiz = CType(o, IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_ImgSiz, "MCE_ImageSizer")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_ImgSiz)
                MCE_ImgSiz_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)

                HR = MCE_ImgSiz.FindPin("In", MCE_ImgSiz_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = MCE_ImgSiz.FindPin("Out", MCE_ImgSiz_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                MCE_ImgSiz_PP = CType(MCE_ImgSiz, ISpecifyPropertyPages)

                Return True
            Catch ex As Exception
                Throw New Exception(ex.ToString)
                Throw New Exception("Problem with AddMCE_ImgSiz. Error: " & ex.Message)
            End Try
        End Function

        Public Function MCE_SetScaling(ByVal output_size As eVideoResolution, ByVal scaling_mode As eScalingMode, ByVal aspect_x As Byte, ByVal aspect_y As Byte) As Boolean
            Try
                If MCE_ImgSiz Is Nothing Then Exit Function
                If MCE_ImgSiz_IMC Is Nothing Then Exit Function

                'SIZING & MATTING LOGIC
                Dim scl As Boolean = False
                Dim mtt As Boolean = False
                Dim sw, sh As Integer 'source width and height
                Dim dw, dh As Integer 'destination width and height
                Dim vb, hb As Integer 'vertical and horizontal bar sizes
                Dim tw, th As Integer 'target width and height (output video signal dimensions)
                Dim ax, ay As Byte 'source aspect ratio
                Dim ratio As Double 'aspect ratio (ie 1.77)

                ax = aspect_x
                ay = aspect_y

                'GET SOURCE WxH
                sw = MCE_ImgSiz_IMC.GetParamValue(mMainConcept_Scaler.MC_SCALER_SourceWidth)
                sh = MCE_ImgSiz_IMC.GetParamValue(mMainConcept_Scaler.MC_SCALER_SourceHeight)

                'SET DEFAULT DESTINATION SIZE AS SOURCE SIZE
                dw = sw
                dh = sh

                'HEED USER'S REQUESTS
                Select Case output_size
                    Case eVideoResolution._720x486, eVideoResolution._720x480
                        tw = 720
                        th = 486
                    Case eVideoResolution._720x576
                        tw = 720
                        th = 576
                    Case eVideoResolution._1280x720
                        tw = 1280
                        th = 720
                    Case eVideoResolution._1920x1080
                        tw = 1920
                        th = 1080
                End Select

                If sh > 576 And scaling_mode <> eScalingMode.Native Then Throw New Exception("Scaling of HD video is not supported.")

                Select Case scaling_mode

                    Case eScalingMode.Native

                        'CALCULATE BAR SIZES
                        vb = (tw - dw) / 2
                        hb = (th - dh) / 2

                    Case eScalingMode.Native_ScaleToAR

                        'CALCULATE NEW WIDTH
                        ratio = ax / ay
                        dw = ratio * sh

                        'CALCULATE BAR SIZES
                        vb = (tw - dw) / 2
                        hb = (th - dh) / 2

                    Case eScalingMode.Maximize_NativeAR

                        'SET DESTINATION VIDEO W & H
                        dw = tw
                        dh = (tw / sw) * sh

                        If dh > th Then 'expanding width to tw makes dh bigger than th so we must set dh to th and recompute dw
                            dh = th
                            dw = (th / sh) * sw

                            'SET BAR SIZES
                            hb = 0
                            vb = (tw - dw) / 2

                        Else
                            'SET BAR SIZES
                            vb = 0 'we've scaled the width upto picture width
                            hb = th - dh
                        End If


                    Case eScalingMode.Maximize_ScaleToAR

                        'CALCULATE SCALED NATIVE WIDTH
                        ratio = ax / ay
                        Dim scaled_native_width As Integer = ratio * sh 'here we set sw so it can be used below

                        'SET DESTINATION VIDEO W & H
                        dw = tw
                        dh = (tw / scaled_native_width) * sh

                        If dh > th Then
                            dh = th
                            dw = (th / sh) * scaled_native_width

                            'SET BAR SIZES
                            hb = 0
                            vb = (tw - dw) / 2

                        Else
                            'SET BAR SIZES
                            vb = 0 'we've scaled the width upto picture width
                            hb = (th - dh) / 2
                        End If

                End Select

                'SET BARS
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_AppendixMode, 1) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_AppendixStripesType, 2) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_StripesSizeValue2, vb) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_StripesSizeValue, hb) Then Return False

                'SET SCALING
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_ResizeImage, 1) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_ResizeType, 2) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_DestWidth, dw) Then Return False
                If Not MCE_ImgSiz_IMC.SetParamValue(mMainConcept_Scaler.MC_SCALER_DestHeight, dh) Then Return False

                'Debug.WriteLine(vbNewLine & "===========================")
                'Debug.WriteLine("SET SCALER")
                'Debug.WriteLine("Vertical Bars = " & vb)
                'Debug.WriteLine("Horizontal Bars = " & hb)
                'Debug.WriteLine("Width = " & dw)
                'Debug.WriteLine("Height = " & dh)
                'Debug.WriteLine("===========================" & vbNewLine)

                MCE_ImgSiz_IMC.CommitChanges()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with MCE_SetScaling(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:SCALER

#Region "FILTERS:MAINCONCEPT:CSC"

        Public MCE_CSC As IBaseFilter
        Public MCE_CSC_In, MCE_CSC_Out As IPin
        Public MCE_CSC_PP As ISpecifyPropertyPages
        Public MCE_CSC_IMC As ModuleConfig

        Public Function AddMCE_CSC() As Boolean
            Try
                Dim o As Object = DsBugWO.CreateDsInstance(New Guid("272D77A0-A852-4851-ADA4-9091FEAD4C86"), Clsid.IBaseFilter_GUID)
                MCE_CSC = CType(o, IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_CSC, "MCE_CSC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MCE_CSC)
                MCE_CSC_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = MCE_CSC_IMC.SetParamValue(New Guid("595D3585-21B1-4056-B0C7-2D7E4A9BD006"), 2086917237) 'set input media type to yv16
                MCE_CSC_IMC.CommitChanges()

                'dwFourcc_yv16	2086917237	unsigned long
                'dwFourcc_uyvy	1498831189	unsigned long

                HR = MCE_CSC.FindPin("In", MCE_CSC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                MCE_CSC_PP = CType(MCE_AVC, ISpecifyPropertyPages)

                Return True
            Catch ex As Exception
                Throw New Exception(ex.ToString)
                Throw New Exception("Problem with AddMCE_CSC. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:CSC

#Region "FILTERS:MAINCONCEPT:MPA"

        Public MC_MPADEC As IBaseFilter
        Public MC_MPADEC_In, MC_MPADEC_Out As IPin
        Public MC_MPADEC_PP As ISpecifyPropertyPages
        Public MC_MPADEC_IMC As ModuleConfig

        Public Function AddMC_MPADEC() As Boolean
            Try
                Dim o As Object = DsBugWO.CreateDsInstance(New Guid("2F75E451-A88C-4939-BFE5-D92D48C102F2"), Clsid.IBaseFilter_GUID)
                MC_MPADEC = CType(o, IBaseFilter)
                HR = GraphBuilder.AddFilter(MC_MPADEC, "MCE_AVC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(MC_MPADEC)
                MC_MPADEC_IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean = MC_MPADEC_IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing)
                MC_MPADEC_IMC.CommitChanges()

                'EnumeratePinNames(MC_MPADEC)

                HR = MC_MPADEC.FindPin("In", MC_MPADEC_In) 'for mc dmx
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                MC_MPADEC_PP = CType(MC_MPADEC, ISpecifyPropertyPages)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMC_MPADEC. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:MPA

#Region "FILTERS:MAINCONCEPT:SHARED"

        Public Function MCE_ToggleDecoderOSD(ByVal TurnOn As Boolean) As Boolean
            Try
                Return MCE_IMC_SetParamValue(ModuleConfig_Consts.EMC_OSD, Convert.ToInt32(TurnOn))
                'If MCE_IMC Is Nothing Then Return False
                'Dim Res As Boolean = False
                'Res = MCE_IMC.SetParamValue(New Guid("F5C51906-ED89-4c6e-9C37-A5CCB34F5389"), Convert.ToInt32(TurnOn))
                'MCE_IMC.CommitChanges()
                'Return Res
            Catch ex As Exception
                Throw New Exception("Problem with MCE_ToggleDecoderOSD(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function MCE_IMC_SetParamValue(ByVal TargetValue As Guid, ByVal Value As Object) As Boolean
            Try
                If MCE_IMC Is Nothing Then Return False
                Dim Res As Boolean = False
                Res = MCE_IMC.SetParamValue(TargetValue, Value)
                MCE_IMC.CommitChanges()
                Return Res
            Catch ex As Exception
                Throw New Exception("Problem with MCE_IMC_SetParamValue(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FILTERS:MAINCONCEPT:SHAREDSHARED

#End Region 'FILTERS:MAINCONCEPT

#Region "FILTERS:MEDIALOOKS"

#Region "FILTERS:MEDIALOOKS:QTSRC"

        Public ML_QT As IBaseFilter
        Public ML_QT_Vid, ML_QT_Aud As IPin
        Public ML_QT_FSF As IFileSourceFilter

        Public Function AddML_QT(ByVal nPath As String) As Boolean
            Try
                ML_QT = CType(DsBugWO.CreateDsInstance(New Guid("7CE55CCC-403E-4A29-8281-BF8542A0C37D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(ML_QT, "ML QT")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                ML_QT_FSF = CType(ML_QT, IFileSourceFilter)
                HR = ML_QT_FSF.Load(nPath, 0)
                HR = ML_QT.FindPin("Video", ML_QT_Vid)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = ML_QT.FindPin("Audio", ML_QT_Aud)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddML_QT(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MEDIALOOKS:QTSRC

#Region "FILTERS:MEDIALOOKS:AUDIO MIXER"

        Public ML_AudioMixer As IBaseFilter
        'Public ML_iAudioMixer As ?
        Public ML_AudioMixer_In, ML_AudioMixer_Out As IPin

        Public Function AddMediaLooksAudioMixer() As Boolean
            Try
                ML_AudioMixer = CType(DsBugWO.CreateDsInstance(New Guid("C54EAA68-7047-415D-A7EB-5131A655A6CD"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(ML_AudioMixer, "ML AM")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'AMTC_Interface = CType(ML_AudioMixer, IAMTC)
                'Me.EnumeratePinNames(ML_AudioMixer)
                HR = ML_AudioMixer.FindPin("Input 01", ML_AudioMixer_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = ML_AudioMixer.FindPin("AM Out 01", ML_AudioMixer_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMediaLooksAudioMixer(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MEDIALOOKS:AUDIO MIXER

#End Region 'FILTERS:MEDIALOOKS

#Region "FILTERS:MICROSOFT"

#Region "FILTERS:MICROSOFT:DVD_NAV"

        Public DVDNavigator As IBaseFilter
        Public DVDCtrl As IDvdControl2
        Public DVDInfo As IDvdInfo2
        Public DVDNavigator_PP As ISpecifyPropertyPages
        Public DVDNav_AudPin As IPin
        Public DVDNav_SubPin As IPin
        Public DVDNav_VidPin As IPin

        Public Function AddDVDNav() As Boolean
            Try
                DVDNavigator = CType(DsBugWO.CreateDsInstance(New Guid("9B8C4620-2C1A-11D0-8493-00A02438AD48"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(DVDNavigator, "DVD Navigator")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                DVDCtrl = CType(DVDNavigator, IDvdControl2)

                'HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.ResetOnStop, True)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_NotifyParentalLevelChange, True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_HMSF_TimeCodeEvents, True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_AudioDuringFFwdRew, False)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_EnableExtendedCopyProtectErrors, True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_NotifyPositionChange, True)
                If HR < 0 Then Debug.WriteLine("DVD Nav SetOption Failed: DVD_NotifyPositionChange")
                HR = DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_EnableLoggingEvents, True)
                If HR < 0 Then Debug.WriteLine("DVD Nav SetOption Failed: DVD_EnableLoggingEvents")

                DVDInfo = CType(DVDNavigator, IDvdInfo2)
                DVDNavigator_PP = CType(DVDNavigator, ISpecifyPropertyPages)
                HR = DVDNavigator.FindPin("Video", DVDNav_VidPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDNavigator.FindPin("AC3", DVDNav_AudPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DVDNavigator.FindPin("SubPicture", DVDNav_SubPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDVDNav(). Error: " & ex.Message)
            End Try
        End Function

        Public Function InitializeDVDNavigator(ByRef Languages As cLanguages, ByRef Countries As cCountries, ByVal NavigatorSetup As sNavigatorSetup) As Boolean
            Try
                'MENUS
                Dim ML As cLanguage = Languages.GetLanguageByName(NavigatorSetup.DEFAULT_MENU_LANGUAGE.ToString)
                HR = Me.DVDCtrl.SelectDefaultMenuLanguage(ML.DecimalValue)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'AUDIO
                Dim AL As cLanguage = Languages.GetLanguageByName(NavigatorSetup.DEFAULT_AUDIO_LANGUAGE.ToString)
                HR = Me.DVDCtrl.SelectDefaultAudioLanguage(CInt(AL.DecimalValue), NavigatorSetup.DEFAULT_AUDIO_EXTENSION)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'SUBTITLES
                Dim subext As DvdSubPicLangExt = GetSubtitleExtensionFromString(NavigatorSetup.DEFAULT_SUBTITLE_EXTENSION.ToString)
                Dim SL As cLanguage = Languages.GetLanguageByName(NavigatorSetup.DEFAULT_SUBTITLE_LANGUAGE.ToString)
                HR = Me.DVDCtrl.SelectDefaultSubpictureLanguage(SL.DecimalValue, subext)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'ASPECT RATIO
                HR = Me.DVDCtrl.SelectVideoModePreference(NavigatorSetup.ASPECT_RATIO)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Me.KO_IKeystoneMixer.SetResizeMode(NavigatorSetup.ASPECT_RATIO)
                'HR = Me.KO_IKeystoneMixer.SetResizeMode(NavigatorSetup.ASPECT_RATIO)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'PARENTAL LEVEL
                If Not NavigatorSetup.PARENTAL_LEVEL = eParentalLevels.PL_Off Then
                    HR = Me.DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_NotifyParentalLevelChange, True)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    Dim PaC As cCountry = Countries.GetCountryByName(NavigatorSetup.PARENTAL_COUNTRY.ToString)

                    'Dim CA As cCountryNameAlpha2Pair = CType(cbParentalCountry.SelectedItem, cCountryNameAlpha2Pair)
                    Dim CA As String = Countries.GetCountryAlpha2(NavigatorSetup.PARENTAL_COUNTRY.ToString)
                    Dim CR() As Byte = System.Text.Encoding.ASCII.GetBytes(CA)

                    HR = Me.DVDCtrl.SelectParentalCountry(CR)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Me.DVDCtrl.SelectParentalLevel(CInt(NavigatorSetup.PARENTAL_LEVEL))
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    'To disable parental management after it has been enabled, pass 0xffffffff for ulParentalLevel.
                    HR = Me.DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_NotifyParentalLevelChange, False)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Me.DVDCtrl.SelectParentalLevel(4294967295)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with InitializeDVDNavigator(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:DVD_NAV

#Region "FILTERS:MICROSOFT:AVI_SPL"

        Public AVISplitter As IBaseFilter

        Public Function AddAVISplitter() As Boolean
            Try
                '1B544C20-FD0B-11CE-8C63-00AA0044B51E
                AVISplitter = CType(DsBugWO.CreateDsInstance(New Guid("1B544C20-FD0B-11CE-8C63-00AA0044B51E"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(AVISplitter, "AVI Splitter")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddAVISplitter. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:AVI_SPL

#Region "FILTERS:MICROSOFT:VC1 DMO A"

        'Public VC1_DMO As IBaseFilter
        Public MS_VC1A_In, MS_VC1A_Out As IPin
        Public VC1A_PP As ISpecifyPropertyPages

        Public Function AddVC1DMOA() As Boolean
            Try
                'HKEY_CLASSES_ROOT\DirectShow\MediaObjects\c9bfbccf-e60e-4588-a3df-5a03b1fd9585  'Single-threaded DMO
                'HKEY_CLASSES_ROOT\DirectShow\MediaObjects\82d353df-90bd-4382-8bc2-3f6192b76e34  'Multi-threaded DMO
                If Not AddDMOWrapperAndDMO(New Guid("82d353df-90bd-4382-8bc2-3f6192b76e34"), DMOCATEGORY_VIDEO_DECODER) Then
                    Throw New Exception("Couldn't create wrapper for MS VC-1 DMO")
                    Return False
                End If
                HR = DMOWrapper.FindPin("in0", MS_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DMOWrapper.FindPin("out0", MS_VC1A_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddVC1DMO. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:VC1 DMO A

#Region "FILTERS:MICROSOFT:VC1 DMO B"

        'Public VC1_DMO As IBaseFilter
        Public MS_VC1B_In, MS_VC1B_Out As IPin
        Public VC1B_PP As ISpecifyPropertyPages

        Public Function AddVC1DMOB() As Boolean
            Try
                'HKEY_CLASSES_ROOT\DirectShow\MediaObjects\c9bfbccf-e60e-4588-a3df-5a03b1fd9585  'Single-threaded DMO
                'HKEY_CLASSES_ROOT\DirectShow\MediaObjects\82d353df-90bd-4382-8bc2-3f6192b76e34  'Multi-threaded DMO
                If Not AddDMOWrapperAndDMO(New Guid("82d353df-90bd-4382-8bc2-3f6192b76e34"), DMOCATEGORY_VIDEO_DECODER) Then
                    Throw New Exception("Couldn't create wrapper for MS VC-1 DMO")
                    Return False
                End If
                HR = DMOWrapper.FindPin("in0", MS_VC1B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = DMOWrapper.FindPin("out0", MS_VC1B_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddVC1DMO. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:VC1 DMO B

#Region "FILTERS:MICROSOFT:VMR9"

        Public VMR9 As IBaseFilter
        Public VMRDeinterlaceControl As IVMRDeinterlaceControl9
        Public VMRFilterConfig As IVMRFilterConfig9
        Public VMRAspectRatio As IVMRAspectRatioControl9
        Public VMRMixerBitmap As IVMRMixerBitmap9
        Public VMRMixerControl As IVMRMixerControl9
        Public VMRMonitorConfig As IVMRMonitorConfig9
        Public VMRWindowlessControl As IVMRWindowlessControl9
        Public VMR9_In_1, VMR_In_2 As IPin
        Public UsingVMR9 As Boolean = False
        Public BasicVideo As IBasicVideo2

        Public Function AddVMR9(ByVal NumberOfPins As Short) As Boolean
            Try
                VMR9 = CType(DsBugWO.CreateDsInstance(New Guid("51B4ABF3-748F-4E3B-A276-C828330E926A"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(VMR9, "Video Mixing Renderer 9")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                VMRFilterConfig = CType(VMR9, IVMRFilterConfig9)
                VMRAspectRatio = CType(VMR9, IVMRAspectRatioControl9)
                VMRMixerBitmap = CType(VMR9, IVMRMixerBitmap9)
                VMRMonitorConfig = CType(VMR9, IVMRMonitorConfig9)
                VideoRenderer_PP = CType(VMR9, ISpecifyPropertyPages)
                'BasicVideo = CType(GB, IBasicVideo2)

                'VMR9 Setup
                '1) Add pins
                HR = VMRFilterConfig.SetNumberOfStreams(Convert.ToUInt32(NumberOfPins))
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = VMR9.FindPin("VMR Input0", VMR9_In_1)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If NumberOfPins > 1 Then
                    HR = VMR9.FindPin("VMR Input1", Me.VMR_In_2)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                '2)windowed or windowless?
                'use windowed, windowless is buggy, or so says Yaron/InMatrix
                HR = VMRFilterConfig.SetRenderingMode(VMR9Mode.VMR9Mode_Windowed)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                '3) Aspect ratio
                HR = VMRAspectRatio.SetAspectRatioMode(VMR9AspectRatioMode.VMR9ARMode_None)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                '4) Get Mixer Control
                VMRMixerControl = CType(VMR9, IVMRMixerControl9)

                UsingVMR9 = True

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddVMR9. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Private Function ConvertInterlaceFlags(ByVal dwInterlaceFlags As Integer) As VMR9_SampleFormat
            Try
                If (dwInterlaceFlags And AM_INTERLACE.AMINTERLACE_IsInterlaced) Then
                    If (dwInterlaceFlags And AM_INTERLACE.AMINTERLACE_1FieldPerSample) Then
                        If (dwInterlaceFlags And AM_INTERLACE.AMINTERLACE_Field1First) Then
                            Return VMR9_SampleFormat.VMR9_SampleFieldSingleEven
                        Else
                            Return VMR9_SampleFormat.VMR9_SampleFieldSingleOdd
                        End If
                    Else
                        If (dwInterlaceFlags And AM_INTERLACE.AMINTERLACE_Field1First) Then
                            Return VMR9_SampleFormat.VMR9_SampleFieldInterleavedEvenFirst
                        Else
                            Return VMR9_SampleFormat.VMR9_SampleFieldInterleavedOddFirst
                        End If
                    End If
                Else
                    Return VMR9_SampleFormat.VMR9_SampleProgressiveFrame
                End If
            Catch ex As Exception
                Throw New Exception("Problem converting interlace flags. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:VMR9

#Region "FILTERS:MICROSOFT:VC-1 ES Parser A"

        Public MS_VC1ESParserA As IBaseFilter
        Public MS_VC1ESParserA_In, MS_VC1ESParserA_Out As IPin

        Public Function AddMS_VC1ESParserA() As Boolean
            Try
                MS_VC1ESParserA = CType(DsBugWO.CreateDsInstance(New Guid("DBB2C1E7-DA10-4C25-91CA-525502025EA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MS_VC1ESParserA, "VC-1 ES Parser")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                EnumeratePinNames(MS_VC1ESParserA)
                HR = MS_VC1ESParserA.FindPin("Input", MS_VC1ESParserA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with MS_VC1ESParser. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:VC-1 ES Parser A

#Region "FILTERS:MICROSOFT:VC-1 ES Parser B"

        Public MS_VC1ESParserB As IBaseFilter
        Public MS_VC1ESParserB_In, MS_VC1ESParserB_Out As IPin

        Public Function AddMS_VC1ESParserB() As Boolean
            Try
                MS_VC1ESParserB = CType(DsBugWO.CreateDsInstance(New Guid("DBB2C1E7-DA10-4C25-91CA-525502025EA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MS_VC1ESParserB, "VC-1 ES Parser")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                EnumeratePinNames(MS_VC1ESParserB)
                HR = MS_VC1ESParserB.FindPin("Input", MS_VC1ESParserB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with MS_VC1ESParser. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:VC-1 ES Parser B

#Region "FILTERS:MICROSOFT:FSF A"

        Public FileSourceA As IBaseFilter
        Public iFSFA As IFileSourceFilter
        Public FSFA_Out As IPin

        Public Function AddFilesourceA(ByVal FilePath As String) As Boolean
            Try
                FileSourceA = CType(DsBugWO.CreateDsInstance(New Guid("E436EBB5-524F-11CE-9F53-0020AF0BA770"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(FileSourceA, "FSF")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                iFSFA = CType(FileSourceA, IFileSourceFilter)
                HR = iFSFA.Load(FilePath, 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = FileSourceA.FindPin("Output", FSFA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddFilesource A. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:FSF A

#Region "FILTERS:MICROSOFT:FSF B"

        Public FileSourceB As IBaseFilter
        Public iFSFB As IFileSourceFilter
        Public FSFB_Out As IPin

        Public Function AddFilesourceB(ByVal FilePath As String) As Boolean
            Try
                FileSourceB = CType(DsBugWO.CreateDsInstance(New Guid("E436EBB5-524F-11CE-9F53-0020AF0BA770"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(FileSourceB, "FSF")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                iFSFB = CType(FileSourceB, IFileSourceFilter)
                HR = iFSFB.Load(FilePath, 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = FileSourceB.FindPin("Output", FSFB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddFilesourceB. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:FSF B

#Region "FILTERS:MICROSOFT:L21 1"

        Public Line21Decoder As IBaseFilter
        Public Line21 As IAMLine21Decoder
        Public Line21Decoder_PP As ISpecifyPropertyPages

        Public Function AddLine21Decoder1() As Boolean
            Try
                Line21Decoder = CType(DsBugWO.CreateDsInstance(New Guid("6E8D4A20-310C-11D0-B79A-00AA003767A7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(Line21Decoder, "Line 21 Decoder")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Line21 = CType(Line21Decoder, IAMLine21Decoder)
                HR = Line21.SetServiceState(AMLine21_CCState.Off)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddLine21Decoder1. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:L21 1

#Region "FILTERS:MICROSOFT:L21 2"

        Public Function AddLine21Decoder2() As Boolean
            Try
                Line21Decoder = CType(DsBugWO.CreateDsInstance(New Guid("E4206432-01A1-4BEE-B3E1-3702C8EDC574"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(Line21Decoder, "Line 21 Decoder 2")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Line21 = CType(Line21Decoder, IAMLine21Decoder)
                HR = Line21.SetServiceState(AMLine21_CCState.Off)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddLine21Decoder2. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:L21 2

#Region "FILTERS:MICROSOFT:PIN TEE"

        Public PinTee As IBaseFilter
        Public PinTee_In, PinTee_Out1, PinTee_Out2 As IPin

        Public Function AddPinTee() As Boolean
            Try
                PinTee = CType(DsBugWO.CreateDsInstance(New Guid("F8388A40-D5BB-11D0-BE5A-0080C706568E"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(PinTee, "Pin Tee")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = PinTee.FindPin("Input", PinTee_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = PinTee.FindPin("Output1", PinTee_Out1)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddPinTee. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:PIN TEE

#Region "FILTERS:MICROSOFT:WAV PARSER"

        Public WaveParser As IBaseFilter
        Public WP_In, WP_Out As IPin

        Public Function AddWaveParser() As Boolean
            Try
                WaveParser = CType(DsBugWO.CreateDsInstance(New Guid("D51BD5A1-7548-11CF-A520-0080C77EF58A"), Clsid.IBaseFilter_GUID), IBaseFilter)
                GraphBuilder.AddFilter(WaveParser, "WP")
                HR = WaveParser.FindPin("input pin", WP_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddWaveParser. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:WAV PARSER

#Region "FILTERS:MICROSOFT:DIRECTSOUND RENDERER"

        Public Function AddDefaultDirectSoundDevice() As Boolean
            Try
                AudioRenderer = CType(DsBugWO.CreateDsInstance(New Guid("79376820-07D0-11CF-A24D-0020AFD79767"), Clsid.IBaseFilter_GUID), IBaseFilter)
                GraphBuilder.AddFilter(AudioRenderer, "Default DirectSound Device")

                Dim cnt As Integer
                Dim Pins(0) As Integer
                Dim PinEnum As IEnumPins
                HR = AudioRenderer.EnumPins(PinEnum)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = PinEnum.Next(1, Pins, cnt)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim ptr As IntPtr = New IntPtr(CInt(Pins(0)))
                Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
                Dim tPin As IPin = CType(obj, IPin)
                Dim PinID As String
                HR = tPin.QueryId(PinID)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = AudioRenderer.FindPin(PinID, AudRen_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDefaultDirectSoundDevice. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:DIRECTSOUND RENDERER

#Region "FILTERS:MICROSOFT:DMO WRAPPER"

        Public DMOWrapper As IBaseFilter
        Public iDMOW As IDMOWrapperFilter

        Public Function AddDMOWrapperAndDMO(ByVal DMOGUID As Guid, ByVal DMOCategory As Guid) As Boolean
            Try
                DMOWrapper = CType(DsBugWO.CreateDsInstance(CLSID_DMOWrapperFilter, Clsid.IBaseFilter_GUID), IBaseFilter)
                If DMOWrapper Is Nothing Then
                    Throw New Exception("DMOWRAPPER DID NOT INSTANTIATE")
                    Return False
                End If
                iDMOW = CType(DMOWrapper, IDMOWrapperFilter)

                If iDMOW Is Nothing Then
                    Throw New Exception("Didn't get interface on dmo wrapper")
                    Return False
                End If

                'HR = iDMOW.Init(DMOGUID.ToByteArray, CLSID_NULL.ToByteArray)
                HR = iDMOW.Init(DMOGUID.ToByteArray, DMOCategory.ToByteArray)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = GraphBuilder.AddFilter(DMOWrapper, "DMOWrapper")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddDMOWrapperAndDMO. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:MICROSOFT:DMO WRAPPER

#End Region 'FILTERS:MICROSOFT

#Region "FILTERS:FFDSHOW"

#Region "FILTERS:FFDSHOW:AUD DEC"

        Public FFD_AudDec As IBaseFilter
        Public FFD_AudDec_In, FFD_AudDec_Out As IPin

        Public Function AddFFD_AudDec() As Boolean
            Try
                FFD_AudDec = CType(DsBugWO.CreateDsInstance(New Guid("0F40E1E5-4F79-4988-B1A9-CC98794E6B55 "), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(FFD_AudDec, "FFD AD")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'HR = FFD_AudDec.FindPin("Input", FFD_AudDec_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddFFD_AudDec. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:FFDSHOW:AUD DEC

#End Region 'FILTERS:FFDSHOW

#Region "FILTERS:ELECARD"

#Region "FILTERS:ELECARD:CHEGGEPUGA"

        Public MCE_CHG As IBaseFilter
        Public MCE_CHG_In As IPin

        Public Function AddMCE_CHG() As Boolean
            Try
                MCE_CHG = CType(DsBugWO.CreateDsInstance(New Guid("5600EDB4-FE9A-4901-A504-29C855A7067C"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(MCE_CHG, "MC CHG")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = MCE_CHG.FindPin("In", MCE_CHG_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddMCE_CHG. Error: " & ex.Message)
                Return False
            End Try
        End Function

#End Region 'FILTERS:ELECARD:CHEGGEPUGA

#End Region 'FILTERS:ELECARD

#Region "FILTERS:AJA"

#Region "FILTERS:AJA:RENDERER"

        Public AJARen As IBaseFilter
        Public AJA_Audio, AJA_Video As IPin
        Public Function AddAJARenderer() As Boolean
            Try
                AJARen = CType(DsBugWO.CreateDsInstance(New Guid("0890EBDD-1A17-432F-9146-E87926EE412E"), Clsid.IBaseFilter_GUID), IBaseFilter)
                GraphBuilder.AddFilter(AJARen, "AJA Renderer")
                AJARen.FindPin("Video", AJA_Video)
                AJARen.FindPin("Audio", AJA_Audio)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddAJARenderer(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FILTERS:AJA:RENDERER

#End Region 'FILTERS:AJA

#Region "FILTERS:CORE"

#Region "FILTERS:CORE:AAC AUD DEC"

        Public CoreAACAudDec As IBaseFilter
        Public CoreAACAudDec_In, CoreAACAudDec_Out As IPin

        Public Function AddCoreAACAudDec() As Boolean
            Try
                CoreAACAudDec = CType(DsBugWO.CreateDsInstance(New Guid("6AC7C19E-8CA0-4E3D-9A9F-2881DE29E0AC"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(CoreAACAudDec, "CAAC AD")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddCoreAACAudDec. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:CORE:AAC AUD DEC

#End Region 'FILTERS:CORE

#Region "FILTERS:HAALI"

        Public HaaliSplitter As IBaseFilter
        Public HS_In, HS_Out As IPin

        Public Function AddHaaliSplitter() As Boolean
            Try
                HaaliSplitter = CType(DsBugWO.CreateDsInstance(New Guid("564FD788-86C9-4444-971E-CC4A243DA150"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(HaaliSplitter, "HS")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = HaaliSplitter.FindPin("Input", HS_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddHaaliSplitter. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:HAALI

#Region "FILTERS:LEAD"

#Region "FILTERS:LEAD:DEINTERLACER"

        Public LEAD_Deinterlacer As IBaseFilter
        Public LD_DINT_In, LD_DINT_Out As IPin

        Public Function AddLEAD_Deinterlacer() As Boolean
            Try
                LEAD_Deinterlacer = CType(DsBugWO.CreateDsInstance(New Guid("E2B7DD90-38C5-11D5-91F6-00104BDB8FF9"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(LEAD_Deinterlacer, "DINT")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = LEAD_Deinterlacer.FindPin("In", LD_DINT_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = LEAD_Deinterlacer.FindPin("XForm Out", LD_DINT_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AddLEAD_Deinterlacer. Error: " & ex.Message)
            End Try
        End Function

#End Region 'FILTERS:LEAD:DEINTERLACER

#End Region 'FILTERS:LEAD

#End Region 'FILTERS

#Region "ADD AUDIO"

        Public Function AddAudioToGraph(ByVal AudioPath As String) As Boolean
            Try
                Select Case Path.GetExtension(AudioPath).ToLower

                    Case ".dts", ".ac3"
                        AddDTSAC3Source(AudioPath)
                        HR = IDTSAC3.Load(AudioPath, 0)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = DTSAC3Source.FindPin("1", DTSAC3_Out)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        AddNVidiaAudioDecoder(False)
                        AddAMTC()
                        AddDeckLinkAudio()

                        HR = GraphBuilder.Connect(DTSAC3_Out, AudDec_InPin)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = GraphBuilder.Connect(AudDec_OutPin, AMTC_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = GraphBuilder.Connect(AMTC_Out, AudRen_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        Return True

                    Case ".pcm", ".wav"
                        'File source filter
                        AddFilesourceA(AudioPath)

                        'Wave Parser
                        AddWaveParser()

                        'Decklink Audio Renderer
                        AddDeckLinkAudio()

                        HR = GraphBuilder.Connect(FSFA_Out, WP_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = WaveParser.FindPin("output", WP_Out)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = GraphBuilder.Connect(WP_Out, AudRen_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        Return True

                    Case ".mpa"
                        ' Not currently supported
                        Return False

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with AddAudioToGraph(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'ADD AUDIO

        Public Function GetSystemClock() As IReferenceClock
            Try
                Return CType(DsBugWO.CreateDsInstance(Clsid.CLSID_SystemClock, Clsid.IID_IReferenceClock), IReferenceClock)
            Catch ex As Exception
                Throw New Exception("Problem with GetSystemClock(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class 'cSMTGraph

End Namespace
