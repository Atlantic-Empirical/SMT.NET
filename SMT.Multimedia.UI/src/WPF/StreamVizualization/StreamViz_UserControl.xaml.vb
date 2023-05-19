#Region "IMPORTS"

Imports SMT.Multimedia.Players
Imports SMT.Multimedia.Players.M2TS
Imports SMT.Multimedia.Players.M2TS.cM2TSPlayer
Imports System.Windows
Imports System.Windows.Media
Imports System.Windows.Controls
Imports System.Windows.Shapes
Imports SMT.THX.MediaDirector.Formats.Shared.Elements
Imports SMT.THX.MediaDirector.Formats.Bluray.Streem.Project
Imports SMT.Multimedia.Classes

#End Region 'IMPORTS

Namespace UI.WPF.StreamVizualization

    Partial Public Class StreamViz_UserControl

#Region "PROPERTIES"

        'Public Property IsExpanded_Audio() As Boolean
        '    Get
        '        Return Me.exAudio.IsExpanded
        '    End Get
        '    Set(ByVal value As Boolean)
        '        Me.exAudio.IsExpanded = value
        '    End Set
        'End Property

        'Public Property IsExpanded_Video() As Boolean
        '    Get
        '        Return Me.exVideo.IsExpanded
        '    End Get
        '    Set(ByVal value As Boolean)
        '        Me.exVideo.IsExpanded = value
        '    End Set
        'End Property

        'Public Property IsExpanded_Graphics() As Boolean
        '    Get
        '        Return Me.exGraphics.IsExpanded
        '    End Get
        '    Set(ByVal value As Boolean)
        '        Me.exGraphics.IsExpanded = value
        '    End Set
        'End Property

        Public Property VizMode() As eVizualizerMode
            Get
                Return _VizMode
            End Get
            Set(ByVal value As eVizualizerMode)
                _VizMode = value
            End Set
        End Property
        Private _VizMode As eVizualizerMode

        Public ReadOnly Property VizExpandersHeight() As Integer
            Get
                'Return Me.exVideo.ActualHeight + Me.exAudio.ActualHeight + Me.exGraphics.ActualHeight + 9
            End Get
        End Property

        Public Property Player() As cBasePlayer
            Get
                Return _Player
            End Get
            Set(ByVal value As cBasePlayer)
                _Player = value
            End Set
        End Property
        Private _Player As cBasePlayer

#End Region 'PROPERTIES

#Region "EVENTS"

        Public Event evSelectStream(ByVal SI As cStreamInfo)
        Public Event evStreamLine_MouseRightButtonUp(ByVal sender As Object, ByVal p As System.Windows.Point)

#End Region 'EVENTS

#Region "FUNCTIONALITY"

        Public Sub ClearVizualizers()

            Me.spMain.Children.Clear()

            'spViz_vid.Children.Clear()
            'spViz_aud.Children.Clear()
            'spViz_grf.Children.Clear()
        End Sub

