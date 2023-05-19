Imports SMT.Multimedia.Players.DVD.Enums
Imports System.Drawing
Imports System.Windows.Forms
Imports SMT.Multimedia.Players
Imports System.Windows
Imports SMT.Win.UserInterface
Imports SMT.Multimedia.Players.DVD
Imports SMT.Multimedia.Enums

Namespace Multimedia.UI.WinForms.Viewers

    Public Class Viewer_Form
        Inherits Form

        Public Sub New(ByVal nX As Short, ByVal nY As Short, ByVal nIcon As Icon, ByRef nPlayer As cBasePlayer, ByVal nDVDPlayer As Boolean, Optional ByVal nViewerName As String = "SMT")
            MyBase.New()

            'This call is required by the Windows Form Designer.
            InitializeComponent()

            'Add any initialization after the InitializeComponent() call
            Player = nPlayer
            DVDPlayer = nDVDPlayer
            Me.X = nX
            Me.Y = nY
            Me.MyIcon = nIcon
            ViewerName = nViewerName
        End Sub

        Public ForceClose As Boolean = False

        Private DVDPlayer As Boolean
        Public X, Y As Short
        Private MyIcon As Icon
        Private Player As cBasePlayer
        Private VidWinSize As System.Windows.Size
        Private ViewerName As String

        Public PreFS_Location As System.Drawing.Point

        Public Property PreFS_Size() As eViewerSize
            Get
                Return _PreFS_Size
            End Get
            Set(ByVal value As eViewerSize)
                If value = eViewerSize.Fullscreen Then Exit Property
                _PreFS_Size = value
            End Set
        End Property
        Private _PreFS_Size As eViewerSize

        Public ReadOnly Property InFullscreen() As Boolean
            Get
                'Return Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
            End Get
        End Property

        Private Sub Viewer_Form_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            DisableFormClose(Me.Handle)
        End Sub

        Private Sub Viewer_Form_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
            If Not ForceClose Then e.Cancel = True
        End Sub

        Private Sub Viewer_Form_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
            If Player Is Nothing Then Exit Sub
            If DVDPlayer Then
                'Dim DVD1 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                'If Not DVD1 Is Nothing Then
                '    DVD1.KeyStrike(e)
                '    GoTo Cont
                'End If
                Dim DVD2 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                If Not DVD2 Is Nothing Then
                    DVD2.KeyStrike(e)
                End If
            End If
