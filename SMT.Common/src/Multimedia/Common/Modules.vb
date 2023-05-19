
Namespace Multimedia.Modules

    Public Module Shuttling

        Public Const OneX As Double = 1.0
        Public Const TwoX As Double = 2.0
        Public Const FourX As Double = 4.1
        Public Const EightX As Double = 8.0
        Public Const SixteenX As Double = 16.0
        Public Const ThirtyTwoX As Double = 32.0

        Public Function GetNewSpeed(ByVal CurrentSpeed As Double, ByVal Faster As Boolean) As Double
            Try
                Select Case CurrentSpeed
                    Case OneX
                        If Faster Then
                            Return TwoX
                        Else
                            Return OneX
                        End If
                    Case TwoX
                        If Faster Then
                            Return FourX
                        Else
                            Return OneX
                        End If
                    Case FourX
                        If Faster Then
                            Return EightX
                        Else
                            Return TwoX
                        End If
                    Case EightX
                        If Faster Then
                            Return SixteenX
                        Else
                            Return FourX
                        End If
                    Case SixteenX
                        If Faster Then
                            Return ThirtyTwoX
                        Else
                            Return EightX
                        End If
                    Case ThirtyTwoX
                        If Faster Then
                            Return ThirtyTwoX
                        Else
                            Return SixteenX
                        End If
                    Case Else
                        Return OneX
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetNewSpeed(). Error: " & ex.Message)
            End Try
        End Function

    End Module


End Namespace