#Region "FUNCTIONALITY:CONSTRUCTION"

        Public Sub SetupVizualizer(ByVal nVizMode As eVizualizerMode, ByRef nPlayer As cBasePlayer)
            Try
                Player = nPlayer
                VizMode = nVizMode
                ClearVizualizers()
                Select Case Player.PlayerType
                    Case Multimedia.Enums.ePlayerType.M2TS
                        DrawVizualizer_M2TS()

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with SetupVizualizers(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Function CreateCanvas_ES(ByVal S As cStreamInfo, ByRef ParentElement As FrameworkElement) As Canvas
            Try
                Dim pn As New System.Windows.Controls.Canvas
                pn.Name = S.Station_Safe
                pn.Width = ParentElement.Width
                pn.Height = 15
                pn.Tag = S
                pn.Background = New SolidColorBrush(Windows.Media.Color.FromArgb(15, S.Color.R, S.Color.G, S.Color.B))
                AddHandler pn.MouseEnter, AddressOf HandleMouseEnter_StreamLine
                AddHandler pn.MouseLeave, AddressOf HandleMouseLeave_StreamLine
                AddHandler pn.MouseRightButtonUp, AddressOf HandleMouseRightButtonUp_StreamLine

                'Draw station label
                Dim lb As New System.Windows.Controls.Label
                lb.Content = S.Station
                lb.FontFamily = New System.Windows.Media.FontFamily("Arial")
                lb.FontSize = 12
                lb.FontWeight = FontWeights.Bold
                lb.Foreground = New SolidColorBrush(System.Windows.Media.Color.FromArgb(S.Color.A, S.Color.R, S.Color.G, S.Color.B))
                Canvas.SetLeft(lb, 3)
                Canvas.SetTop(lb, 0)
                pn.Children.Add(lb)

                'Draw format box
                Dim FormatBox As New System.Windows.Controls.Border
                FormatBox.Name = "bx_" & S.Station_Safe
                FormatBox.SnapsToDevicePixels = True
                FormatBox.Width = 28
                FormatBox.Height = 15

                If S.IsActive Then
                    FormatBox.Background = GetSelectedColor(lb.Foreground)
                Else
                    FormatBox.Background = New SolidColorBrush(Colors.WhiteSmoke)
                End If

                FormatBox.BorderBrush = lb.Foreground
                FormatBox.BorderThickness = New Thickness(2)
                FormatBox.CornerRadius = New CornerRadius(2)
                Canvas.SetTop(FormatBox, 0)
                Canvas.SetLeft(FormatBox, 170)
                pn.Children.Add(FormatBox)

                'Draw format text
                Dim tb As New System.Windows.Controls.TextBlock
                tb.Name = "lb_" & S.Station_Safe
                tb.Text = S.Format.ToString
                tb.FontFamily = New System.Windows.Media.FontFamily("Arial")
                tb.FontSize = 9.5
                tb.FontWeight = FontWeights.Normal
                tb.Foreground = FormatBox.BorderBrush
                tb.Width = FormatBox.Width - 2
                tb.Tag = S
                tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                tb.TextAlignment = TextAlignment.Center
                'AddHandler tb.MouseLeftButtonUp, AddressOf HandleLeftButtonUp_FormatBox
                'AddHandler tb.MouseRightButtonUp, AddressOf HandleRightButtonUp_FormatBox
                'AddHandler tb.MouseRightButtonUp, AddressOf HandleMouseRightButtonUp_StreamLine
                FormatBox.Child = tb

                'Draw timeline
                Dim ln As New Line
                ln.Name = "ln_" & S.Station_Safe
                ln.SnapsToDevicePixels = True
                ln.X1 = 198
                ln.Y1 = 7
                ln.X2 = 803
                ln.Y2 = 7
                ln.Stroke = lb.Foreground
                ln.StrokeThickness = 2
                ln.Tag = S
                Canvas.SetTop(ln, 0)
                Canvas.SetLeft(ln, 0)
                Canvas.SetZIndex(ln, 0)
                'AddHandler ln.MouseRightButtonUp, AddressOf HandleMouseRightButtonUp_StreamLine
                pn.Children.Add(ln)

                If PROJECT IsNot Nothing Then

                    'Draw boxes to indicate presense of descriptors
                    Dim Ds() As cMD_LinearItem = PROJECT.GetDescriptorsForElementaryStream(S.Category, S.PID)
                    If Ds IsNot Nothing AndAlso Ds.Count > 0 Then

                        'draw the boxes
                        Dim PTS_0_Left As UInt32 = ln.X1
                        Dim PTS_MAX_Width As UInt32 = ln.X2 - ln.X1
                        Dim rt As System.Windows.Shapes.Rectangle

                        For i As Integer = 0 To Ds.Count - 1
                            If Not Ds(i).IsModifier Then
                                'CREATE MASTER BOX
                                rt = New System.Windows.Shapes.Rectangle
                                rt.Name = "descriptor_rect_" & S.Station_Safe & "_" & Ds(i).MediaTime_Start
                                rt.SnapsToDevicePixels = True
                                rt.Width = PTS_MAX_Width
                                rt.Height = 10
                                rt.Fill = GetSelectedColor(lb.Foreground)
                                rt.Stroke = lb.Foreground
                                rt.StrokeThickness = 1
                                rt.RadiusX = 0
                                rt.RadiusY = 0
                                rt.Tag = Ds(i) 'store the linear item so it is easier to lookup later
                                Canvas.SetTop(rt, 2)
                                Canvas.SetLeft(rt, PTS_0_Left)
                                Canvas.SetZIndex(rt, 100)
                                AddHandler rt.MouseLeftButtonUp, AddressOf HandleStreamLineDescriptorBox_MouseLeftButtonUp
                                AddHandler rt.MouseRightButtonUp, AddressOf HandleStreamLineDescriptorBox_MouseRightButtonUp
                                pn.Children.Add(rt)

                            Else
                                'CREATE MODIFIER BOX
                                rt = New System.Windows.Shapes.Rectangle
                                rt.Name = "descriptor_rect_" & S.Station_Safe & "_" & Ds(i).MediaTime_Start
                                rt.SnapsToDevicePixels = True
                                rt.Height = 8
                                rt.Fill = New SolidColorBrush(Colors.WhiteSmoke) 'GetSelectedColor(lb.Foreground)
                                rt.Stroke = lb.Foreground 'New SolidColorBrush(Colors.Blue) 
                                rt.StrokeThickness = 1
                                rt.Opacity = 0.6
                                rt.RadiusX = 1
                                rt.RadiusY = 1
                                rt.Tag = Ds(i) 'store the linear item so it is easier to lookup later
                                Canvas.SetTop(rt, 3)

                                Debug.WriteLine("PTS_0_Left = " & PTS_0_Left)
                                Debug.WriteLine("PTS_MAX_Width = " & PTS_MAX_Width)
                                Debug.WriteLine("MediaTime_Start = " & Ds(i).MediaTime_Start)
                                'Debug.WriteLine("rt = " & Me.txtRunningTime.Text)
                                Dim l As UInt32 = GetLeftForModifierBox(PTS_0_Left, PTS_MAX_Width, Ds(i).MediaTime_Start)
                                Debug.WriteLine("Box_Left = " & l)

                                Canvas.SetLeft(rt, l)
                                Canvas.SetZIndex(rt, 100)
                                rt.Width = GetWidthForModifierBox(PTS_0_Left, PTS_MAX_Width, Ds(i).MediaTime_Start, Ds(i).MediaTime_End)
                                AddHandler rt.MouseLeftButtonUp, AddressOf HandleStreamLineDescriptorBox_MouseLeftButtonUp
                                AddHandler rt.MouseRightButtonUp, AddressOf HandleStreamLineDescriptorBox_MouseRightButtonUp
                                pn.Children.Add(rt)
                            End If
                        Next

                    End If
                End If
                Return pn
            Catch ex As Exception
                Throw New Exception("Problem with CreateESCanvas(). Error: " & ex.Message, ex)
            End Try
        End Function

#Region "FUNCTIONALITY:CONSTRUCTION:M2TS"

        Private Sub DrawVizualizer_M2TS()
            Try
                Dim tEx As Expander
                'PRIMARY STREAMS
                tEx = CreateExpanderForStreamSet(CType(Player, cM2TSPlayer).PrimaryStreams)
                tEx.IsExpanded = True
                Me.spMain.Children.Add(tEx)
                'SECONDARY STREAMS
                tEx = CreateExpanderForStreamSet(CType(Player, cM2TSPlayer).SecondaryStreams)
                tEx.IsExpanded = True
                Me.spMain.Children.Add(tEx)
            Catch ex As Exception
                Throw New Exception("Problem with DrawVizualizer_M2TS(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Function CreateExpanderForStreamSet(ByVal StreamSet As cM2TS_StreamSet) As Expander
            Try
                Dim out As New Expander()
                out.Header = StreamSet.SetType.ToString & " Streams"
                out.Name = "ex" & Replace(out.Header, " ", "_")
                Dim sp As New StackPanel
                sp.Name = "sp" & out.Name

                'VIDEO
                For Each ES As cStreamInfo In StreamSet.Streams(cStreamInfo.eCategories_General.Video)
                    sp.Children.Add(CreateCanvas_ES(ES, sp))
                Next

                'AUDIO
                For Each ES As cStreamInfo In StreamSet.Streams(cStreamInfo.eCategories_General.Audio)
                    sp.Children.Add(CreateCanvas_ES(ES, sp))
                Next

                'GRAPHICS
                For Each ES As cStreamInfo In StreamSet.Streams(cStreamInfo.eCategories_General.Graphics)
                    sp.Children.Add(CreateCanvas_ES(ES, sp))
                Next

                out.Content = sp
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with CreateStationExpander(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FUNCTIONALITY:CONSTRUCTION:M2TS

#End Region 'FUNCTIONALITY:CONSTRUCTION

#Region "FUNCTIONALITY:UI EVENTS:STREAM LINE"

        Private ActiveStreamCanvas As Canvas
        Private LastLineColor As SolidColorBrush

        Private Sub HandleMouseEnter_StreamLine(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
            Try
                Dim fe As FrameworkElement = CType(sender, FrameworkElement)
                If fe Is Nothing Then Exit Sub

                ActiveStreamCanvas = CType(sender, Canvas)
                Dim l As Line
                For Each child As FrameworkElement In ActiveStreamCanvas.Children
                    l = TryCast(child, Line)
                    If l IsNot Nothing Then
                        Me.LastLineColor = l.Stroke
                        l.Stroke = GetSelectedColor(l.Stroke)
                    End If
                Next

            Catch ex As Exception
                Throw New Exception("Problem with HandleMouseEnter_StreamLine(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub HandleMouseLeave_StreamLine(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
            Try
                Dim fe As FrameworkElement = CType(sender, FrameworkElement)
                If fe Is Nothing Then Exit Sub

                Dim l As Line
                For Each child As FrameworkElement In ActiveStreamCanvas.Children
                    l = TryCast(child, Line)
                    If l IsNot Nothing Then
                        l.Stroke = Me.LastLineColor
                    End If
                Next
            Catch ex As Exception
                Throw New Exception("Problem with HandleMouseLeave_StreamLine(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub HandleMouseRightButtonUp_StreamLine(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
            Dim p As System.Windows.Point = e.GetPosition(sender)
            RaiseEvent evStreamLine_MouseRightButtonUp(sender, p)
        End Sub

#End Region 'FUNCTIONALITY:UI EVENTS:STREAM LINE

#End Region 'FUNCTIONALITY

#Region "PRIVATE METHODS"

        Private Function GetSelectedColor(ByVal B As System.Windows.Media.SolidColorBrush) As SolidColorBrush
            Dim scb As SolidColorBrush = CType(B, SolidColorBrush)
            Dim c As System.Windows.Media.Color = scb.Color
            Return New SolidColorBrush(System.Windows.Media.Color.FromArgb(100, c.R, c.G, c.B))
        End Function

#End Region 'PRIVATE METHODS

#Region "SKUNKWORKS SPECIFIC"

#Region "SKUNKWORKS:PROPERTIES"

        Public Property PROJECT() As cMD_PRJ_Bluray_Stream
            Get
                Return _PROJECT
            End Get
            Set(ByVal value As cMD_PRJ_Bluray_Stream)
                _PROJECT = value
            End Set
        End Property
        Private _PROJECT As cMD_PRJ_Bluray_Stream

#End Region 'SKUNKWORKS:PROPERTIES

#Region "SKUNKWORKS:EVENTS"

        Public Event evDescriptorBox_MouseLeftButtonUp(ByVal Tag As Object)
        Public Event evDescriptorBox_MouseRightButtonUp(ByVal Parent As FrameworkElement, ByVal Tag As Object, ByVal p As System.Windows.Point)
        'Public Event evAddMasterDescriptorSet(ByVal SI As cStreamInfo)
        'Public Event evDeleteMasterDescriptorSet(ByVal SI As cStreamInfo)
        'Public Event evCopyMasterDescriptorSet(ByVal SI As cStreamInfo)
        'Public Event evCopyAllDescriptors(ByVal SI As cStreamInfo)
        'Public Event evPasteDescriptors(ByVal SI As cStreamInfo)
        'Public Event evDeleteSingleModifier(ByVal SI As cStreamInfo)
        'Public Event evDeleteAllDescriptorsForStation(ByVal SI As cStreamInfo)
        'Public Event evCopyModiferSet(ByVal Tag As Object)
        'Public Event evAddModiferDescriptorSet(ByVal SI As cStreamInfo, ByVal PTS_OnContextOpen As UInt64)

#End Region 'SKUNKWORKS:EVENTS

#Region "FUNCTIONALITY:UI:VIZUALIZERS:DESCRIPTOR BOXES"

        Private Sub HandleStreamLineDescriptorBox_MouseLeftButtonUp(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
            Try
                Dim rt As System.Windows.Shapes.Rectangle = TryCast(sender, System.Windows.Shapes.Rectangle)
                If rt Is Nothing Then Exit Sub
                RaiseEvent evDescriptorBox_MouseLeftButtonUp(rt.Tag)

                ''MOVE BACK TO SW:
                'Dim li As cMD_LinearItem = TryCast(rt.Tag, cMD_LinearItem)
                'If li Is Nothing Then Exit Sub
                'DisplayDescriptor(li)

            Catch ex As Exception
                Throw New Exception("Problem with HandleStreamLineDescriptorBox_MouseLeftButtonUp(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub HandleStreamLineDescriptorBox_MouseRightButtonUp(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
            Try
                Dim rt As System.Windows.Shapes.Rectangle = TryCast(sender, System.Windows.Shapes.Rectangle)
                If rt Is Nothing Then Exit Sub
                Dim p As System.Windows.Point = e.GetPosition(rt.Parent)
                RaiseEvent evDescriptorBox_MouseRightButtonUp(rt.Parent, rt.Tag, p)
                e.Handled = True

                ''MOVE BACK TO SW:
                'Dim li As cMD_LinearItem = TryCast(rt.Tag, cMD_LinearItem)
                'If li Is Nothing Then Exit Sub
                'ShowStreamContextMenu(rt.Parent, p, li)

            Catch ex As Exception
                Throw New Exception("Problem with HandleStreamLineDescriptorBox_MouseRightButtonUp(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Function GetLeftForModifierBox(ByRef PTS_0_Left As UInt32, ByRef PTS_MAX_Width As UInt32, ByRef MediaTime_Start As UInt64) As UInt32
            Try
                Dim p As cM2TSPlayer = TryCast(Player, cM2TSPlayer)
                If p Is Nothing Then Throw New Exception("It is illegal to call this method if the player is not M2TS.")
                Debug.WriteLine("PTS_Duration = " & p.PTS_Duration)
                Debug.WriteLine("PTS MAX-BASE = " & p.PTS_MAX - p.PTS_Base)
                Dim per As Double = MediaTime_Start / p.PTS_Duration 'how far in (percentage) should this box be written?
                Debug.WriteLine("per = " & per)
                'now convert that percentage into a point between PTS_0_Left and PTS_MAX_Width
                Dim offset As Double = per * PTS_MAX_Width
                Debug.WriteLine("offset = " & offset)
                Return PTS_0_Left + offset
            Catch ex As Exception
                Throw New Exception("Problem with GetLeftForModifierBox(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function GetWidthForModifierBox(ByRef PTS_0_Left As UInt32, ByRef PTS_MAX_Width As UInt32, ByRef MediaTime_Start As UInt64, ByRef MediaTime_End As UInt64) As UInt32
            Try
                If MediaTime_End = UInt64.MaxValue Then MediaTime_End = MediaTime_Start + 200000
                Dim Start_pos As UInt32 = GetLeftForModifierBox(PTS_0_Left, PTS_MAX_Width, MediaTime_Start)

                ''DEBUGGING
                'MediaTime_End = PLAYER.PTS_MAX
                ''DEBUGGING

                Dim End_pos As UInt32 = GetLeftForModifierBox(PTS_0_Left, PTS_MAX_Width, MediaTime_End)
                Return End_pos - Start_pos
            Catch ex As Exception
                Throw New Exception("Problem with GetWidthForModifierBox(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'FUNCTIONALITY:UI:VIZUALIZERS:MOUSE:DESCRIPTOR BOXES

#End Region 'SKUNKWORKS SPECIFIC

#Region "STRUCTURES AND ENUMS"

        Public Enum eVizualizerMode
            Generic
            Skunkworks
        End Enum

#End Region 'STRUCTURES AND ENUMS

    End Class

End Namespace