Cont:
            If e.KeyCode = Keys.Escape Then
                If InFullscreen Then Me.CancelFullScreen()
            End If
        End Sub

        Private Sub MainForm_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
            Me.SetVideoWin()
        End Sub

        Private Sub MainForm_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseMove
            If Player Is Nothing Then Exit Sub
            If DVDPlayer Then
                Dim p As New System.Drawing.Point(e.X, e.Y)

                'Dim DVD1 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                'If Not DVD1 Is Nothing Then
                '    If DVD1.InputMousePosition(p) Then
                '        Me.Cursor = Cursors.Hand
                '    Else
                '        Me.Cursor = Cursors.Arrow
                '    End If
                'End If
                Dim DVD2 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                If Not DVD2 Is Nothing Then
                    If DVD2.InputMousePosition(p.X, p.Y) Then
                        Me.Cursor = Cursors.Hand
                    Else
                        Me.Cursor = Cursors.Arrow
                    End If
                End If
            End If
        End Sub

        Private Sub MainForm_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseDown
            If Player Is Nothing Then Exit Sub
            If DVDPlayer Then
                Dim p As New System.Drawing.Point(e.X, e.Y)
                'Dim DVD1 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                'If Not DVD1 Is Nothing Then
                '    DVD1.ActivateButtonAt(p)
                'End If
                Dim DVD2 As cDVDPlayer = TryCast(Player, cDVDPlayer)
                If Not DVD2 Is Nothing Then
                    DVD2.ActivateButtonAt(p.X, p.Y)
                End If
            End If
        End Sub

        Private Sub Viewer_Form_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
            Me.Top = Y
            Me.Left = X
            Me.Icon = MyIcon
        End Sub

        Public Property ViewerSize() As eViewerSize
            Get
                Return _ViewerSize
            End Get
            Set(ByVal value As eViewerSize)
                If Not value = eViewerSize.Fullscreen And InFullscreen Then
                    Me.CancelFullScreen()
                End If
                Select Case value
                    Case eViewerSize.NTSC_Half_360x243
                        Me.ClientSize = New System.Drawing.Size(360, 243)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.NTSC_Half_360x243
                    Case eViewerSize.NTSC_720x486
                        Me.ClientSize = New System.Drawing.Size(720, 486)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.NTSC_720x486
                    Case eViewerSize.NTSC_Anamorphic_853x486
                        Me.ClientSize = New System.Drawing.Size(853, 486)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.NTSC_Anamorphic_853x486
                    Case eViewerSize.PAL_Half_360x288
                        Me.ClientSize = New System.Drawing.Size(360, 288)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.PAL_Half_360x288
                    Case eViewerSize.PAL_720x576
                        Me.ClientSize = New System.Drawing.Size(720, 576)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.PAL_720x576
                    Case eViewerSize.PAL_Anamorphic_853x576
                        Me.ClientSize = New System.Drawing.Size(853, 576)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.PAL_Anamorphic_853x576
                    Case eViewerSize.HD_Quarter_480x270
                        Me.ClientSize = New System.Drawing.Size(480, 270)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.HD_Quarter_480x270
                    Case eViewerSize.HD_Half_960x540
                        Me.ClientSize = New System.Drawing.Size(960, 540)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.HD_Half_960x540
                    Case eViewerSize.HD_1920x1080
                        Me.ClientSize = New System.Drawing.Size(1920, 1080)
                        SetVideoWin()
                        _ViewerSize = eViewerSize.HD_1920x1080
                    Case eViewerSize.Fullscreen
                        Me.SetFullScreen(Screen.GetBounds(Me))
                        _ViewerSize = eViewerSize.Fullscreen
                End Select
            End Set
        End Property
        Private _ViewerSize As eViewerSize = eViewerSize.NTSC_Half_360x243

        Private Sub SetVideoWin()
            Try
                If Player Is Nothing Then Exit Sub
                If Player.Graph Is Nothing Then Exit Sub
                If Player.Graph.VideoWin Is Nothing Then Exit Sub
                'Player.Graph.VideoWin.SetWindowPosition(0, 0, Me.ClientRectangle.Right, Me.ClientRectangle.Bottom)
                ScaleVideoWindow()
            Catch ex As Exception
                Throw New Exception("Problem with SetVideoWin(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub ScaleVideoWindow()
            Try
                Dim ClientAreaAR As Double = Me.ClientRectangle.Width / Me.ClientRectangle.Height
                Dim VideoAR As Double = Player.CurrentVideoDimensions.Width / Player.CurrentVideoDimensions.Height
                If ClientAreaAR <> VideoAR Then
                    'we must scale
                    Dim Ratio As Double = Me.ClientRectangle.Width / Player.CurrentVideoDimensions.Width
                    Dim VidWinHeight As Integer = Math.Round(Player.CurrentVideoDimensions.Height * Ratio, 0)
                    Dim VerticalOffset As Integer = Math.Abs(ClientRectangle.Height - VidWinHeight) / 2
                    Player.Graph.VideoWin.SetWindowPosition(0, VerticalOffset, ClientRectangle.Width, VidWinHeight)

                    Me.Text = ViewerName & " Viewer - " & ClientRectangle.Width & "x" & VidWinHeight
                    VidWinSize = New System.Windows.Size(ClientRectangle.Width, VidWinHeight)

                    'If (Me.ClientRectangle.Width = Player.CurrentVideoDimensions.Width) And (Player.CurrentVideoDimensions.Height <= Me.ClientRectangle.Height) Then
                    '    Player.Graph.VideoWin.SetWindowPosition(0, (Me.ClientRectangle.Height - Player.CurrentVideoDimensions.Height) / 2, ClientRectangle.Width, Player.CurrentVideoDimensions.Height)
                    'End If

                    'Player.Graph.VideoWin.SetWindowPosition(0, 60, ClientRectangle.Right, 1080)

                Else
                    'default
                    Player.Graph.VideoWin.SetWindowPosition(0, 0, Me.ClientRectangle.Right, Me.ClientRectangle.Bottom)
                    Me.Text = ViewerName & " Viewer - " & ClientRectangle.Width & "x" & ClientRectangle.Height
                    VidWinSize = New System.Windows.Size(ClientRectangle.Width, ClientRectangle.Height)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with ScaleVideoWindow(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub SetFullScreen(ByVal TargetMonitorRect As Rectangle)
            Try
                'Player.Graph.VideoWin.put_FullScreenMode(Media.DirectShow.Microsoft_DShow.OABool.True)
                ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                PreFS_Location = Location
                PreFS_Size = ViewerSize

                'FormBorderStyle = Windows.Forms.FormBorderStyle.None
                TopMost = True
                Left = TargetMonitorRect.X
                Top = TargetMonitorRect.Y
                Width = TargetMonitorRect.Width
                Height = TargetMonitorRect.Height
                Me.SetDimensionsLabel()
            Catch ex As Exception
                Throw New Exception("Problem with SetFullScreen(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub SetDimensionsLabel()
            Me.lblVidWinSize.Visible = True
            Me.lblVidWinSize.Text = Me.VidWinSize.Width & "x" & Me.VidWinSize.Height
        End Sub

        Public Sub CancelFullScreen()
            Try
                'Player.Graph.VideoWin.put_FullScreenMode(Media.DirectShow.Microsoft_DShow.OABool.False)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'FormBorderStyle = Windows.Forms.FormBorderStyle.FixedDialog
                'TopMost = False
                Location = PreFS_Location
                ViewerSize = PreFS_Size
                Me.lblVidWinSize.Visible = False

            Catch ex As Exception
                Throw New Exception("Problem with CancelFullScreen(). Error: " & ex.Message, ex)
            End Try
        End Sub

        'Public Property PreventMove() As Boolean
        '    Get
        '        Return _PreventMove
        '    End Get
        '    Set(ByVal value As Boolean)
        '        _PreventMove = value
        '    End Set
        'End Property
        'Private _PreventMove As Boolean = False

        'Private Sub Viewer_Form_Move(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Move
        '    'If msg = WM_WINDOWPOSCHANGING Then
        '    '    ' Reset the position.
        '    '    lParam.x = DesiredX
        '    '    lParam.y = DesiredY
        '    'End If
        '    If PreventMove Then
        '        Me.Left = X
        '        Me.Top = Y
        '    End If
        'End Sub

#Region "Viewer Size Context Menu"

        Private Sub cmViewerSize_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles cmViewerSize.Opening
            Try
                For i As Byte = 0 To Me.cmViewerSize.Items.Count - 1
                    CType(Me.cmViewerSize.Items(i), ToolStripMenuItem).Checked = False
                Next
                Select Case Me.ViewerSize
                    Case eViewerSize.NTSC_Anamorphic_853x486
                        CType(Me.cmViewerSize.Items(2), ToolStripMenuItem).Checked = True
                    Case eViewerSize.PAL_Anamorphic_853x576
                        CType(Me.cmViewerSize.Items(5), ToolStripMenuItem).Checked = True
                    Case eViewerSize.HD_1920x1080
                        CType(Me.cmViewerSize.Items(8), ToolStripMenuItem).Checked = True
                    Case eViewerSize.NTSC_720x486
                        CType(Me.cmViewerSize.Items(1), ToolStripMenuItem).Checked = True
                    Case eViewerSize.PAL_720x576
                        CType(Me.cmViewerSize.Items(4), ToolStripMenuItem).Checked = True
                    Case eViewerSize.HD_Half_960x540
                        CType(Me.cmViewerSize.Items(7), ToolStripMenuItem).Checked = True
                    Case eViewerSize.NTSC_Half_360x243
                        CType(Me.cmViewerSize.Items(0), ToolStripMenuItem).Checked = True
                    Case eViewerSize.PAL_Half_360x288
                        CType(Me.cmViewerSize.Items(3), ToolStripMenuItem).Checked = True
                    Case eViewerSize.HD_Quarter_480x270
                        CType(Me.cmViewerSize.Items(6), ToolStripMenuItem).Checked = True
                    Case eViewerSize.Fullscreen
                        CType(Me.cmViewerSize.Items(9), ToolStripMenuItem).Checked = True
                End Select
            Catch ex As Exception
                Debug.WriteLine("Problem with cmViewerSize_Opening(). Error: " & ex.Message)
            End Try
        End Sub

        Private Sub miNTSC_Half_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miNTSC_Half.Click
            Me.ViewerSize = eViewerSize.NTSC_Half_360x243
        End Sub

        Private Sub miNTSC_Full_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miNTSC_Full.Click
            Me.ViewerSize = eViewerSize.NTSC_720x486
        End Sub

        Private Sub miNTSC_Anamorphic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miNTSC_Anamorphic.Click
            Me.ViewerSize = eViewerSize.NTSC_Anamorphic_853x486
        End Sub

        Private Sub miPAL_Half_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miPAL_Half.Click
            Me.ViewerSize = eViewerSize.PAL_Half_360x288
        End Sub

        Private Sub miPAL_Full_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miPAL_Full.Click
            Me.ViewerSize = eViewerSize.PAL_720x576
        End Sub

        Private Sub miPAL_Anamorphic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miPAL_Anamorphic.Click
            Me.ViewerSize = eViewerSize.PAL_Anamorphic_853x576
        End Sub

        Private Sub miHD_Quarter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miHD_Quarter.Click
            Me.ViewerSize = eViewerSize.HD_Quarter_480x270
        End Sub

        Private Sub miHD_Half_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miHD_Half.Click
            Me.ViewerSize = eViewerSize.HD_Half_960x540
        End Sub

        Private Sub miHD_Full_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miHD_Full.Click
            Me.ViewerSize = eViewerSize.HD_1920x1080
        End Sub

        Private Sub miFullscreen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles miFullscreen.Click
            Me.ViewerSize = eViewerSize.Fullscreen
        End Sub

#End Region

    End Class

End Namespace
