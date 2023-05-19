Imports System.Text
Imports System.Xml.Serialization
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.Multimedia.DirectShow

Namespace Multimedia.Utility.Timecode

    Public Class cTimecode

        Public Hours As Short
        Public Minutes As Short
        Public Seconds As Short
        Public Frames As Short
        Public Framerate As eFramerate

        Public Overloads Overrides Function ToString() As String
            Dim oH, oM, oS, [oF] As String
            oH = Hours
            oM = Minutes
            oS = Seconds
            [oF] = Frames
            If oH.Length = 1 And oH <> 0 Then oH = "0" & oH
            If oM.Length = 1 Then oM = "0" & oM
            If oS.Length = 1 Then oS = "0" & oS
            If [oF].Length = 1 Then [oF] = "0" & [oF]

            Return oH & ":" & oM & ":" & oS & "." & [oF]

        End Function

        Public Function ToString_NoFrames() As String
            Dim oH, oM, oS, [oF] As String
            oH = Hours
            oM = Minutes
            oS = Seconds
            If oM.Length = 1 Then oM = "0" & oM
            If oS.Length = 1 Then oS = "0" & oS
            Return oH & ":" & oM & ":" & oS
        End Function

        Public Function ToString_WithFrameRate()
            If Not Framerate = Nothing Then
                Return ToString() & " / " & Framerate.ToString
            Else
                Return ToString()
            End If
        End Function

        Public Sub New()
        End Sub

        Public Sub New(ByVal nTC As DvdTimeCode, ByVal NTSC As Boolean)
            Me.Hours = nTC.bHours
            Me.Minutes = nTC.bMinutes
            Me.Seconds = nTC.bSeconds
            Me.Frames = nTC.bFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByVal nHours As Byte, ByVal nMinutes As Byte, ByVal nSeconds As Byte, ByVal nFrames As Byte, ByVal NTSC As Boolean)
            Me.Hours = nHours
            Me.Minutes = nMinutes
            Me.Seconds = nSeconds
            Me.Frames = nFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByRef C_PBTM() As Byte, ByVal Offset As Long)
            Try
                Dim h10, h1, m10, m1, s10, s1, f10, f1 As Byte

                h10 = C_PBTM(Offset) >> 4 And 15
                h1 = C_PBTM(Offset) And 15

                m10 = C_PBTM(Offset + 1) >> 4 And 15
                m1 = C_PBTM(Offset + 1) And 15

                s10 = C_PBTM(Offset + 2) >> 4 And 15
                s1 = C_PBTM(Offset + 2) And 15

                f10 = C_PBTM(Offset + 3) >> 4 And 3
                f1 = C_PBTM(Offset + 3) And 15


                Hours = (h10 * 10) + h1
                Minutes = (m10 * 10) + m1
                Seconds = (s10 * 10) + s1
                Frames = (f10 * 10) + f1

                Select Case (C_PBTM(Offset + 3) >> 6) And 3
                    Case 3
                        Framerate = eFramerate.NTSC
                    Case 2
                        Framerate = Nothing
                    Case 1
                        Framerate = eFramerate.PAL
                    Case 0
                        Framerate = Nothing
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with New cTimecode from C_PBTM. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub New(ByVal GrossSeconds As UInt32, ByVal nFramerate As eFramerate)
            Me.New(CLng(GrossSeconds * nFramerate), nFramerate)
        End Sub

        Public Sub New(ByVal nFrames As Long, ByVal nFramerate As eFramerate)
            Dim res As Long = 0
            Me.Hours = Math.DivRem(nFrames, nFramerate * 3600, res)
            nFrames -= (nFramerate * 3600) * Hours
            Me.Minutes = Math.DivRem(nFrames, (nFramerate * 60), res)
            nFrames -= (nFramerate * 60) * Minutes
            Me.Seconds = Math.DivRem(nFrames, nFramerate, res)
            nFrames -= nFramerate * Seconds
            Me.Framerate = nFramerate

            If nFrames = nFramerate Then
                nFrames = 0
                Seconds += 1
            End If

            If Seconds = 60 Then
                Seconds = 0
                Minutes += 1
            End If

            If Minutes = 60 Then
                Minutes = 0
                Hours += 1
            End If

            Me.Frames = nFrames

        End Sub

        Public Sub New(ByVal REFERENCE_TIME As ULong, ByVal ATPF As UInt32, ByVal nFrameRate As eFramerate)
            Me.New(CLng(REFERENCE_TIME / ATPF), nFrameRate)
        End Sub

        Public ReadOnly Property DVDTimeCode() As DvdTimeCode
            Get
                Try
                    Dim out As New DvdTimeCode
                    out.bFrames = Me.Frames
                    out.bHours = Me.Hours
                    out.bMinutes = Me.Minutes
                    out.bSeconds = Me.Seconds
                    Return out
                Catch ex As Exception
                    MsgBox("Problem with ToDVDTimeCode. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public Sub Subtract(ByVal Hours As Byte, ByVal Minutes As Byte, ByVal Seconds As Byte, ByVal Frames As Byte)
            Try
                Dim tempTC As cTimecode = SubtractTimecode(Me, New cTimecode(Hours, Minutes, Seconds, Frames, Me.Framerate), Me.Framerate)
                Me.Frames = tempTC.Frames
                Me.Seconds = tempTC.Seconds
                Me.Minutes = tempTC.Minutes
                Me.Hours = tempTC.Hours
            Catch ex As Exception
                Throw New Exception("Problem with cTimecode.Subtract(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub Add(ByVal Hours As Byte, ByVal Minutes As Byte, ByVal Seconds As Byte, ByVal Frames As Byte)
            Try
                Dim tempTC As cTimecode = AddTimecode(Me, New cTimecode(Hours, Minutes, Seconds, Frames, Me.Framerate), Me.Framerate)
                Me.Frames = tempTC.Frames
                Me.Seconds = tempTC.Seconds
                Me.Minutes = tempTC.Minutes
                Me.Hours = tempTC.Hours
            Catch ex As Exception
                Throw New Exception("Problem with cTimecode.Add(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub AddGrossSeconds(ByVal Seconds As Integer)
            Try
                If Seconds = 0 Then Exit Sub

                Dim tempTC As cTimecode
                If Seconds > 0 Then
                    tempTC = AddTimecode(Me, New cTimecode(CUInt(Seconds), Me.Framerate), Me.Framerate)
                Else
                    tempTC = SubtractTimecode(Me, New cTimecode(CUInt(Math.Abs(Seconds)), Me.Framerate), Me.Framerate)
                End If
                Me.Frames = tempTC.Frames
                Me.Seconds = tempTC.Seconds
                Me.Minutes = tempTC.Minutes
                Me.Hours = tempTC.Hours
            Catch ex As Exception
                Throw New Exception("Problem with cTimecode.Add(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public ReadOnly Property TotalSeconds() As UInt64
            Get
                Dim out As UInt64 = Hours * 3600
                out += Minutes * 60
                out += Seconds
                Return out
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Class representing a timecode value (in either NTSC, PAL, or Film formats).
    ''' </summary>
    ''' <remarks>Use cTimecode to represent timecode values and perform 
    ''' timecode calculations, such as calculating duration.</remarks>
    <Serializable()> _
    Public Class cTimecode_MRJ

#Region "cTimecode_MRJ Public Constructors"

        ''' <summary>
        ''' Default constructor for the <i>cTimecode_MRJ</i> object.
        ''' </summary>
        ''' <remarks>Use to create a new <i>cTimecode_MRJ</i> object with default values
        ''' (NTSC-NDF 00:00:00:00).</remarks>
        Public Sub New()
            MyBase.New()
            Reset()
        End Sub

        ''' <summary>
        ''' Constructor for the <i>cTimecode_MRJ</i> object in which 
        ''' timecode format and value are specified.
        ''' </summary>
        ''' <param name="aTCFormat">Timecode format</param>
        ''' <param name="aDropFrame">Drop Frame indicator</param>
        ''' <param name="aTimecode">Timecode value (in string format)</param>
        ''' <remarks>Use to create a new <i>cTimecode_MRJ</i> instance with specified values.</remarks>
        Public Sub New(ByVal aTCFormat As eTimecodeFormat, ByVal aDropFrame As Boolean, ByVal aTimecode As String)
            Me.New()
            _TCFormat = aTCFormat
            _DropFrame = aDropFrame
            Me.Timecode = aTimecode     'Assign Timecode property to convert string
        End Sub

        Public Sub New(ByVal nTC As DvdTimeCode, ByVal NTSC As Boolean)
            Me.New()
            Me.Hours = nTC.bHours
            Me.Minutes = nTC.bMinutes
            Me.Seconds = nTC.bSeconds
            Me.Frames = nTC.bFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByVal nHours As Byte, ByVal nMinutes As Byte, ByVal nSeconds As Byte, ByVal nFrames As Byte, ByVal NTSC As Boolean)
            Me.New()
            Me.Hours = nHours
            Me.Minutes = nMinutes
            Me.Seconds = nSeconds
            Me.Frames = nFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByRef C_PBTM() As Byte, ByVal Offset As Long)
            Me.New()
            Try
                Dim h10, h1, m10, m1, s10, s1, f10, f1 As Byte

                h10 = C_PBTM(Offset) >> 4 And 15
                h1 = C_PBTM(Offset) And 15

                m10 = C_PBTM(Offset + 1) >> 4 And 15
                m1 = C_PBTM(Offset + 1) And 15

                s10 = C_PBTM(Offset + 2) >> 4 And 15
                s1 = C_PBTM(Offset + 2) And 15

                f10 = C_PBTM(Offset + 3) >> 4 And 3
                f1 = C_PBTM(Offset + 3) And 15


                Hours = (h10 * 10) + h1
                Minutes = (m10 * 10) + m1
                Seconds = (s10 * 10) + s1
                Frames = (f10 * 10) + f1

                Select Case (C_PBTM(Offset + 3) >> 6) And 3
                    Case 3
                        Framerate = eFramerate.NTSC
                    Case 2
                        Framerate = Nothing
                    Case 1
                        Framerate = eFramerate.PAL
                    Case 0
                        Framerate = Nothing
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with New cTimecode_MRJ from C_PBTM. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub New(ByVal GrossSeconds As UInt32, ByVal nFramerate As eFramerate)
            Me.New(CLng(GrossSeconds * nFramerate), nFramerate)
        End Sub

        Public Sub New(ByVal nFrames As Long, ByVal nFramerate As eFramerate)
            Me.New()
            Dim res As Long = 0
            Me.Hours = Math.DivRem(nFrames, nFramerate * 3600, res)
            nFrames -= (nFramerate * 3600) * Hours
            Me.Minutes = Math.DivRem(nFrames, (nFramerate * 60), res)
            nFrames -= (nFramerate * 60) * Minutes
            Me.Seconds = Math.DivRem(nFrames, nFramerate, res)
            nFrames -= nFramerate * Seconds
            Me.Framerate = nFramerate

            If nFrames = nFramerate Then
                nFrames = 0
                Seconds += 1
            End If

            If Seconds = 60 Then
                Seconds = 0
                Minutes += 1
            End If

            If Minutes = 60 Then
                Minutes = 0
                Hours += 1
            End If

            Me.Frames = nFrames

        End Sub

        Public Sub New(ByVal REFERENCE_TIME As ULong, ByVal ATPF As UInt32, ByVal nFrameRate As eFramerate)
            Me.New(CLng(REFERENCE_TIME / ATPF), nFrameRate)
        End Sub

#End Region 'cTimecode_MRJ Public Constructors

#Region "cTimecode_MRJ Shared Members & Methods"

        'Private Shared Members
        ''' <summary>
        ''' [Private, Shared] Default format string to use for all new NTSC timecode instances.  Modified with <i>SetDefaultNTSCDelimiters</i>.
        ''' </summary>
        Private Shared __DefaultNTSCDelimiters As String = Consts.NTSC_DELIMITERS
        ''' <summary>
        ''' [Private, Shared] Default format string to use for all new PAL timecode instances.  Modified with <i>SetDefaultPALDelimiters</i>.
        ''' </summary>
        Private Shared __DefaultPALDelimiters As String = Consts.PAL_DELIMITERS
        ''' <summary>
        ''' [Private, Shared] Default format string to use for all new Film timecode instances.  Modified with <i>SetDefaultFilmDelimiters</i>.
        ''' </summary>
        Private Shared __DefaultFilmDelimiters As String = Consts.FILM_DELIMITERS

        'Public Shared Methods
        ''' <summary>
        ''' [Shared] Compares two timecode values (in string format), returning <i>-1</i>, <i>0</i>, or
        ''' <i>1</i> if the first value is less than, equal to, or greater than the second
        ''' value, respectively.
        ''' </summary>
        ''' <param name="time1">First timecode parameter in string format</param>
        ''' <param name="time2">Second timecode parameter in string format</param>
        ''' <returns><i>-1</i>, <i>0</i>, or <i>1</i> if time1 is &lt;, =, or &gt; time2, respectively</returns>
        ''' <remarks>Use to determine if one timecode value is less than, equal to, or greater than another.</remarks>
        Public Shared Function CompareTimes(ByVal time1 As String, ByVal time2 As String) As Integer
            Dim tc1 As cTimecode_MRJ
            Dim tc2 As cTimecode_MRJ
            Dim result As Integer

            tc1 = New cTimecode_MRJ(eTimecodeFormat.NTSC, False, time1)
            tc2 = New cTimecode_MRJ(eTimecodeFormat.NTSC, False, time2)
            If tc1.AbsFrames < tc2.AbsFrames Then
                result = -1
            ElseIf tc1.AbsFrames > tc2.AbsFrames Then
                result = 1
            Else
                result = 0
            End If
            Return result
        End Function

        ''' <summary>
        ''' [Shared] Set the default format string to use for new instances of cTimecode_MRJ using the NTSC format.
        ''' </summary>
        ''' <param name="formatStr">Format string to use (e.g. "%2.2u:%2.2u:%2.2u:%2.2u" for "00:00:00:00")</param>
        ''' <remarks>Use to set the default format string for NTSC timecodes.</remarks>
        Public Shared Sub SetDefaultNTSCDelimiters(ByVal formatStr As String)
            If formatStr IsNot Nothing Then
                __DefaultNTSCDelimiters = formatStr
            End If
        End Sub

        ''' <summary>
        ''' [Shared] Set the default format string to use for new instances of cTimecode_MRJ using the PAL format.
        ''' </summary>
        ''' <param name="formatStr">Format string to use (e.g. "%2.2u:%2.2u:%2.2u;%2.2u" for "00:00:00;00")</param>
        ''' <remarks>Use to set the default format string for PAL timecodes.</remarks>
        Public Shared Sub SetDefaultPALDelimiters(ByVal formatStr As String)
            If formatStr IsNot Nothing Then
                __DefaultPALDelimiters = formatStr
            End If
        End Sub

        ''' <summary>
        ''' [Shared] Set the default format string to use for new instances of cTimecode_MRJ using the Film format.
        ''' </summary>
        ''' <param name="formatStr">Format string to use (e.g. "%2.2u:%2.2u:%2.2u:%2.2u" for "00:00:00:00")</param>
        ''' <remarks>Use to set the default format string for Film timecodes.</remarks>
        Public Shared Sub SetDefaultFilmDelimiters(ByVal formatStr As String)
            If formatStr IsNot Nothing Then
                __DefaultFilmDelimiters = formatStr
            End If
        End Sub

        ''' <summary>
        ''' [Shared] Performs a spot test of the <i>cTimecode_MRJ</i> class to ensure proper operation.
        ''' Returns <i>True</i> if test succeeds.
        ''' </summary>
        ''' <returns><i>True</i> if test is passed.  <i>False</i> if test fails.</returns>
        ''' <remarks>Use to test the class for proper implementation.</remarks>
        Public Shared Function Test() As Boolean
            Dim tc1 As cTimecode_MRJ
            Dim tc2 As cTimecode_MRJ
            Dim result As Boolean

            result = False
            tc1 = New cTimecode_MRJ()
            tc2 = New cTimecode_MRJ(eTimecodeFormat.NTSC, True, "01:00:00:00")
            'Set delimiters
            tc1.SetDelimiters(eTimecodeFormat.NTSC, Consts.NTSC_DELIMITERS)
            tc2.SetDelimiters(eTimecodeFormat.NTSC, Consts.NTSC_DELIMITERS)
            tc1.SetDelimiters(eTimecodeFormat.PAL, Consts.PAL_DELIMITERS)
            tc2.SetDelimiters(eTimecodeFormat.PAL, Consts.PAL_DELIMITERS)
            tc1.SetDelimiters(eTimecodeFormat.Film, Consts.FILM_DELIMITERS)
            tc2.SetDelimiters(eTimecodeFormat.Film, Consts.FILM_DELIMITERS)
            'Check defaults
            result = (tc1.TCFormat = eTimecodeFormat.NTSC)
            result = result And Not tc1.DropFrame
            result = result And (tc1.AbsFrames = 0)
            result = result And (tc2.TCFormat = eTimecodeFormat.NTSC)
            result = result And tc2.DropFrame
            result = result And (tc2.AbsFrames = 107892)
            result = result And (tc2.Timecode = "01:00:00:00")
            'NTSC Non-Drop Frame tests
            tc1.Timecode = "01:02:36:29"
            result = result And (tc1.AbsFrames = 112709)
            result = result And (tc1.Hours = 1)
            result = result And (tc1.Minutes = 2)
            result = result And (tc1.Seconds = 36)
            result = result And (tc1.Frames = 29)
            result = result And (tc1.Timecode = "01:02:36:29")
            'NTSC Drop Frame tests
            tc1.DropFrame = True
            tc1.Timecode = "01:02:36:29"
            result = result And tc1.DropFrame
            result = result And (tc1.AbsFrames = 112597)
            result = result And (tc1.Hours = 1)
            result = result And (tc1.Minutes = 2)
            result = result And (tc1.Seconds = 36)
            result = result And (tc1.Frames = 29)
            result = result And (tc1.Timecode = "01:02:36:29")
            'PAL Non-Drop Frame tests
            tc1.TCFormat = eTimecodeFormat.PAL
            tc1.Timecode = "01:02:36;24"
            result = result And Not tc1.DropFrame
            result = result And (tc1.AbsFrames = 93924)
            result = result And (tc1.Hours = 1)
            result = result And (tc1.Minutes = 2)
            result = result And (tc1.Seconds = 36)
            result = result And (tc1.Frames = 24)
            result = result And (tc1.Timecode = "01:02:36;24")
            'Film Non-Drop Frame tests
            tc1.TCFormat = eTimecodeFormat.Film
            tc1.Timecode = "01:02:36:23"
            result = result And Not tc1.DropFrame
            result = result And (tc1.AbsFrames = 90167)
            result = result And (tc1.Hours = 1)
            result = result And (tc1.Minutes = 2)
            result = result And (tc1.Seconds = 36)
            result = result And (tc1.Frames = 23)
            result = result And (tc1.Timecode = "01:02:36:23")
            'PAL->NTSC-NDF Conversion tests
            tc1.TCFormat = eTimecodeFormat.PAL
            tc1.Timecode = "01:02:36;24"    '93,924 PAL frames
            tc1.ConvertToNTSC(False)        'specify Non-Drop Frame
            result = result And (tc1.TCFormat = eTimecodeFormat.NTSC)
            result = result And Not tc1.DropFrame
            result = result And (tc1.AbsFrames = 112596)
            result = result And (tc1.Timecode = "01:02:33:06")  '* See Note
            '* Note: Remember that NTSC-NDF does not accurately reflect elapsed time, PAL does.
            'PAL->NTSC-DF Conversion tests
            tc1.TCFormat = eTimecodeFormat.PAL
            tc1.Timecode = "01:02:36;24"    '93,924 PAL frames
            tc1.ConvertToNTSC(True)         'specify Drop Frame
            result = result And (tc1.TCFormat = eTimecodeFormat.NTSC)
            result = result And tc1.DropFrame
            result = result And (tc1.AbsFrames = 112596)
            result = result And (tc1.Timecode = "01:02:36:28")  '* See Note
            '* Note: Also remember that NTSC-DF is only exactly accurate every 10 minutes.
            'Duration tests
            tc2.SetToDuration(tc1, tc2)
            result = result And Not tc2.DropFrame       'durations are never Drop Frame
            result = result And (tc2.AbsFrames = 4704)
            result = result And (tc2.Timecode = "00:02:36:24")  'Non-Drop Frame
            'Addition test
            tc2.Add(tc1)
            result = result And (tc2.AbsFrames = 117300)
            result = result And (tc2.Timecode = "01:05:10:00")  'Non-Drop Frame
            'Subtraction test
            tc2.Subtract(tc1)
            result = result And (tc2.AbsFrames = 4704)
            result = result And (tc2.Timecode = "00:02:36:24")  'Non-Drop Frame

            'Return the test results
            Return result
        End Function

#End Region 'cTimecode Shared Members & Methods

#Region "cTimecode Private Members"

        'Private Members
        ''' <summary>
        ''' [Private] Current timecode format (NTSC, PAL or Film) of this object.
        ''' </summary>
        Private _TCFormat As eTimecodeFormat
        ''' <summary>
        ''' [Private] Indicates if this timecode object is using the Drop Frame mode (always <i>False</i> if not NTSC format).
        ''' </summary>
        ''' <remarks></remarks>
        Private _DropFrame As Boolean
        ''' <summary>
        ''' [Private] Absolute number of frames represented by this timecode object.  Internal data representation of the timecode object.
        ''' </summary>
        Private _AbsFrames As Integer
        ''' <summary>
        ''' [Private] Format string delimiters (one per timecode format) to use with this particular timecode object.  (Set in constructor.)
        ''' </summary>
        Private _Delimiters() As String

#End Region 'cTimecode Private Members

#Region "cTimecode Public Properties"

        Public Framerate As eFramerate

        ''' <summary>
        ''' Sets or returns the current timecode format (NTSC, PAL or Film).
        ''' </summary>
        ''' <value>Current timecode format (NTSC, PAL or Film)</value>
        ''' <returns><i>eTimecodeFormat</i></returns>
        ''' <remarks>Use to set/read timecode format (NTSC, PAL or Film).</remarks>
        <XmlIgnoreAttribute()> _
        Public Property TCFormat() As eTimecodeFormat
            Get
                Return _TCFormat
            End Get
            Set(ByVal value As eTimecodeFormat)
                If _TCFormat <> value Then
                    _TCFormat = value
                    If Not (_TCFormat = eTimecodeFormat.NTSC) Then
                        _DropFrame = False
                    End If
                End If
            End Set
        End Property

        ''' <summary>
        ''' For NTSC timecode, indicates if timecode is in Drop Frame format.
        ''' </summary>
        ''' <value>Indicates if timecode is in Drop Frame format.</value>
        ''' <returns><i>True</i> if format is NTSC Drop Frame.  <i>False</i> otherwise.</returns>
        ''' <remarks>Use to indicate Drop Frame format (ignored if timecode format is not NTSC).</remarks>
        <XmlIgnoreAttribute()> _
        Public Property DropFrame() As Boolean
            Get
                Return _DropFrame
            End Get
            Set(ByVal value As Boolean)
                If (_DropFrame <> value) And (_TCFormat = eTimecodeFormat.NTSC) Then
                    _DropFrame = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns a string representation of the current timecode value using the 
        ''' current formatting delimiters.  (When set, assumes an HH:MM:SS:FF configuration, 
        ''' ignoring the delimiters in between the values.)
        ''' </summary>
        ''' <value>String representation of the current timecode value.</value>
        ''' <returns>String representation of the current timecode value.</returns>
        ''' <exception cref="InvalidTimecodeException">Thrown when <i>value</i> is invalid</exception>
        ''' <remarks>Use to set/read timecode using a string representation.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property Timecode() As String
            Get
                Return Me.ToString
            End Get
            Set(ByVal value As String)
                Dim hh, mm, ss, ff As Integer

                Try
                    hh = CInt(value.Substring(0, 2))    'Hours
                    mm = CInt(value.Substring(3, 2))    'Minutes
                    ss = CInt(value.Substring(6, 2))    'Seconds
                    ff = CInt(value.Substring(9, 2))    'Frames
                    SetTimeCodes(hh, mm, ss, ff)
                Catch ex As ArgumentOutOfRangeException
                    Throw New InvalidTimecodeException(My.Resources.InvalidTimecodeExceptionMsg)
                End Try
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns the Hours portion of the current timecode value.
        ''' </summary>
        ''' <value>Hours portion of the current timecode value.</value>
        ''' <returns>Hours portion of the current timecode value.</returns>
        ''' <remarks>Use to set/read the Hours portion of the current timecode value.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property Hours() As Integer
            Get
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                Return hh
            End Get
            Set(ByVal value As Integer)
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                If value <> hh Then
                    SetTimeCodes(value, mm, ss, ff)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns the Minutes portion of the current timecode value.
        ''' </summary>
        ''' <value>Minutes portion of the current timecode value.</value>
        ''' <returns>Minutes portion of the current timecode value.</returns>
        ''' <remarks>Use to set/read the Minutes portion of the current timecode value.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property Minutes() As Integer
            Get
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                Return mm
            End Get
            Set(ByVal value As Integer)
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                If value <> mm Then
                    SetTimeCodes(hh, value, ss, ff)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns the Seconds portion of the current timecode value.
        ''' </summary>
        ''' <value>Seconds portion of the current timecode value.</value>
        ''' <returns>Seconds portion of the current timecode value.</returns>
        ''' <remarks>Use to set/read the Seconds portion of the current timecode value.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property Seconds() As Integer
            Get
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                Return ss
            End Get
            Set(ByVal value As Integer)
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                If value <> ss Then
                    SetTimeCodes(hh, mm, value, ff)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns the Frames portion of the current timecode value.
        ''' </summary>
        ''' <value>Frames portion of the current timecode value.</value>
        ''' <returns>Frames portion of the current timecode value.</returns>
        ''' <remarks>Use to set/read the Frames portion of the current timecode value.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property Frames() As Integer
            Get
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                Return ff
            End Get
            Set(ByVal value As Integer)
                Dim hh, mm, ss, ff As Integer

                GetTimeCodes(hh, mm, ss, ff)
                If value <> ff Then
                    SetTimeCodes(hh, mm, ss, value)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Sets or returns the absolute number of frames represented by the current timecode value.
        ''' </summary>
        ''' <value>Absolute number of frames represented by the current timecode value.</value>
        ''' <returns>Absolute number of frames represented by the current timecode value.</returns>
        ''' <remarks>Use to set/read the absolute number of frames represented by the current timecode value.</remarks>
        <XmlIgnoreAttribute()> _
        Public Property AbsFrames() As Integer
            Get
                Return _AbsFrames
            End Get
            Set(ByVal value As Integer)
                _AbsFrames = value
            End Set
        End Property

        Public ReadOnly Property TotalSeconds() As UInt64
            Get
                Dim out As UInt64 = Hours * 3600
                out += Minutes * 60
                out += Seconds
                Return out
            End Get
        End Property

        Public ReadOnly Property DVDTimeCode() As DvdTimeCode
            Get
                Try
                    Dim out As New DvdTimeCode
                    out.bFrames = Me.Frames
                    out.bHours = Me.Hours
                    out.bMinutes = Me.Minutes
                    out.bSeconds = Me.Seconds
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with ToDVDTimeCode. Error: " & ex.Message)
                End Try
            End Get
        End Property

#End Region 'cTimecode Public Properties

#Region "cTimecode Private Methods"

        ''' <summary>
        ''' [Private] Calculates and returns the hours, minutes, seconds and frames of a timecode value
        ''' in NTSC-DF format given the absolute number of frames.
        ''' </summary>
        ''' <param name="frames">Absolute number of frames (in)</param>
        ''' <param name="hh">Hours (out)</param>
        ''' <param name="mm">Minutes (out)</param>
        ''' <param name="ss">Seconds (out)</param>
        ''' <param name="ff">Frames (out)</param>
        Private Sub GetNTSCDropFrame(ByVal frames As Integer, ByRef hh As Integer, ByRef mm As Integer, ByRef ss As Integer, ByRef ff As Integer)
            Dim n As Integer

            n = frames                  'n keeps a running count of frames
            'Extract even blocks of 10 minutes
            mm = (n \ 17982) * 10       '17,982 is exactly 00:10:00:00
            hh = mm \ 60
            mm = mm Mod 60
            n = n Mod 17982             'remove the 10 min. blocks of frames
            'Account for first minute
            If (n >= 1800) Then         '1,800 frames in first minute, 1,798 in subsequent ones
                'Account for first minute + drop frames
                mm = mm + 1
                ff = 2
                n = n - 1800
                'Account for remaining minutes
                mm = mm + (n \ 1798)    '1,798 frames per minute
                n = n Mod 1798          'remove 1 min. blocks of frames
            Else
                ff = 0
            End If
            'Calculate seconds & frames
            ss = n \ 30                 '30 frames per second
            n = n Mod 30                'remove 1 sec. blocks of frames
            ff = ff + n                 'remaining frames are on their own
            'Adjust for overflow
            If ff >= 30 Then
                ss = ss + 1             'carry over to next second
                ff = ff - 30            'remove 1 sec. block of frames
            End If
        End Sub

        ''' <summary>
        ''' [Private] Calculates and returns the hours, minutes, seconds and frames of a timecode value
        ''' in NTSC-NDF format given the absolute number of frames.
        ''' </summary>
        ''' <param name="frames">Absolute number of frames (in)</param>
        ''' <param name="hh">Hours (out)</param>
        ''' <param name="mm">Minutes (out)</param>
        ''' <param name="ss">Seconds (out)</param>
        ''' <param name="ff">Frames (out)</param>
        Private Sub GetNTSCNonDropFrame(ByVal frames As Integer, ByRef hh As Integer, ByRef mm As Integer, ByRef ss As Integer, ByRef ff As Integer)
            hh = frames \ 108000                '108,000 = 30 fps * 60 s/min * 60 min/hr
            mm = (frames Mod 108000) \ 1800     '1,800 = 30 fps * 60 s/min
            ss = (frames Mod 1800) \ 30         '30 = 30 fps
            ff = frames Mod 30
        End Sub

        ''' <summary>
        ''' [Private] Calculates and returns the hours, minutes, seconds and frames of a timecode value
        ''' in PAL format given the absolute number of frames.
        ''' </summary>
        ''' <param name="frames">Absolute number of frames (in)</param>
        ''' <param name="hh">Hours (out)</param>
        ''' <param name="mm">Minutes (out)</param>
        ''' <param name="ss">Seconds (out)</param>
        ''' <param name="ff">Frames (out)</param>
        Private Sub GetPALNonDropFrame(ByVal frames As Integer, ByRef hh As Integer, ByRef mm As Integer, ByRef ss As Integer, ByRef ff As Integer)
            hh = frames \ 90000                 '90,000 = 25 fps * 60 s/min * 60 min/hr
            mm = (frames Mod 90000) \ 1500      '1,500 = 25 fps * 60 s/min
            ss = (frames Mod 1500) \ 25         '25 = 25 fps
            ff = frames Mod 25
        End Sub

        ''' <summary>
        ''' [Private] Calculates and returns the hours, minutes, seconds and frames of a timecode value
        ''' in Film format given the absolute number of frames.
        ''' </summary>
        ''' <param name="frames">Absolute number of frames (in)</param>
        ''' <param name="hh">Hours (out)</param>
        ''' <param name="mm">Minutes (out)</param>
        ''' <param name="ss">Seconds (out)</param>
        ''' <param name="ff">Frames (out)</param>
        Private Sub GetFilmNonDropFrame(ByVal frames As Integer, ByRef hh As Integer, ByRef mm As Integer, ByRef ss As Integer, ByRef ff As Integer)
            hh = frames \ 86400                 '86,400 = 24 fps * 60 s/min * 60 min/hr
            mm = (frames Mod 86400) \ 1440      '1,440 = 24 fps * 60 s/min
            ss = (frames Mod 1440) \ 24         '24 = 24 fps
            ff = frames Mod 24
        End Sub

        ''' <summary>
        ''' [Private] Sets the current timecode to NTSC-DF format with the given value.
        ''' </summary>
        ''' <param name="hh">Hours</param>
        ''' <param name="mm">Minutes</param>
        ''' <param name="ss">Seconds</param>
        ''' <param name="ff">Frames</param>
        Private Sub SetNTSCDropFrame(ByVal hh As Integer, ByVal mm As Integer, ByVal ss As Integer, Optional ByVal ff As Integer = 0)
            Dim dropped As Integer
            Dim valid As Boolean

            valid = (hh >= 0) And (hh < 24) And (mm >= 0) And (mm < 60) And _
                    (ss >= 0) And (ss < 60) And (ff >= 0) And (ff < 30)
            'Check for 'dropped frames' -- not valid if set to one
            If (ss = 0) And ((mm Mod 10) <> 0) Then
                valid = valid And (ff <> 0) And (ff <> 1)
            End If
            'If valid, continue
            If valid Then
                _AbsFrames = (hh * 108000) + (mm * 1800) + (ss * 30) + ff
                'Account for dropped frames
                dropped = 2 * ((hh * 60 + mm) - ((hh * 60 + mm) \ 10))
                _AbsFrames = _AbsFrames - dropped
            Else
                Throw New InvalidTimecodeException(My.Resources.InvalidNTSCDFTimecodeExceptionMsg)
            End If
        End Sub

        ''' <summary>
        ''' [Private] Sets the current timecode to NTSC-NDF format with the given value.
        ''' </summary>
        ''' <param name="hh">Hours</param>
        ''' <param name="mm">Minutes</param>
        ''' <param name="ss">Seconds</param>
        ''' <param name="ff">Frames</param>
        Private Sub SetNTSCNonDropFrame(ByVal hh As Integer, ByVal mm As Integer, ByVal ss As Integer, Optional ByVal ff As Integer = 0)
            Dim valid As Boolean

            valid = (hh >= 0) And (hh < 24) And (mm >= 0) And (mm < 60) And _
                    (ss >= 0) And (ss < 60) And (ff >= 0) And (ff < 30)
            If valid Then
                _AbsFrames = (hh * 108000) + (mm * 1800) + (ss * 30) + ff
            Else
                Throw New InvalidTimecodeException(My.Resources.InvalidNTSCNDFTimecodeExceptionMsg)
            End If
        End Sub

        ''' <summary>
        ''' [Private] Sets the current timecode to PAL-NDF format with the given value.
        ''' </summary>
        ''' <param name="hh">Hours</param>
        ''' <param name="mm">Minutes</param>
        ''' <param name="ss">Seconds</param>
        ''' <param name="ff">Frames</param>
        Private Sub SetPALNonDropFrame(ByVal hh As Integer, ByVal mm As Integer, ByVal ss As Integer, Optional ByVal ff As Integer = 0)
            Dim valid As Boolean

            valid = (hh >= 0) And (hh < 24) And (mm >= 0) And (mm < 60) And _
                    (ss >= 0) And (ss < 60) And (ff >= 0) And (ff < 25)
            If valid Then
                _AbsFrames = (hh * 90000) + (mm * 1500) + (ss * 25) + ff
            Else
                Throw New InvalidTimecodeException(My.Resources.InvalidPALNDFTimecodeExceptionMsg)
            End If
        End Sub

        ''' <summary>
        ''' [Private] Sets the current timecode to Film-NDF format with the given value.
        ''' </summary>
        ''' <param name="hh">Hours</param>
        ''' <param name="mm">Minutes</param>
        ''' <param name="ss">Seconds</param>
        ''' <param name="ff">Frames</param>
        Private Sub SetFilmNonDropFrame(ByVal hh As Integer, ByVal mm As Integer, ByVal ss As Integer, Optional ByVal ff As Integer = 0)
            Dim valid As Boolean

            valid = (hh >= 0) And (hh < 24) And (mm >= 0) And (mm < 60) And _
                    (ss >= 0) And (ss < 60) And (ff >= 0) And (ff < 24)
            If valid Then
                _AbsFrames = (hh * 86400) + (mm * 1440) + (ss * 24) + ff
            Else
                Throw New InvalidTimecodeException(My.Resources.InvalidFilmNDFTimecodeExceptionMsg)
            End If
        End Sub

#End Region 'cTimecode Private Methods

#Region "cTimecode Public Methods"

        ''' <summary>
        ''' Add the specified timecode to the current timecode.
        ''' </summary>
        ''' <param name="aTimecode">Timecode to add</param>
        ''' <remarks>Use to add one timecode value to another.</remarks>
        ''' <exception cref="TCFormatMismatchException">Thrown if the format of the specified 
        ''' timecode object does not match the current object's format</exception>
        Public Sub Add(ByVal aTimecode As cTimecode_MRJ)
            If aTimecode IsNot Nothing Then
                If _TCFormat = aTimecode.TCFormat Then
                    _AbsFrames = _AbsFrames + aTimecode.AbsFrames
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Add the specified number of frames to the current timecode.
        ''' </summary>
        ''' <param name="frames">Number of frames to add</param>
        ''' <remarks>Use to add a number of frames to the current timecode.</remarks>
        Public Sub Add(ByVal frames As Integer)
            _AbsFrames += frames
        End Sub

        ''' <summary>
        ''' Assign the values of the specified timecode object to this one.
        ''' </summary>
        ''' <param name="aTimecode">Timecode object to copy</param>
        ''' <remarks>Use to assign the values of one timecode object to another.</remarks>
        Public Sub Assign(ByVal aTimecode As cTimecode_MRJ)
            Dim i As Integer

            If aTimecode IsNot Nothing Then
                _TCFormat = aTimecode.TCFormat
                _DropFrame = aTimecode.DropFrame
                _AbsFrames = aTimecode.AbsFrames
                'Copy all of the delimiter settings
                For i = 0 To UBound(_Delimiters)
                    _Delimiters(i) = aTimecode._Delimiters(i)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Sets the timecode value to zero without affecting other settings 
        ''' (e.g. TCFormat or DropFrame).
        ''' </summary>
        ''' <remarks>Use to set the timecode value to zero.</remarks>
        Public Sub Clear()
            _AbsFrames = 0
        End Sub

        ''' <summary>
        ''' Compares the instance to a specified cTimecode object and 
        ''' returns an indication of their relative values.
        ''' </summary>
        ''' <param name="aTimecode">A cTimecode object to compare to</param>
        ''' <returns>A signed number indicating the relative values of this 
        ''' instance and the value parameter.  (Less than zero indicates that 
        ''' this instance is less than the given object.  Zero indicates that 
        ''' the two are of equal value, and greater than zero indicates that 
        ''' this instance is greater than the given object.)</returns>
        ''' <exception cref="TCFormatMismatchException">Thrown if the given object 
        ''' is not the same timecode format as the instance.</exception>
        ''' <remarks>Compares the instance to another cTimecode object.  
        ''' Before comparing, be sure both objects are of the same timecode 
        ''' format.  If the given object is <i>Nothing</i> then a value of <i>1</i>
        ''' is returned.</remarks>
        Public Function CompareTo(ByVal aTimecode As cTimecode_MRJ) As Integer
            Dim result As Integer

            result = 1
            If aTimecode IsNot Nothing Then
                If aTimecode.TCFormat = Me.TCFormat Then
                    result = Me.AbsFrames - aTimecode.AbsFrames
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
            'Return the result
            Return result
        End Function

        ''' <summary>
        ''' Convert the current timecode to an alternate timecode format, maintaining a comparable 
        ''' time offset as accurately as possible.  (Note, not all timecode formats accurately 
        ''' represent real time -- e.g. NTSC-NDF.)
        ''' </summary>
        ''' <param name="aTCFormat">Timecode format to convert to</param>
        ''' <param name="df">Drop frame indicator (optional, ignored if not NTSC)</param>
        ''' <remarks>Use to convert current timecode to an alternate timecode format.</remarks>
        Public Sub ConvertTo(ByVal aTCFormat As eTimecodeFormat, Optional ByVal df As Boolean = False)
            Dim ratesNum() As Long = {30000, 25000, 24000}   'NTSC = 30,000/1,001 fps
            Dim ratesDen() As Long = {1001, 1000, 1001}    'Film = 24,000/1,001 fps
            Dim frames As Integer
            Dim remainder As Integer

            'Only recalculate if timecode format changes (changing DF flag does not need re-calc)
            If (_TCFormat <> aTCFormat) Then
                'Calculate frame rate conversion = (# frames * destination rate) / source rate
                frames = (_AbsFrames * ratesNum(aTCFormat) * ratesDen(_TCFormat)) \ _
                         (ratesDen(aTCFormat) * ratesNum(_TCFormat))
                'Round up if needed
                remainder = (_AbsFrames * ratesNum(aTCFormat) * ratesDen(_TCFormat)) Mod _
                            (ratesDen(aTCFormat) * ratesNum(_TCFormat))
                If remainder >= (ratesDen(aTCFormat) * ratesNum(_TCFormat)) \ 2 Then
                    frames += 1
                End If
                'Store new frames value
                _AbsFrames = frames
                _TCFormat = aTCFormat
            End If
            'Update Drop Frame flag for NTSC only (no re-calc needed)
            If aTCFormat = eTimecodeFormat.NTSC Then
                _DropFrame = df         'Set Drop Frame flag
            Else
                _DropFrame = False      'Clear Drop Frame flag
            End If
        End Sub

        ''' <summary>
        ''' Convert the current timecode to NTSC.  See <i>ConvertTo</i>.
        ''' </summary>
        ''' <param name="df">Drop Frame indicator</param>
        ''' <remarks>Use to convert current timecode to NTSC-DF.</remarks>
        Public Sub ConvertToNTSC(ByVal df As Boolean)
            ConvertTo(eTimecodeFormat.NTSC, df)
        End Sub

        ''' <summary>
        ''' Convert the current timecode to PAL.  See <i>ConvertTo</i>.
        ''' </summary>
        ''' <remarks>Use to convert current timecode to PAL.</remarks>
        Public Sub ConvertToPAL()
            ConvertTo(eTimecodeFormat.PAL)
        End Sub

        ''' <summary>
        ''' Convert the current timecode to Film.  See <i>ConvertTo</i>.
        ''' </summary>
        ''' <remarks>Use to convert current timecode to Film.</remarks>
        Public Sub ConvertToFilm()
            ConvertTo(eTimecodeFormat.Film)
        End Sub

        ''' <summary>
        ''' Create a new duplicate of the current object instance.
        ''' </summary>
        ''' <returns>Duplicate object instance</returns>
        ''' <remarks>Use to perform a deep copy of the current object 
        ''' and return the results in a new instance.</remarks>
        Public Function Duplicate() As cTimecode_MRJ
            Dim result As cTimecode_MRJ

            result = New cTimecode_MRJ()
            result.Assign(Me)
            Return result
        End Function

        ''' <summary>
        ''' Get the individual hour, minute, second and frame values for the current timecode.
        ''' </summary>
        ''' <param name="hh">Hours (out)</param>
        ''' <param name="mm">Minutes (out)</param>
        ''' <param name="ss">Seconds (out)</param>
        ''' <param name="ff">Frames (out)</param>
        ''' <remarks>Use to get the hours, minutes, seconds and frames of the current timecode.</remarks>
        Public Sub GetTimeCodes(ByRef hh As Integer, ByRef mm As Integer, ByRef ss As Integer, ByRef ff As Integer)
            Select Case _TCFormat
                Case eTimecodeFormat.NTSC
                    If _DropFrame Then
                        GetNTSCDropFrame(_AbsFrames, hh, mm, ss, ff)
                    Else
                        GetNTSCNonDropFrame(_AbsFrames, hh, mm, ss, ff)
                    End If
                Case eTimecodeFormat.PAL
                    GetPALNonDropFrame(_AbsFrames, hh, mm, ss, ff)
                Case eTimecodeFormat.Film
                    GetFilmNonDropFrame(_AbsFrames, hh, mm, ss, ff)
            End Select
        End Sub

        ''' <summary>
        ''' Determines if the instance falls within the time period starting 
        ''' at the given timecode and lasting for the given duration.
        ''' </summary>
        ''' <param name="aTimecode">Start timecode to which to compare</param>
        ''' <param name="aDuration">Duration timecode to which to compare</param>
        ''' <returns><i>True</i> if the instance falls within the period described</returns>
        ''' <exception cref="TCFormatMismatchException">Thrown if the given objects 
        ''' are not the same timecode format as the instance.</exception>
        ''' <remarks>Determines if the instance falls within the time period 
        ''' described, indicating <i>False</i> if either <i>aTimecode</i> 
        ''' or <i>aDuration</i> are Nothing.</remarks>
        Public Function IsWithin(ByVal aTimecode As cTimecode_MRJ, ByVal aDuration As cTimecode_MRJ) As Boolean
            Dim result As Boolean

            result = False
            If (aTimecode IsNot Nothing) And (aDuration IsNot Nothing) Then
                If (aTimecode.TCFormat = Me.TCFormat) And (aDuration.TCFormat = Me.TCFormat) Then
                    result = (Me.AbsFrames >= aTimecode.AbsFrames) _
                              And (Me.AbsFrames < aTimecode.AbsFrames + aDuration.AbsFrames)
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
            'Return the result
            Return result
        End Function

        ''' <summary>
        ''' Determines if the instance falls between the two specified timecodes.
        ''' </summary>
        ''' <param name="time1">First timecode to which to compare</param>
        ''' <param name="time2">Second timecode to which to compare</param>
        ''' <returns><i>True</i> if the instance falls within the period described, inclusively</returns>
        ''' <exception cref="TCFormatMismatchException">Thrown if the given objects 
        ''' are not the same timecode format as the instance.</exception>
        ''' <remarks>Determines if the instance falls within the time period 
        ''' described by the two timecodes inclusively, indicating <i>False</i> 
        ''' if either <i>aTimecode</i> or <i>aDuration</i> 
        ''' are Nothing.</remarks>
        Public Function IsBetween(ByVal time1 As cTimecode_MRJ, ByVal time2 As cTimecode_MRJ) As Boolean
            Dim result As Boolean

            result = False
            If (time1 IsNot Nothing) And (time2 IsNot Nothing) Then
                If (time1.TCFormat = Me.TCFormat) And (time2.TCFormat = Me.TCFormat) Then
                    If time1.CompareTo(time2) <= 0 Then
                        result = (Me.AbsFrames >= time1.AbsFrames) _
                                And (Me.AbsFrames <= time2.AbsFrames)
                    Else
                        result = (Me.AbsFrames >= time2.AbsFrames) _
                                And (Me.AbsFrames <= time1.AbsFrames)
                    End If
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
            'Return the result
            Return result
        End Function

        ''' <summary>
        ''' Resets the values of this timecode object, including format delimiters (NTSC-NDF 00:00:00:00).
        ''' </summary>
        ''' <remarks>Use to reset the values of this timecode object.</remarks>
        Public Sub Reset()
            _AbsFrames = 0
            _DropFrame = False
            _TCFormat = eTimecodeFormat.NTSC
            'Initialize _Delimiters format strings
            ReDim _Delimiters(eTimecodeFormat.Film)
            _Delimiters(eTimecodeFormat.NTSC) = __DefaultNTSCDelimiters
            _Delimiters(eTimecodeFormat.PAL) = __DefaultPALDelimiters
            _Delimiters(eTimecodeFormat.Film) = __DefaultFilmDelimiters
        End Sub

        ''' <summary>
        ''' Set the delimiter characters to use after the Hours, Minutes and Seconds values when 
        ''' converting the specified timecode format to a string representation.
        ''' </summary>
        ''' <param name="aTCFormat">The timecode format to specify delimiters for</param>
        ''' <param name="formatStr">A string of 3 characters representing the 3 different delimiters
        ''' (e.g. ":;." would give "01:02;03.04")</param>
        ''' <remarks>Use to set the delimiter characters for the specified timecode format.</remarks>
        Public Sub SetDelimiters(ByVal aTCFormat As eTimecodeFormat, ByVal formatStr As String)
            _Delimiters(aTCFormat) = formatStr
        End Sub

        ''' <summary>
        ''' Sets the current timecode to the given value based on this object's current timecode format (NTSC-DF/NDF, PAL or Film) .
        ''' </summary>
        ''' <param name="hh">Hours</param>
        ''' <param name="mm">Minutes</param>
        ''' <param name="ss">Seconds</param>
        ''' <param name="ff">Frames (<i>0</i> if none specified)</param>
        ''' <remarks>Use to set the timecode using Hours, Minutes, Seconds and (optionally) Frames</remarks>
        Public Sub SetTimeCodes(ByVal hh As Integer, ByVal mm As Integer, ByVal ss As Integer, Optional ByVal ff As Integer = 0)
            Select Case _TCFormat
                Case eTimecodeFormat.NTSC
                    If _DropFrame Then
                        SetNTSCDropFrame(hh, mm, ss, ff)
                    Else
                        SetNTSCNonDropFrame(hh, mm, ss, ff)
                    End If
                Case eTimecodeFormat.PAL
                    SetPALNonDropFrame(hh, mm, ss, ff)
                Case eTimecodeFormat.Film
                    SetFilmNonDropFrame(hh, mm, ss, ff)
            End Select
        End Sub

        ''' <summary>
        ''' Set the timecode of the current timecode object to the absolute duration between
        ''' the two specified timecode objects.  Order of the two specified timecodes does 
        ''' not matter.  Current timecode object will have matching timecode format and 
        ''' <i>DropFrame</i> will be <i>False</i>.
        ''' </summary>
        ''' <param name="time1">First timecode</param>
        ''' <param name="time2">Second timecode</param>
        ''' <remarks>Use to set current timecode to the duration between two specified timecodes.</remarks>
        ''' <exception cref="TCFormatMismatchException">Thrown if the timecode formats of the two specified timecodes do not match.</exception>
        Public Sub SetToDuration(ByVal time1 As cTimecode_MRJ, ByVal time2 As cTimecode_MRJ)
            If (time1 IsNot Nothing) And (time2 IsNot Nothing) Then
                If time1.TCFormat = time2.TCFormat Then
                    _TCFormat = time1.TCFormat  'Apply matching settings
                    _DropFrame = False          'Drop frame does not apply to durations
                    _AbsFrames = Math.Abs(time2.AbsFrames - time1.AbsFrames)
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Subtract the specified timecode from the current timecode value.  (May result in a 
        ''' negative value.)
        ''' </summary>
        ''' <param name="aTimecode">Timecode to subtract</param>
        ''' <remarks>Use to subtract one timecode value from another.</remarks>
        ''' <exception cref="TCFormatMismatchException">Thrown if specified timecode's format 
        ''' does not match</exception>
        Public Sub Subtract(ByVal aTimecode As cTimecode_MRJ)
            If aTimecode IsNot Nothing Then
                If _TCFormat = aTimecode.TCFormat Then
                    _AbsFrames = _AbsFrames - aTimecode.AbsFrames
                Else
                    Throw New TCFormatMismatchException(My.Resources.TCFormatMismatchExceptionMsg)
                End If
            End If
        End Sub

        ''' <summary>
        ''' Subtract the specified number of frames from the current timecode value.  
        ''' (May result in a negative value.)
        ''' </summary>
        ''' <param name="frames">Number of frames to subtract</param>
        ''' <remarks>Use to subtract frames from the current timecode value.</remarks>
        Public Sub Subtract(ByVal frames As Integer)
            _AbsFrames = _AbsFrames - frames
        End Sub

        ''' <summary>
        ''' Returns the string representation of the current timecode value.
        ''' </summary>
        ''' <returns>String representation of timecode value.</returns>
        ''' <remarks>Use to get the string representation of the current timecode.</remarks>
        Public Overrides Function ToString() As String
            Dim hh, mm, ss, ff As Integer
            Dim formatStr As String
            Dim result As String

            GetTimeCodes(hh, mm, ss, ff)
            formatStr = _Delimiters(_TCFormat)
            result = hh.ToString.PadLeft(2, "0") & formatStr.Substring(0, 1) & _
                     mm.ToString.PadLeft(2, "0") & formatStr.Substring(1, 1) & _
                     ss.ToString.PadLeft(2, "0") & formatStr.Substring(2, 1) & _
                     ff.ToString.PadLeft(2, "0")
            'Return the resulting string
            Return result
        End Function

        Public Function ToString_NoFrames() As String
            Dim oH, oM, oS, [oF] As String
            oH = Hours
            oM = Minutes
            oS = Seconds
            If oM.Length = 1 Then oM = "0" & oM
            If oS.Length = 1 Then oS = "0" & oS
            Return oH & ":" & oM & ":" & oS
        End Function

        Public Function ToString_WithFrameRate()
            If Not Framerate = Nothing Then
                Return ToString() & " / " & Framerate.ToString
            Else
                Return ToString()
            End If
        End Function

#End Region 'cTimecode Public Methods

    End Class   'cTimecode_MRJ

End Namespace   'Timecode Enums
