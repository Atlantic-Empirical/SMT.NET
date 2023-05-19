
Namespace Multimedia.Filters.MainConcept

    Public Module mMainConcept_Scaler

        Public ReadOnly MC_SCALER_CropImage As Guid = New Guid("EDD0D20C-D35E-4da8-B9F9-E6F07E0FE861")
        '5.1 ImScaler_CropImage
        'GUID:
        '{EDD0D20C-D35E-4da8-B9F9-E6F07E0FE861}
        'Description:
        'Enables/disables cropping.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Disable
        '• 1 – Enable

        Public ReadOnly MC_SCALER_ResizeImage As Guid = New Guid("A68F13F1-CC33-436a-BCBF-0A5C95EBE27F")
        '5.2 ImScaler_ResizeImage
        'GUID:
        '{A68F13F1-CC33-436a-BCBF-0A5C95EBE27F}
        'Description:
        'Enables/disables resizing.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Disable
        '• 1 – Enable

        Public ReadOnly MC_SCALER_ResizeType As Guid = New Guid("B102C6E1-DCF7-441b-8890-B884B0949902")
        '5.3 ImScaler_ResizeType
        'GUID:
        '{B102C6E1-DCF7-441b-8890-B884B0949902}
        'Description:
        'Selects a resampling filter.
        'Type:
        'VT_UI1
        'Available Values:
        '• 0 – B-Spline filter
        '• 1 – Notch filter
        '• 2 – Catmull-Rom spline filter
        '• 3 – Mitchell-Netravali filter

        Public ReadOnly MC_SCALER_AppendixMode As Guid = New Guid("E0CAC972-450E-44ae-9825-BC6E2448CD67")
        '5.4 ImScaler_AppendixMode
        'GUID:
        '{E0CAC972-450E-44ae-9825-BC6E2448CD67}
        'Description:
        'Enables/disables the black stripes (mattes) addition (letterboxing).
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Disable
        '• 1 – Enable
        '• 2 – Keep Aspect Ratio

        Public ReadOnly MC_SCALER_AppendixStripesType As Guid = New Guid("B23C99E0-D86F-4d50-85E4-7A0D6F5C9C61")
        '5.5 ImScaler_AppendixStripesType
        'GUID:
        '{B23C99E0-D86F-4d50-85E4-7A0D6F5C9C61}
        'Description:
        'Selects the black stripes location.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – At top and bottom of the frame.
        '• 1 – At left and right sides of the frame.
        '• 2 – Simultaneously vertical and horizontal.

        Public ReadOnly MC_SCALER_CropImageXLeft As Guid = New Guid("57F15B02-E645-4d90-8834-31AEEF960A07")
        '5.6 ImScaler_CropImageXLeft
        'GUID:
        '{57F15B02-E645-4d90-8834-31AEEF960A07}
        'Description:
        'Specifies the x-coordinate of the cropping rectangle’s upper-left corner.
        'Type:
        'VT_UINT
        'Available Values:
        'From 0 up to the source frame width.

        Public ReadOnly MC_SCALER_CropImageYTop As Guid = New Guid("C2F3CF68-417F-413b-926C-A09D67AAB84D")
        '5.7 ImScaler_CropImageYTop
        'GUID:
        '{C2F3CF68-417F-413b-926C-A09D67AAB84D}
        'Description:
        'Specifies the y-coordinate of the cropping rectangle’s upper-left corner.
        'Type:
        'VT_UINT
        'Available Values:
        'From 0 up to the source frame height.

        Public ReadOnly MC_SCALER_CropImageXRight As Guid = New Guid("C038AA00-17DC-4c5b-A3FE-F31B0F68A75A")
        '5.8 ImScaler_CropImageXRight
        'GUID:
        '{C038AA00-17DC-4c5b-A3FE-F31B0F68A75A}
        'Description:
        'Specifies the x-coordinate of the cropping rectangle’s lower-right corner.
        'Type:
        'VT_UINT
        'Available Values:
        'From ImScaler_CropImageXLeft value up to the source frame width.
        '© Copyright MainConcept GmbH, 2008. All rights reserved.
        'MainConcept Codec SDK DirectShow Documentation Page 499

        Public ReadOnly MC_SCALER_CropImageYBot As Guid = New Guid("9DC1C8D4-0870-40bc-B78B-F00190E03112")
        '5.9 ImScaler_CropImageYBot
        'GUID:
        '{9DC1C8D4-0870-40bc-B78B-F00190E03112}
        'Description:
        'Specifies the y-coordinate of the cropping rectangle’s lower-right corner.
        'Type:
        'VT_UINT
        'Available Values:
        'From ImScaler_CropImageYTop up to the source frame height.

        Public ReadOnly MC_SCALER_DestHeight As Guid = New Guid("32A5E96D-2AC3-4d46-BF70-431BEE62DF35")
        '5.10 ImScaler_DestHeight
        'GUID:
        '{32A5E96D-2AC3-4d46-BF70-431BEE62DF35}
        'Description:
        'Specifies the height of output frames.
        'Type:
        'VT_UINT
        'Available Values:
        '16 – 4096

        Public ReadOnly MC_SCALER_DestWidth As Guid = New Guid("04B86B7E-C04B-448f-BFF5-2A50F1945ACD")
        '5.11 ImScaler_DestWidth
        'GUID:
        '{04B86B7E-C04B-448f-BFF5-2A50F1945ACD}
        'Description:
        'Specifies the width of output frames.
        'Type:
        'VT_UINT
        'Available Values:
        '16 – 4096

        Public ReadOnly MC_SCALER_StripesSizeValue As Guid = New Guid("BFC35D7A-B763-439a-89CE-4E20F6192B99")
        '5.12 ImScaler_StripesSizeValue
        'GUID:
        '{BFC35D7A-B763-439a-89CE-4E20F6192B99}
        'Description:
        'Specifies the height of each black bar that is appended to the output frame.
        'Type:
        'VT_UINT
        'Available Values:
        '0 – ((MAX_RESIZE - Resize_Height) / 2)

        Public ReadOnly MC_SCALER_StripesSizeValue2 As Guid = New Guid("E9F189CB-5ACB-4916-94CA-498D2A92E9E6")
        '5.13 ImScaler_StripesSizeValue2
        'GUID:
        '{E9F189CB-5ACB-4916-94CA-498D2A92E9E6}
        'Description:
        'Specifies the size of additional stripes (in pixels), used if both vertical and horizontal stripes
        'exist. Second value for size of appendix stripes.
        'Type:
        'VT_UINT
        'Available Values:
        '0 – ((MAX_RESIZE - Resize_Height) / 2)

        Public ReadOnly MC_SCALER_PictureType As Guid = New Guid("2E1DD59C-78B4-4f17-A0BA-41DEC7756EE1")
        '5.14 ImScaler_PictureType
        'GUID:
        '{2E1DD59C-78B4-4f17-A0BA-41DEC7756EE1}
        'Description:
        'Sets a picture type.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Interlaced
        '• 1 – Progressive
        '• 2 – Autodetecting

        Public ReadOnly MC_SCALER_FilterState As Guid = New Guid("0F414B44-5B63-43cd-8813-4B34A9A84C67")
        '5.15 ImScaler_FilterState
        'GUID:
        '{0F414B44-5B63-43cd-8813-4B34A9A84C67}
        'Description:
        'Indicates the filter state (Stopped, Running, Paused etc). Read-only.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Filter Stopped
        '• 1 – Filter Paused
        '• 2 – Filter Running

        Public ReadOnly MC_SCALER_SourceWidth As Guid = New Guid("E35D983E-2004-45e2-A5FB-59AE94FCC258")
        '5.16 ImScaler_SourceWidth
        'GUID:
        '{E35D983E-2004-45e2-A5FB-59AE94FCC258}
        'Description:
        'Indicates width of the input frame. Read-only.
        'Type:
        'VT_I4
        'Available Values:
        '0 – 4096

        Public ReadOnly MC_SCALER_SourceHeight As Guid = New Guid("ECCA3864-E79E-4e77-A46E-D823CA882309")
        '5.17 ImScaler_SourceHeight
        'GUID:
        '{ECCA3864-E79E-4e77-A46E-D823CA882309}
        'Description:
        'Indicates height of the input frame. Read-only.
        'Type:
        'VT_I4
        'Available Values:
        '0 – 4096

        Public ReadOnly MC_SCALER_Align As Guid = New Guid("8803E341-A76D-4874-B5CC-711C92BC385D")
        '5.18 ImScaler_Align
        'GUID:
        '{8803E341-A76D-4874-B5CC-711C92BC385D}
        'Description:
        'Indicates the alignment value for all coordinate values. This value is a step of changing the
        'resizing and cropping parameters. For mattes the Align/2 value is used. Read-only.
        'Type:
        'VT_UINT

        Public ReadOnly MC_SCALER_MaxResizeWidth As Guid = New Guid("A7767F9E-F2DD-4b51-BFDE-626852DD8DA3")
        '5.19 ImScaler_MaxResizeWidth
        'GUID:
        '{A7767F9E-F2DD-4b51-BFDE-626852DD8DA3}
        'Description:
        'Indicates the maximum supported output width.
        'Type:
        'VT_UINT

        Public ReadOnly MC_SCALER_MaxResizeHeight As Guid = New Guid("97A2B1E2-EB2E-469c-A3BF-61E287578E38")
        '5.20 ImScaler_MaxResizeHeight
        'GUID:
        '{97A2B1E2-EB2E-469c-A3BF-61E287578E38}
        'Description:
        'Indicates the maximum supported output height.
        'Type:
        'VT_UINT

        Public ReadOnly MC_SCALER_MinResizeWidth As Guid = New Guid("4FE96158-A190-44e1-BF99-599E292FD999")
        '5.21 ImScaler_MinResizeWidth
        'GUID:
        '{4FE96158-A190-44e1-BF99-599E292FD999}
        'Description:
        'Indicates the minimum supported output width.
        'Type:
        'VT_UINT

        Public ReadOnly MC_SCALER_MinResizeHeight As Guid = New Guid("69189676-63D0-4b5c-8BEB-FAD9A6775B6B")
        '5.22 ImScaler_MinResizeHeight
        'GUID:
        '{69189676-63D0-4b5c-8BEB-FAD9A6775B6B}
        'Description:
        'Indicates the minimum supported output height.
        'Type:
        'VT_UINT

        Public ReadOnly MC_SCALER_SetAspectRatio As Guid = New Guid("6E7CFA3C-7395-4df9-A3D2-7E1DBE442B42")
        '5.23 ImScaler_SetAspectRatio
        'GUID:
        '{6E7CFA3C-7395-4df9-A3D2-7E1DBE442B42}
        'Description:
        'The option sets the current picture aspect ratio from the input. This disables all other aspect
        'settings.
        'Type:
        'VT_BOOL
        'Available Values:
        '• True – Set current picture aspect ratio.
        '• False – Do not set current picture aspect ratio.

        Public ReadOnly MC_SCALER_SetAspect4_3 As Guid = New Guid("D70EC5EF-C271-4728-A5BF-C0B6BD4F6D3E")
        '5.24 ImScaler_SetAspect4_3
        'GUID:
        '{D70EC5EF-C271-4728-A5BF-C0B6BD4F6D3E}
        'Description:
        'The option sets the picture aspect ratio to 4:3. This disables the all other aspect settings.
        'Type:
        'VT_BOOL
        'Available Values:
        '• True – Set picture aspect ratio to 4:3.
        '• False – Do not set picture aspect ratio.

        Public ReadOnly MC_SCALER_SetAspect16_9 As Guid = New Guid("6E7CFA3C-7395-4df9-A3D2-7E1DBE442B42")
        '5.25 ImScaler_SetAspect16_9
        'GUID:
        '{6E7CFA3C-7395-4df9-A3D2-7E1DBE442B42}
        'Description:
        'The option sets the picture aspect ratio to 16:9. This disables the all other aspect settings.
        'Type:
        'VT_BOOL
        'Available Values:
        '• True – Set picture aspect ratio to 16:9.
        '• False – Do not set picture aspect ratio.

        Public ReadOnly MC_SCALER_BindCrop As Guid = New Guid("B14F5971-1EA7-46e8-9B6E-42B0F3E37F7E")
        '5.26 ImScaler_BindCrop
        'GUID:
        '{B14F5971-1EA7-46e8-9B6E-42B0F3E37F7E}
        'Description:
        'Specified whether the crop values should be bound to the source picture size or not.
        'Type:
        'VT_BOOL
        'Available Values:
        '• True – Bind crop values to source picture size.
        '• False – Do not bind crop values to source picture size.

        Public ReadOnly MC_SCALER_CropDefinitionMode As Guid = New Guid("1F9739D4-EAC4-40e2-9BF5-97A618073785")
        '5.27 ImScaler_CropDefinitionMode
        'GUID:
        '{1F9739D4-EAC4-40e2-9BF5-97A618073785}
        'Description:
        'Specified the crop parameters definition mode.
        'Type:
        'VT_UINT
        'Available Values:
        '• 0 – Crop coordinates.
        '• 1 – Crop values.
        '• 2 – Picture offset and size.

    End Module

End Namespace
