
Namespace MCS3

    Public Class cMCS3Event

        Public Pressed As Boolean
        Public Control As eMCSControls
        Public Value As Short
        Public Message As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal Msg As String)
            Message = Msg
        End Sub

        Public Overrides Function ToString() As String
            Return Message
        End Function

        Public ReadOnly Property ValueAsShuttleSpeed() As Byte
            Get
                Select Case Value
                    Case 1
                        Return 1
                    Case 2 To 3
                        Return 2
                    Case 4 To 5
                        Return 4
                    Case 6 To 7
                        Return 8
                    Case 8 To 9
                        Return 16
                    Case 10 To 12
                        Return 32
                    Case Else
                        Return 255
                End Select
            End Get
        End Property

    End Class

    Public Enum eMCSControls
        F1
        F2
        F3
        F4
        F5
        F6
        W1
        W2
        W3
        W4
        W5
        W6
        W7
        Up
        Down
        Left
        Right
        FastForward
        Rewind
        [Stop]
        Play
        Record
        Shuttle_Forward
        Shuttle_Center
        Shuttle_Rewind
        Jog_Forward
        Jog_Backward
        Unknown
    End Enum

    Public Module MCS3_Shared

        Public Function IsEven(ByVal value As Long) As Boolean
            Return (value Mod 2) = 0
        End Function

    End Module

End Namespace

