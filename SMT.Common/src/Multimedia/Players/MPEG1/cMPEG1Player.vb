

Namespace Multimedia.Players.MPEG1

    Public Class cMPEG1Player
        'Inherits cBasePlayer

        Public Function BuildGraph() As Boolean
            'for now we're just going to try to use the M2V graph
            'If Not BuildGraph_SD_M2V() Then Return False
            Return True
        End Function

        '#Region "cBasePlayer Overrides"

        '        Public Overrides Function Play() As Boolean

        '        End Function

        '        Public Overrides Function Pause() As Boolean

        '        End Function

        '        Public Overrides Function [Stop]() As Boolean

        '        End Function

        '        Public Overrides Function FastForward(ByVal Rate As Integer) As Boolean

        '        End Function

        '        Public Overrides Function Rewind(ByVal Rate As Integer) As Boolean

        '        End Function

        '        Public Overrides Function FrameStep(ByVal Rate As Integer) As Boolean

        '        End Function

        '        Public Overrides Function Timesearch() As Boolean

        '        End Function

        '        Public Overrides Function SetOSD() As Boolean

        '        End Function

        '        Public Overrides Function ClearOSD() As Boolean

        '        End Function

        '#End Region 'Overrides

    End Class

End Namespace
