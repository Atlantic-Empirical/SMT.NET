
Namespace Multimedia.UI.WinForms.Viewers

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class Viewer_Form

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
            MyBase.Dispose(disposing)
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container
            Me.cmViewerSize = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.miNTSC_Half = New System.Windows.Forms.ToolStripMenuItem
            Me.miNTSC_Full = New System.Windows.Forms.ToolStripMenuItem
            Me.miNTSC_Anamorphic = New System.Windows.Forms.ToolStripMenuItem
            Me.miPAL_Half = New System.Windows.Forms.ToolStripMenuItem
            Me.miPAL_Full = New System.Windows.Forms.ToolStripMenuItem
            Me.miPAL_Anamorphic = New System.Windows.Forms.ToolStripMenuItem
            Me.miHD_Quarter = New System.Windows.Forms.ToolStripMenuItem
            Me.miHD_Half = New System.Windows.Forms.ToolStripMenuItem
            Me.miHD_Full = New System.Windows.Forms.ToolStripMenuItem
            Me.lblVidWinSize = New System.Windows.Forms.Label
            Me.miFullscreen = New System.Windows.Forms.ToolStripMenuItem
            Me.cmViewerSize.SuspendLayout()
            Me.SuspendLayout()
            '
            'cmViewerSize
            '
            Me.cmViewerSize.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.miNTSC_Half, Me.miNTSC_Full, Me.miNTSC_Anamorphic, Me.miPAL_Half, Me.miPAL_Full, Me.miPAL_Anamorphic, Me.miHD_Quarter, Me.miHD_Half, Me.miHD_Full, Me.miFullscreen})
            Me.cmViewerSize.Name = "cmViewerSize"
            Me.cmViewerSize.Size = New System.Drawing.Size(224, 246)
            '
            'miNTSC_Half
            '
            Me.miNTSC_Half.Name = "miNTSC_Half"
            Me.miNTSC_Half.Size = New System.Drawing.Size(223, 22)
            Me.miNTSC_Half.Text = "360x243 (Half NTSC)"
            '
            'miNTSC_Full
            '
            Me.miNTSC_Full.Name = "miNTSC_Full"
            Me.miNTSC_Full.Size = New System.Drawing.Size(223, 22)
            Me.miNTSC_Full.Text = "720x486 (Full NTSC)"
            '
            'miNTSC_Anamorphic
            '
            Me.miNTSC_Anamorphic.Name = "miNTSC_Anamorphic"
            Me.miNTSC_Anamorphic.Size = New System.Drawing.Size(223, 22)
            Me.miNTSC_Anamorphic.Text = "853x486 (Anamorphic NTSC)"
            '
            'miPAL_Half
            '
            Me.miPAL_Half.Name = "miPAL_Half"
            Me.miPAL_Half.Size = New System.Drawing.Size(223, 22)
            Me.miPAL_Half.Text = "360x288 (Half PAL)"
            '
            'miPAL_Full
            '
            Me.miPAL_Full.Name = "miPAL_Full"
            Me.miPAL_Full.Size = New System.Drawing.Size(223, 22)
            Me.miPAL_Full.Text = "720x576 (Full PAL)"
            '
            'miPAL_Anamorphic
            '
            Me.miPAL_Anamorphic.Name = "miPAL_Anamorphic"
            Me.miPAL_Anamorphic.Size = New System.Drawing.Size(223, 22)
            Me.miPAL_Anamorphic.Text = "853x576 (Anamorphic PAL)"
            '
            'miHD_Quarter
            '
            Me.miHD_Quarter.Name = "miHD_Quarter"
            Me.miHD_Quarter.Size = New System.Drawing.Size(223, 22)
            Me.miHD_Quarter.Text = "480x270 (Quarter HD)"
            '
            'miHD_Half
            '
            Me.miHD_Half.Name = "miHD_Half"
            Me.miHD_Half.Size = New System.Drawing.Size(223, 22)
            Me.miHD_Half.Text = "960x540 (Half HD)"
            '
            'miHD_Full
            '
            Me.miHD_Full.Name = "miHD_Full"
            Me.miHD_Full.Size = New System.Drawing.Size(223, 22)
            Me.miHD_Full.Text = "1920x1080 (Full HD)"
            '
            'lblVidWinSize
            '
            Me.lblVidWinSize.AutoSize = True
            Me.lblVidWinSize.Font = New System.Drawing.Font("Tahoma", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.lblVidWinSize.ForeColor = System.Drawing.Color.White
            Me.lblVidWinSize.Location = New System.Drawing.Point(12, 9)
            Me.lblVidWinSize.Name = "lblVidWinSize"
            Me.lblVidWinSize.Size = New System.Drawing.Size(20, 16)
            Me.lblVidWinSize.TabIndex = 1
            Me.lblVidWinSize.Text = "[]"
            Me.lblVidWinSize.Visible = False
            '
            'miFullscreen
            '
            Me.miFullscreen.Name = "miFullscreen"
            Me.miFullscreen.Size = New System.Drawing.Size(223, 22)
            Me.miFullscreen.Text = "Fullscreen"
            '
            'Viewer_Form
            '
            'Me.Appearance.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
            'Me.Appearance.Options.UseBackColor = True
            Me.ClientSize = New System.Drawing.Size(292, 266)
            Me.ContextMenuStrip = Me.cmViewerSize
            Me.Controls.Add(Me.lblVidWinSize)
            Me.KeyPreview = True
            'Me.LookAndFeel.SkinName = "Office 2007 Black"
            Me.MaximizeBox = False
            Me.MinimizeBox = False
            Me.Name = "Viewer_Form"
            Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
            Me.Text = " Viewer"
            Me.cmViewerSize.ResumeLayout(False)
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub
        Friend WithEvents cmViewerSize As System.Windows.Forms.ContextMenuStrip
        Friend WithEvents miNTSC_Half As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miNTSC_Full As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miNTSC_Anamorphic As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miPAL_Half As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miPAL_Full As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miPAL_Anamorphic As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miHD_Quarter As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miHD_Half As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents miHD_Full As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents lblVidWinSize As System.Windows.Forms.Label
        Friend WithEvents miFullscreen As System.Windows.Forms.ToolStripMenuItem
    End Class

End Namespace
