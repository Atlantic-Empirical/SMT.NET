
Namespace Multimedia.Players.DVD

    Public Module mDVDDataTypeConversions

        Public Function GetAudioExtAsInt(ByVal Ext As String) As Integer
            Try
                Select Case Ext.ToLower
                    Case "notspecified"
                        Return 0
                    Case "captions"
                        Return 1
                    Case "visuallyimpaired"
                        Return 2
                    Case "directorcomments1"
                        Return 3
                    Case "directorcomments2"
                        Return 4
                End Select
            Catch ex As Exception
                Return -1
            End Try
        End Function

        Public Function GetSubtitleExtAsInt(ByVal Ext As String) As Integer
            Try
                Select Case Ext.ToLower
                    Case "notspecified"
                        Return 0
                    Case "captionnormal"
                        Return 1
                    Case "captionbig"
                        Return 2
                    Case "captionchildren"
                        Return 3
                    Case "closednormal"
                        Return 5
                    Case "closedbig"
                        Return 6
                    Case "closedchildren"
                        Return 7
                    Case "forced"
                        Return 9
                    Case "directorcmtnormal"
                        Return 13
                    Case "directorcmtbig"
                        Return 14
                    Case "directorcmtchildren"
                        Return 15
                End Select
            Catch ex As Exception
                Return -1
            End Try
        End Function

        Public Function GetVideoModeAsInt(ByVal Mode As String) As Integer
            Try
                Select Case Mode
                    Case "CONTENT_DEFAULT"
                        Return 0
                    Case "16x9"
                        Return 1
                    Case "4x3_PANSCAN_PREFERRED"
                        Return 2
                    Case "4x3_LETTERBOX_PREFERRED"
                        Return 3
                End Select
            Catch ex As Exception
                Return -1
            End Try
        End Function

        Public Function GetDVDErrorString(ByVal P1 As String) As String
            Try
                Select Case P1
                    Case 1
                        Return "DVD_ERROR_Unexpected"   'Something unexpected happened, perhaps content is incorrectly authored.
                    Case 2
                        Return "DVD_ERROR_CopyProtectFail"  'Key exchange for DVD copy protection failed.
                    Case 3
                        Return "DVD_ERROR_InvalidDVD1_0Disc"    'DVD-Video disc is incorrectly authored for v1.0 of spec.
                    Case 4
                        Return "DVD_ERROR_InvalidDiscRegion"    'The Disc cannot be played because the disc is not authored to play in system region. The region mismatch may be fixable by changing the system region with dvdrgn.exe
                    Case 5
                        Return "DVD_ERROR_LowParentalLevel" 'Player parental level is lower than the lowest parental level available in the DVD content.
                    Case 6
                        Return "DVD_ERROR_MacrovisionFail"  'Macrovision Distribution Failed.
                    Case 7
                        Return "DVD_ERROR_IncompatibleSystemAndDecoderRegions"  'No discs can be played because the system region does not match the decoder region.
                    Case 8
                        Return "DVD_ERROR_IncompatibleDiscAndDecoderRegions"    'The disc cannot be played because the disc is not authored to be played in the decoder's region
                End Select
            Catch ex As Exception
                Throw New Exception("problem getting dvd error string. error: " & ex.Message)
            End Try
        End Function

        Public Function GetDVDWarningString(ByVal P1 As String) As String
            Try
                Select Case P1
                    Case 1
                        Return "InvalidDVD1_0Disc"  'DVD-Video disc is incorrectly authored. Playback can continue, but unexpected behavior may occur.
                    Case 2
                        Return "FormatNotSupported" 'A decoder would not support the current format.  Playback of a stream (audio, video of SP) may not function. lParam2 contains the stream type (see AM_DVD_STREAM_FLAGS)
                    Case 3
                        Return "IllegalNavCommand"  'The internal DVD navigation command processor attempted to process an illegal command.
                    Case 4
                        Return "Open"   'File Open Failed
                    Case 5
                        Return "Seek"   'File Seek Failed
                    Case 6
                        Return "Read"   'File Read Failed
                End Select
            Catch ex As Exception
                Throw New Exception("problem getting dvd warning string. error: " & ex.Message)
            End Try
        End Function

        Public Function GetPlaybackStoppedString(ByVal P1 As String) As String
            Try
                Select Case P1
                    Case 0
                        Return "DVD_PB_STOPPED_Other"   'The navigator stopped the playback (no reason available).
                    Case 1
                        Return "DVD_PB_STOPPED_NoBranch"    'The nav completed the current pgc and there was no more video and did not find any other branching instruction for subsequent playback.
                    Case 2
                        Return "DVD_PB_STOPPED_NoFirstPlayDomain"   'The disc does not contain an initial startup program.
                    Case 3
                        Return "DVD_PB_STOPPED_StopCommand" 'The app issued a stop() command or a stop command was authored on the disc.
                    Case 4
                        Return "DVD_PB_STOPPED_Reset"   'The navigator was reset to the start of the disc (using ResetOnStop).
                    Case 5
                        Return "DVD_PB_STOPPED_DiscEjected" 'The disc was ejected.
                    Case 6
                        Return "DVD_PB_STOPPED_IllegalNavCommand"   'An illegal nav command prevented playback from continuing.
                    Case 7
                        Return "DVD_PB_STOPPED_PlayPeriodAutoStop"  'PlayPeriod completed
                    Case 8
                        Return "DVD_PB_STOPPED_PlayChapterAutoStop" 'PlayChapter completed
                    Case 9
                        Return "DVD_PB_STOPPED_ParentalFailure" 'A parental level failure prevented playback
                    Case 10
                        Return "DVD_PB_STOPPED_RegionFailure"   'A region failure prevented playback
                    Case 11
                        Return "DVD_PB_STOPPED_MacrovisionFailure"  'A Macrovision failure prevented playback.
                    Case 12
                        Return "DVD_PB_STOPPED_DiscReadError"   'A read error prevented playback.
                    Case 13
                        Return "DVD_PB_STOPPED_CopyProtectFailure"  'Copy protection failure.
                End Select
            Catch ex As Exception
                Throw New Exception("problem getting dvd playback stopped string. error: " & ex.Message)
            End Try
        End Function

    End Module

End Namespace
