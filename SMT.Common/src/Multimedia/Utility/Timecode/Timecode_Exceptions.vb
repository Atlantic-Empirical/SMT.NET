Namespace Multimedia.Utility.Timecode

    ''' <summary>
    ''' Inherits:  <i>System.Exception</i><br /><br />
    ''' Base class for all Timecode-related exceptions.
    ''' </summary>
    <Serializable()> _
    Public Class TimeCodeException
        Inherits System.Exception

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub

        Public Sub New( _
            ByVal info As System.Runtime.Serialization.SerializationInfo, _
            ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class

    ''' <summary>
    ''' Inherits:  <i>Javelin.Libraries.Common.Timecode.TimeCodeException</i><br /><br />
    ''' Exception that is thrown when their is a mismatch of timecode formats (e.g. NTSC vs. PAL) 
    ''' between two timecode objects.
    ''' </summary>
    <Serializable()> _
    Public Class TCFormatMismatchException
        Inherits TimeCodeException

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub

        Public Sub New( _
            ByVal info As System.Runtime.Serialization.SerializationInfo, _
            ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class

    ''' <summary>
    ''' Inherits:  <i>Javelin.Libraries.Common.Timecode.TimeCodeException</i><br /><br />
    ''' Exception that is thrown when the timecode specified is invalid.
    ''' </summary>
    <Serializable()> _
    Public Class InvalidTimecodeException
        Inherits TimeCodeException

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub

        Public Sub New( _
            ByVal info As System.Runtime.Serialization.SerializationInfo, _
            ByVal context As System.Runtime.Serialization.StreamingContext)
            MyBase.New(info, context)
        End Sub
    End Class

End Namespace   'Timecode

