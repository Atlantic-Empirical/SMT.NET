Imports SMT.Multimedia.Formats.DVD.IFO
Imports System.IO
Imports SMT.Multimedia.Formats.DVD.Globalization
Imports SMT.Multimedia.Utility.Timecode

Namespace Multimedia.Mounting

    Public Class cMountableMedia_DVD
        Inherits cMountableMedia_Base

        Public DVD As cDVD
        Public Streams As List(Of cDVDStreamNfo)
        Public LongestGTT As cGlobalTT
        Public Languages As cLanguages
        Public SelectedAudioIndex As Integer
        Public SelectedSubtitleIndex As Integer

        Public Sub New(ByVal VIDEO_TS As String)
            Try
                Me.MediaType = eMediaType.DVD_Video

                If Microsoft.VisualBasic.Right(VIDEO_TS, 8).ToLower <> "video_ts" Then
                    VIDEO_TS &= "\video_ts"
                End If

                DVD = New cDVD(VIDEO_TS)
                Streams = New List(Of cDVDStreamNfo)

                'look for feature video or identify episodes
                'go through the GTTs in the VMGM

                'RULES:
                ' 1) Ignore menu space
                ' 2) Ignore non-one sequential titles
                ' 3) Ignore non-global title content

                'CURRENTLY:
                ' 1) Only supporting the longest GTT in the DVD

                Dim record As ULong = 0
                Dim current As ULong = 0
                Dim tTC As cTimecode
                For Each GTT As cGlobalTT In DVD.VMGM.GlobalTTs
                    If GTT.Type.Sequential = "One_Sequential_PGC" Then
                        tTC = DVD.GetGTTDuration(GTT.GlobalTT_N)
                        current = tTC.TotalSeconds
                        If current > record Then
                            record = current
                            LongestGTT = GTT
                            LongestGTT.RunningTime = tTC
                        End If
                    End If
                Next

                Dim Assm As System.Reflection.Assembly = Me.GetType.Assembly
                Dim LanguagesCSV As Stream = Assm.GetManifestResourceStream("SMT.Common.Languages.csv")
                Languages = New cLanguages(LanguagesCSV)

                Dim tA As cTitleAudioAttributes.cVTS_AST_ATRT
                For i As Integer = 0 To DVD.VTSs(LongestGTT.VTSN - 1).VTS_AudioStreamCount - 1
                    tA = DVD.VTSs(LongestGTT.VTSN - 1).VTS_AudioAttributes.Streams(i)
                    Streams.Add(New cDVDStreamNfo(cDVDStreamNfo.eStreamType.Audio, i, Languages.ShortLangString2LongLangString(tA.LanguageCodeAsString), tA.LanguageCode_Extension.ToString, tA.ChannelCount.ToString, tA.CodingMode.ToString))
                Next

                Dim tS As cSubpictureAttributes.cVXXX_SPST_ATR
                For i As Integer = 0 To DVD.VTSs(LongestGTT.VTSN - 1).VTS_SubpictureStreamCount - 1
                    tS = DVD.VTSs(LongestGTT.VTSN - 1).VTS_SubpictureAttributes.Streams(i)
                    If tS.LangCodeAsString <> "" Then
                        Streams.Add(New cDVDStreamNfo(cDVDStreamNfo.eStreamType.Subtitle, i, Languages.ShortLangString2LongLangString(tS.LangCodeAsString), tS.LangCodeEx.ToString))
                    End If
                Next

            Catch ex As Exception
                Throw New Exception("Problem with New() cMountableMedia_DVD. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cDVDStreamNfo

        Public Number As Integer
        Public Language As String
        Public Extension As String
        Public Type As eStreamType

        'AV Specific
        Public Compression As String

        'Audio Specific
        Public ChannelCount As String

        Public Sub New()
        End Sub

        'For subtitles
        Public Sub New(ByVal nType As eStreamType, ByVal nNumber As Integer, ByVal nLang As String, ByVal nExtension As String)
            Type = nType
            Number = nNumber
            Language = nLang
            Extension = nExtension
        End Sub

        'For audio
        Public Sub New(ByVal nType As eStreamType, ByVal nNumber As Integer, ByVal nLang As String, ByVal nExtension As String, ByVal nChannels As String, ByVal nCompression As String)
            Type = nType
            Number = nNumber
            Language = nLang
            Extension = nExtension
            ChannelCount = nChannels
            Compression = nCompression
        End Sub

        Public Enum eStreamType
            Video
            Audio
            Subtitle
        End Enum

    End Class

End Namespace
