Imports System.Drawing
Imports SMT.Multimedia.DirectShow
Imports System.Runtime.InteropServices
Imports SMT.Multimedia.Classes

Namespace Multimedia.Players.FrameSplitter

    Public MustInherit Class cBaseFrameSplitter
        Inherits cBasePlayer

        Public MustOverride Function SetFrameSplit(ByVal FSM As eFrameSplitMode) As Boolean 'ByVal AnchorX As Integer, ByVal AnchorY As Integer, ByVal R1_X As Integer, ByVal R1_Y As Integer, ByVal R2_X As Integer, ByVal R2_y As Integer
        Public MustOverride Function BuildFrameSplittingGraph(ByVal FileA As String, ByVal FileB As String, ByVal NotifyWindowHandle As Integer) As Boolean
        Public MustOverride Function SetDynamicSplitVals(ByVal A As Integer, ByVal B As Integer, ByVal C As Integer, ByVal D As Integer) As Boolean

        Public CurrentFrameSplittingMode As eFrameSplitMode

        Public Enum eFrameSplitMode
            Checker_A
            Checker_B
            FiftyFifty_A
            FiftyFifty_B
            RightSides
            LeftSides
            AllA
            AllB
        End Enum

        Public Function GetSourceProperties(ByVal PinOne As Boolean) As cSourceProperties
            Try
                Dim AMT As AM_MEDIA_TYPE
                If Graph.IKeyFS Is Nothing Then Return Nothing
                If PinOne Then
                    HR = Me.Graph.IKeyFS.get_InputOneMediaType(AMT)
                Else
                    HR = Me.Graph.IKeyFS.get_InputTwoMediaType(AMT)
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim VIH As VIDEOINFOHEADER = CType(Marshal.PtrToStructure(AMT.pbFormat, GetType(VIDEOINFOHEADER)), VIDEOINFOHEADER)
                Dim out As New cSourceProperties
                out.FrameRate = Math.Round(10000000 / VIH.AvgTimePerFrame, 2)
                out.Height = VIH.SrcRect.Bottom
                out.Width = VIH.SrcRect.Right
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetStreamAProperties(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

End Namespace
