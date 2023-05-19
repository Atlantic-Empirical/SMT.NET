
Namespace Multimedia.Utility.Timecode

    ''' <summary>
    ''' Constant values used by the cTimecode class.
    ''' </summary>
    Public Module Consts

        ''' <summary>
        ''' [Shared, Read Only] Pattern of delimiters used to separate numeric values in NTSC timecode values.
        ''' (E.g. ":::" corresponds to values such as 00:00:00:00)
        ''' </summary>
        Public ReadOnly NTSC_DELIMITERS As String = ":::"  '00:00:00:00

        ''' <summary>
        ''' [Shared, Read Only] Pattern of delimiters used to separate numeric values in PAL timecode values.
        ''' (E.g. "::;" corresponds to values such as 00:00:00;00)
        ''' </summary>
        Public ReadOnly PAL_DELIMITERS As String = "::;"   '00:00:00;00 <- uses semicolon for frames (sometimes done for NTSC-DF)

        ''' <summary>
        ''' [Shared, Read Only] Pattern of delimiters used to separate numeric values in Film timecode values.
        ''' (E.g. ":::" corresponds to values such as 00:00:00:00)
        ''' </summary>
        Public ReadOnly FILM_DELIMITERS As String = ":::"  '00:00:00:00 (same as NTSC)

    End Module

End Namespace

