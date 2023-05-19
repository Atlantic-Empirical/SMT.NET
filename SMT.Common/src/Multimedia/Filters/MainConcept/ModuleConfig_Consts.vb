
Namespace Multimedia.Filters.MainConcept

    Public Class ModuleConfig_Consts

#Region "GUIDS"

#Region "GUIDS:SHARED"

        Public Shared ReadOnly Deinterlace As Guid = New Guid("9CF1A332-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly HQUpsample_VC1AVC As Guid = New Guid("9CF1A333-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly DoubleRate As Guid = New Guid("9CF1A339-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly FieldsReordering As Guid = New Guid("9951682E-0F78-4924-92A4-BD7BFBA30063")
        Public Shared ReadOnly FieldsReorderingCondition As Guid = New Guid("8AE4A3D8-240B-427d-B845-5474965CBB48")
        Public Shared ReadOnly FramesDecodedDisplayed As Guid = New Guid("575BA759-6F13-4a84-A126-005A5523D203")
        Public Shared ReadOnly FramesSkipped As Guid = New Guid("592A9FAB-CBF8-4592-A0B2-21D0C79DACE4")
        Public Shared ReadOnly Synchronizing As Guid = New Guid("24F62768-7740-437f-9651-9ED347C0CAD6")
        Public Shared ReadOnly HardwareAcceleration As Guid = New Guid("BA6DDF74-5F8A-4bdc-88BB-A2563314BC3E")
        Public Shared ReadOnly FormatVideoInfo As Guid = New Guid("110272F6-DA17-4162-B6BA-866CC5CB6889")
        Public Shared ReadOnly MediaTimeStart As Guid = New Guid("3903A73E-6A89-4e09-8E9F-95E8A56F614D")
        Public Shared ReadOnly MediaTimeStop As Guid = New Guid("BCF5D243-B80E-400a-9B60-035A1D3E5C38")
        Public Shared ReadOnly EMC_Quality As Guid = New Guid("9CF1A330-E72B-4a6d-BBE8-199595944546")

        ''' <summary>
        '''EMC_ErrorConcealment
        '''GUID:
        '''{BB8F00E9-655B-47c4-966A-A4B4BBF8D2D2}
        '''Description:
        '''Sets the errors concealment mode.
        '''Type:
        '''VT_UI4
        '''Available Values:
        '''· 0 – Error concealment is disabled (default).
        '''· 1 – Error concealment is enabled, i.e. frames with errors are not shown.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared ReadOnly EMC_ErrorConcealment As Guid = New Guid("BB8F00E9-655B-47c4-966A-A4B4BBF8D2D2")

        '''<summary>
        '''5.18 EMC_OSD
        '''GUID:
        '''{ F5C51906-ED89-4C6E-9C37-A5CCB34F5389}
        '''Description:
        '''Enables/disables the option to display of the decoding statistical information.
        '''Type:
        '''VT_UI4
        '''Available Values:
        '''· 0 – Feature is disabled (default).
        '''· 1 – Feature is enabled.
        '''</summary>
        ''' <remarks></remarks>
        Public Shared ReadOnly EMC_OSD As Guid = New Guid("F5C51906-ED89-4C6E-9C37-A5CCB34F5389")

#End Region 'GUIDS:SHARED

#Region "GUIDS:VC1"

        Public Shared ReadOnly VC1_SkipMode As Guid = New Guid("4B0A9190-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_ErrorResilienceMode As Guid = New Guid("4B0A9191-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_Reserved1 As Guid = New Guid("4B0A9192-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_EnableVideoInfo As Guid = New Guid("4B0A9193-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_InvertFieldOrder As Guid = New Guid("4B0A9194-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputYUY2 As Guid = New Guid("4B0A9195-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputUYVY As Guid = New Guid("4B0A9196-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputYV12 As Guid = New Guid("4B0A9197-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputRGB32 As Guid = New Guid("4B0A9198-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputRGB24 As Guid = New Guid("4B0A9199-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputRGB565 As Guid = New Guid("4B0A919A-A6AE-41f6-BF58-8321D76E1661")
        Public Shared ReadOnly VC1_OutputRGB555 As Guid = New Guid("4B0A919B-A6AE-41f6-BF58-8321D76E1661")

        '  MCVC1VD_SkipMode                     VT_UINT         vc1dec_skip_mode_none
        '                                                       vc1dec_skip_mode_decode_reference
        '                                                       vc1dec_skip_mode_decode_intra
        '                                                       vc1dec_skip_mode_auto
        '  MCVC1VD_ErrorResilienceMode          VT_UINT         vc1dec_error_resilience_mode_proceed_anyway
        '                                                       vc1dec_error_resilience_mode_skip_till_intra
        '                                                       vc1dec_error_resilience_mode_skip_till_intra
        '  MCVC1VD_TreatVUIPictureTimingAsFrameRate
        '                                       VT_BOOL         false, true                         false
        '  MCVC1VD_EnableVideoInfo              VT_BOOL         false, true                         false
        '  MCVC1VD_InvertFieldOrder             VT_BOOL         false, true                         false                   DirectShow filter only
        '  MCVC1VD_OutputYUY2                   VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputUYVY                   VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputYV12                   VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputRGB32                  VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputRGB24                  VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputRGB565                 VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_OutputRGB555                 VT_BOOL         false, true                         true                    DirectShow filter only
        '  MCVC1VD_Deinterlace                  VT_UI4          [0, 3]                              [0]             Selecting output deinterlace mode
        '  MCVC1VD_HQUpsample                   VT_UI4          [0, 1]                              [1]             Enable/disable chroma vertical upsample (only software decode mode)
        '  MCVC1VD_DoubleRate                   VT_UI4          [0, 1]                              [0]             Enable/disable output double rate (avalible only if software decode mode and video is interlaced)
        '  MCVC1VD_FieldsReordering             VT_UI4         [0, 2]                              [0]             Sets the mode of fields reordering
        '  MCVC1VD_FieldsReorderingCondition    VT_UI4 [0, 2]                             [0]             Sets the condition of fields reordering
        '  MCVC1VD_MediaTimeStart               VT_I8           []                                                  Media time of current frame (it may be a byte offset of current frame)
        '  MCVC1VD_MediaTimeStop                VT_I8           []	
        '  MCVC1VD_FramesDisplayed              VT_UI4          []                                  [0]             Count of displayed frames
        '  MCVC1VD_FramesSkipped                VT_UI4          []                                  [0]             Count of skipped frames
        '  MCVC1VD_SYNCHRONIZING                VT_UI4          [0, 2]                              [0]

#End Region 'GUIDS:VC1

#Region "GUIDS:AVC"

        Public Shared ReadOnly AVC_ErrorResilienceMode As Guid = New Guid("23E71B01-F642-4a66-8C90-9749A5C094A4")
        Public Shared ReadOnly AVC_Deblock As Guid = New Guid("8DE7DD1D-F577-4531-ABCD-F15BF5F0CCF7")
        Public Shared ReadOnly AVC_RateMode As Guid = New Guid("AF65371F-7E7D-40a4-A1BA-AAFD10090ACD")

        ' EH264VD_SkipMode                     VT_UI4      [0, 4]              [0]             Selecting skip mode(I,IP,IBP) or obey quality messages
        ' EH264VD_HardwareAcceleration         VT_UI4      [0, 1]              [0]             Switching between software and hardware decode modes
        ' EH264VD_ErrorResilience              VT_UI4      [0, 2]              [0]
        ' EH264VD_Deblock                      VT_UI4      [0, 2]              [0]
        ' EH264VD_Deinterlace                  VT_UI4      [0, 3]              [0]             Selecting output deinterlace mode
        ' EH264VD_HQUpsample                   VT_UI4      [0, 1]              [1]             Enable/disable chroma vertical upsample (only software decode mode)
        ' EH264VD_DoubleRate                   VT_UI4      [0, 1]              [0]             Enable/disable output double rate (avalible only if software decode mode and video is interlaced)
        ' EH264VD_FieldsReordering             VT_UI4      [0, 2]              [0]             Sets the mode of fields reordering
        ' EH264VD_FieldsReorderingCondition    VT_UI4      [0, 2]              [0]             Sets the condition of fields reordering
        ' EH264VD_MediaTimeStart               VT_I8       []                                  Media time of current frame (it may be a byte offset of current frame)
        ' EH264VD_MediaTimeStop                VT_I8       []
        ' EH264VD_FramesDisplayed              VT_UI4      []                  [0]             Amount of displayed frames
        ' EH264VD_FramesSkipped                VT_UI4      []                  [0]             Amount of skipped frames
        ' EH264VD_OSD                          VT_UI4      [0, 1]              [0]             Enable/disable on screen display info (only software decode mode)
        ' EH264VD_SYNCHRONIZING                VT_UI4      [0, 2]              [0]             Synchronization mode (enum SynchronizingMode)
        ' EH264VD_RateMode			           VT_UI4	   [0, 1]   		   [0]			   Interprete VUI timing information as field or frame rate

#End Region 'GUIDS:AVC

#Region "GUIDS:M2V"

        Public Shared ReadOnly M2VD_HQUpsample As Guid = New Guid("9CF1A340-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_IDCTPrecision As Guid = New Guid("9CF1A333-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_PostProcess As Guid = New Guid("9CF1A334-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_Brightness As Guid = New Guid("9CF1A336-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_Resolution As Guid = New Guid("9CF1A331-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_DeinterlaceCondition As Guid = New Guid("37A10D50-E5FC-45b4-A403-3305D6DCDDAB")
        Public Shared ReadOnly M2VD_CCubeDecodeOrder As Guid = New Guid("9CF1A338-E72B-4a6d-BBE8-199595944546")
        Public Shared ReadOnly M2VD_CapturePicture As Guid = New Guid("6118A160-0FF0-43c8-94E4-345AC9E9F362")
        Public Shared ReadOnly M2VD_CaptureUserData As Guid = New Guid("B587EAF4-337E-4e64-92A4-B9F634467A1E")
        Public Shared ReadOnly M2VD_MultiThread As Guid = New Guid("0612C1C6-DEF7-4d01-A0DA-90F426F9B312")
        Public Shared ReadOnly M2VD_Cropping As Guid = New Guid("A4E0959C-4A56-4c41-BF86-51348264D74B")
        Public Shared ReadOnly M2VD_CropTop As Guid = New Guid("CB9F3BF2-5B18-4102-B384-633EBCDBA490")
        Public Shared ReadOnly M2VD_CropLeft As Guid = New Guid("F57B60B9-ED56-4cab-A203-86B771D70C81")
        Public Shared ReadOnly M2VD_CropBottom As Guid = New Guid("CE898DED-D03C-4448-8937-E8599B46E45A")
        Public Shared ReadOnly M2VD_CropRight As Guid = New Guid("2A3DA148-F045-4e2a-BBA0-8B4E23A91E6F")
        Public Shared ReadOnly M2VD_Adding As Guid = New Guid("E66B38D1-B7F9-4f55-83B7-16A9B854E541")
        Public Shared ReadOnly M2VD_AddTop As Guid = New Guid("7E41D6D1-C084-413a-AAA0-E084B7E14F64")
        Public Shared ReadOnly M2VD_AddLeft As Guid = New Guid("D008A768-6091-4d65-835B-D92D7CDBE267")
        Public Shared ReadOnly M2VD_AddBottom As Guid = New Guid("11B9A48E-FC36-4bdd-B4EC-B19E650774E7")
        Public Shared ReadOnly M2VD_AddRight As Guid = New Guid("B6213545-D209-47e8-AB3D-8890828324DF")
        Public Shared ReadOnly M2VD_DimensionsAdjusting As Guid = New Guid("78F02F8B-62D2-4229-A0FB-DDD7AB7A8274")
        Public Shared ReadOnly M2VD_ForceSubpicture As Guid = New Guid("81138942-7534-4700-B520-BE53BAEEE4CC")
        Public Shared ReadOnly M2VD_InternalData As Guid = New Guid("81A015E4-A01F-4745-9355-5731AD89BA5F")
        Public Shared ReadOnly M2VD_ResetStatistics As Guid = New Guid("164966A1-2BFD-4c74-A80E-E5769A219B9F")
        Public Shared ReadOnly M2VD_MediaTimeSource As Guid = New Guid("086B9C83-FFED-4470-96D5-6441D5394181")
        Public Shared ReadOnly M2VD_SetMediaTime As Guid = New Guid("77638653-613F-4afc-AE6C-5AB47C7F0A14")

        ' EM2VD_HardwareAcceleration			VT_UI4		[0, 1]				[0]				Switching between software and hardware decode modes
        ' EM2VD_IDCTPrecision					VT_UI4		[0, 1]				[0]				Selecting precision of inverse DCT procedure
        ' EM2VD_PostProcess					    VT_UI4		[0, 1]				[0]				Enable/disable deblock and dering filtering (only software mode)
        ' EM2VD_Brightness						VT_UI4		[0, 255]			[128]			Brightness level
        ' EM2VD_Contrast						VT_UI4		[0, 255]			[128]			Contrast level
        ' EM2VD_Resolution						VT_UI4		[0, 3]				[0]				Output resolution (only software decode mode)
        ' EM2VD_Deinterlace					    VT_UI4		[0, 3]				[0]				Selecting output deinterlace mode
        ' EM2VD_DeinterlaceCondition			VT_UI4		[0, 1]				[0]				Condition of execute deinterlace
        ' EM2VD_HQUpsample						VT_UI4		[0, 1]				[0]				Enable/disable chroma vertical upsample (only software decode mode)
        ' EM2VD_DoubleRate						VT_UI4		[0, 1]				[0]				Enable/disable output double rate (available only if software decode mode and video is interlaced)
        ' EM2VD_Quality						    VT_UI4		[0, 3]				[0]				Selecting skip mode(I,IP,IBP) or obey quality messages
        ' EM2VD_OSD							    VT_UI4		[0, 1]				[0]				Enable/disable on screen display info (only software decode mode)
        ' EM2VD_CCubeDecodeOrder				VT_UI4		[0, 1]				[0]				Selecting decode order for closed caption format of Cube
        ' EM2VD_FieldsReordering				VT_UI4		[0, 2]				[0]				Sets the mode of fields reordering
        ' EM2VD_FieldsReorderingCondition		VT_UI4		[0, 2]				[0]				Sets the condition of fields reordering
        ' EM2VD_FormatVideoInfo				    VT_UI4		[0, 2]				[1]				Specified output format type of VideoInfo struct - VideoInfo,VideoInfo2 or both (subject to software media types only)
        ' EM2VD_CapturePicture					VT_BYREF	[]					[0]				Get last decoded picture. Call GetValue(...) and send empty VARIANT argument. Reinterpreted pbVal to EM2VD_CapturePictureInfo struct pointer. Free allocated memory throw CoTaskMemFree on the end.
        ' EM2VD_FramesDecoded					VT_UI4		[]					[0]				Count of decoded frames (read only)
        ' EM2VD_FramesSkipped					VT_UI4		[]					[0]				Count of skipped frames (read only)
        ' EM2VD_MultiThread					    VT_UI4		[0, 1]				[1]				Use multi-thread if available (available on multi-processors systems, only software decode mode, only MPEG-2 streams)
        ' EM2VD_ForceSubpicture				    VT_UI4		[0, 1]				[1]				Forced subpicture show
        ' EM2VD_CaptureUserData				    VT_BYREF	[]					[0]				Get user data.
        ' EM2VD_MediaTimeStart					VT_I8		[]      							Media time of current frame (it may be a byte offset of current frame)
        ' EM2VD_MediaTimeStop					VT_I8		[]	
        ' EM2VD_ResetStatistics				    VT_UI4		[0, 1]				[0]				Reset decoder statistics (decoded and skipped frames counter).
        ' EM2VD_ErrorConcealment				VT_UI4		[0, 1]				[1]				Don't show frames with errors.
        ' EM2VD_MediaTimeSource				    VT_UI4		[0, 1]				[0]				Source(input/GOP time code) of media times.

#End Region 'GUIDS:M2V

#Region "GUIDS:DEMUX"

        '[5:00:26 PM] MC - Kirill Erofeev says: HKEY_CURRENT_USER\Software\MainConcept\MainConcept MPEG Demultiplexer

        Public Shared ReadOnly DMX_SUBPICTURE_SUPPORT As Guid = New Guid("9DA8B680-8F52-4a88-806D-EBEB289BBB25")
        Public Shared ReadOnly DMX_INIT_MODE As Guid = New Guid("64B52AAA-C4E5-45be-AD97-82B5E247FFCA")
        Public Shared ReadOnly DMX_INIT_DATA_LENGTH As Guid = New Guid("2BB2DCDC-E4AD-4ea6-B3C7-7D5ACA983968")
        Public Shared ReadOnly DMX_DISABLE_ACCURATENAVIGATION As Guid = New Guid("4CEC0C13-A72D-42e4-90DF-CE8B91D5C5E1")
        Public Shared ReadOnly DMX_DISABLE_NAVIGATION As Guid = New Guid("36CBFB53-6D75-4ccd-B67A-CFA4093E20F3")
        Public Shared ReadOnly DMX_ALWAYS_PTS_NAVIGATION As Guid = New Guid("F02C83CA-29E3-4044-BFD4-59906D358683")
        Public Shared ReadOnly DMX_EXTERNAL_DURATION As Guid = New Guid("AC02E7D0-19D2-4ffe-90E4-DD7285E1515D")
        Public Shared ReadOnly DMX_PTSJUMPTHRESHOLD As Guid = New Guid("FE8522C7-E8E6-44a2-B61A-DF72ECA6DB45")
        Public Shared ReadOnly DMX_STREAMTYPE As Guid = New Guid("59D26E5F-6C2A-4406-B9B8-19B6A1C87EE1")
        Public Shared ReadOnly DMX_BASETIME As Guid = New Guid("84512624-A6CE-4537-B0D7-2D10BA8DE6C1")
        Public Shared ReadOnly EMPGDMX_INDEX_MODE As Guid = New Guid("8CD97D7F-6606-4acd-8732-3F7A84862ED2")
        Public Shared ReadOnly DMX_INDEX_LOAD As Guid = New Guid("B64E15C7-57AB-40d9-BD91-5A7FD029F071")
        Public Shared ReadOnly DMX_INDEX_SAVE As Guid = New Guid("97C096E0-3B2E-4b56-A030-2164D3B940F9")
        Public Shared ReadOnly EMPGDMX_DIRECT_PTS As Guid = New Guid("5DA61BAD-AF60-4005-9EDA-1C68B2AF4C25")
        Public Shared ReadOnly EMPGDMX_DISABLE_INIT_SCAN As Guid = New Guid("25A0589F-C6AE-40B1-998E-FDF145ACA5EC")
        Public Shared ReadOnly EMPGPDMX_STREAMS_DURATION As New Guid("5DA61BAD-AF60-4005-9EDA-1C68B2AF4D25")

        ' EMPGDMX_SUBPICTURE_SUPPORT            VT_I4       0,1                 0	        This parameter enables or disables the subpicture support.
        ' EMPGDMX_INIT_MODE	                    VT_I4       0,1                 0	        This parameter specifies algorithm of the initial stream duration calculation
        ' EMPGDMX_INIT_DATA_LENGTH              VT_UI4      1000-ULONG_MAX      4194304     This parameter specifies the data length (in bytes) that is read and analized during the initial detection of streams.
        ' EMPGDMX_DISABLE_ACCURATENAVIGATION    VT_I4       0,1                 0           When this parameter is equal to 1, the demultiplexer simply calculates the position on the basis of bitrate information and does not perform the stream scanning.
        ' EMPGDMX_DISABLE_NAVIGATION            VT_I4       0,1      		    0           This parameter enables or disables the demultiplexer navigation abilities.
        ' EMPGDMX_ALWAYS_PTS_NAVIGATION         VT_I4       0,1          		0           This parameter sets the PTS navigation as the preferred navigation mode. If the correct navigation using PTS is impossible the navigation mode is switched to the navigation using bitrate.
        ' EMPGDMX_DISABLE_NAVIGATIONINITSCAN    VT_I4	    0,1    	            0	        If this parameter is equal to 1 then the demultiplexer does not try to scan data at the end of stream and the duration is calculated only on basis of bitrate. 
        ' EMPGDMX_EXTERNAL_DURATION             VT_I8	    0-_I64_MAX	        _I64_MAX    This parameter allows specifying the stream duration which will be returned with the IMediaSeeking interface. The value is specified in 100 ns. units.
        ' EMPGDMX_PTSJUMPTHRESHOLD              VT_I8       0-_I64_MAX          15000000    This parameter allows specifying the maximum threshold value for the PTS discontinuity detection. The value is specified in 100 ns. units.      
        ' EMPGDMX_STREAMTYPE                    VT_I4       Read only           0           This parameter contains the type of the detected stream after connecting to the source. Described in E_STREAM_TYPE.
        ' EMPGDMX_BASETIME                      VT_I8       0-_I64_MAX          _I64_MAX    This parameter contains the time of base PTS of stream. The value is specified in 100 ns. units.      
        ' EMPGDMX_INDEX_MODE				    VT_I4		0,1,2,3				0			0 - noindex, 1 - load index if present, 2 - load index always, 3 - as 2 and store index
        ' EMPGDMX_INDEX_LOAD				    VT_BSTR										Force load index - if empty string, then load default name index
        ' EMPGDMX_INDEX_SAVE				    VT_BSTR										Force save index - if empty string, then save default name index

#End Region 'GUIDS:DEMUX

#Region "GUIDS:AUDIO"

        '//////////////////////////////////////////////////////////////////////////
        '// Input stream info
        Public Shared ReadOnly AUD_ISI_Audio_Type As Guid = New Guid("47E44A96-706A-48ec-A083-78F805E7E419")
        '// For MPEG-1,2,2.5 and AC-3
        Public Shared ReadOnly AUD_ISI_Channels_Config As Guid = New Guid("0DC519E1-5F8C-4472-A95D-07304B7C0E6C")
        '//For MPEG - Layers, AAC - Modes
        Public Shared ReadOnly AUD_ISI_Mode As Guid = New Guid("FA0859A2-BDF7-40cc-8ACB-6CAE8C68CB44")
        '// For AC-3
        Public Shared ReadOnly AUD_ISI_Bit_Stream_ID As Guid = New Guid("80E8E2E6-47D1-4f56-A0AF-D289FA0F9B04")
        '//  For LPCM, AES3
        Public Shared ReadOnly AUD_ISI_Quantization As Guid = New Guid("ACB2B424-9806-4b6e-8900-C37C1A7C9B15")
        '// For LPCM, AES3, PL2, AAC
        Public Shared ReadOnly AUD_ISI_Number_Channels As Guid = New Guid("9CB180BE-272B-42ad-9EB6-5725FBC9EB59")
        Public Shared ReadOnly AUD_ISI_Sample_Rate As Guid = New Guid("5DD57963-F8E8-442c-B5B7-06EDB4E5BF77")
        Public Shared ReadOnly AUD_ISI_Bit_Rate As Guid = New Guid("2AC261C6-F5CF-4380-AB86-147A9571E4EA")

        '//////////////////////////////////////////////////////////////////////////
        '// General decoder settings
        Public Shared ReadOnly AUD_Bit_Per_Sample As Guid = New Guid("14C401D1-FBC0-40af-8BA4-D2C5E3B78192")
        Public Shared ReadOnly AUD_Enable_Input_Pro_Logic_2 As Guid = New Guid("3918F528-87E4-4edc-814A-C079277E5D1C")
        Public Shared ReadOnly AUD_Mute As Guid = New Guid("0B8AD217-9CBA-4642-A6EB-498F1E072DFE")
        Public Shared ReadOnly AUD_Enable_SPDIF As Guid = New Guid("3B41000F-4BD8-4627-A69F-FDE644DF42C7")

        '//////////////////////////////////////////////////////////////////////////
        '// MPEG 1,2,2.5, AAC, LPCM, AES3 decoder settings
        Public Shared ReadOnly AUD_MAL_Output_Channels_Config As Guid = New Guid("B46D861B-FD25-48c1-AFA2-17F2227D4424")

        '//////////////////////////////////////////////////////////////////////////
        '// MPEG 1,2,2.5
        Public Shared ReadOnly AUD_MPEG_Channels_Config As Guid = New Guid("7A26EEDE-AB15-44a7-B4C6-D7CC4BFBBAA1")

        '//////////////////////////////////////////////////////////////////////////
        '// AC-3 decoder settings
        Public Shared ReadOnly AUD_AC3DEC_Stereo_Mode As Guid = New Guid("F52D9B8C-C0C5-434a-93A1-E83335625DD6")
        Public Shared ReadOnly AUD_AC3DEC_DualMono_Mode As Guid = New Guid("6EDF7B39-77A7-4824-968B-680674E5F6E8")
        Public Shared ReadOnly AUD_AC3DEC_Compression_Mode As Guid = New Guid("4FD233E9-ED26-4a74-AF6B-19935893E942")
        Public Shared ReadOnly AUD_AC3DEC_Channels_Config As Guid = New Guid("A344E52F-C7E2-4ff8-89E3-04C80F6E9014")
        Public Shared ReadOnly AUD_AC3DEC_LFE_Channel As Guid = New Guid("99ACB5CE-893F-4dfe-8675-760B35809521")
        Public Shared ReadOnly AUD_AC3DEC_Karaoke_Mode = ("DD4EF209-0FC2-4d7f-B8B6-37FA308DCD6B")
        Public Shared ReadOnly AUD_AC3DEC_Panning_Mode = ("BC6C865C-7D04-45e3-A4F7-9A462F6DE872")
        Public Shared ReadOnly AUD_AC3DEC_PCM_Scale_Factor As Guid = New Guid("58B45A5C-D4BD-458d-BB38-1F2692A87772")
        Public Shared ReadOnly AUD_AC3DEC_DCR_Scale_Factor_High As Guid = New Guid("05845B36-A005-4f7c-AF65-8E369A34A13B")
        Public Shared ReadOnly AUD_AC3DEC_DCR_Scale_Factor_Low As Guid = New Guid("29210F40-1858-4281-A422-FEBB01D21CEF")
        Public Shared ReadOnly AUD_AC3DEC_AC3_SPDIF_Output As Guid = New Guid("A1884CC1-7CF7-437d-9655-DF65CC4BA082")
        Public Shared ReadOnly AUD_AC3DEC_ProLogic2 As Guid = New Guid("A03352D5-A1A3-474b-BED0-9E6E5A293BC0")

        '//////////////////////////////////////////////////////////////////////////
        '// Pro Logic II decoder settings
        Public Shared ReadOnly AUD_PL2DEC_Decode_Mode As Guid = New Guid("937CA545-453A-4143-9B92-0EB6588A3DC4")
        Public Shared ReadOnly AUD_PL2DEC_Dimension_Mode As Guid = New Guid("C539CAC9-F4F0-4590-A8E8-CEEB935F60B4")
        Public Shared ReadOnly AUD_PL2DEC_Center_Width_Control As Guid = New Guid("41393AE3-D300-4550-B546-FF9362923A7F")
        Public Shared ReadOnly AUD_PL2DEC_Channel_Config As Guid = New Guid("942F5C92-E761-448f-9886-206DD13BF114")
        Public Shared ReadOnly AUD_PL2DEC_Auto_Balance As Guid = New Guid("A955F69D-BB85-4e79-BDC5-BEB051265239")
        Public Shared ReadOnly AUD_PL2DEC_Surround_Shelf_Filter As Guid = New Guid("E8B6EAEC-D19E-480f-8408-086FFE4995F0")
        Public Shared ReadOnly AUD_PL2DEC_Panorama As Guid = New Guid("E6381AB3-B812-46d0-A31F-76A01973F64C")
        Public Shared ReadOnly AUD_PL2DEC_Rs_Polarity_Inversion As Guid = New Guid("C19E3F2C-5183-43d4-93D5-9C0F97A33CED")
        Public Shared ReadOnly AUD_PL2DEC_PCM_Scale_Factor As Guid = New Guid("CC0848B8-EFBE-4be6-B3FD-2D4B96234904")

        '  Input stream info:
        '	EL2AD_ISI_Audio_Type					VT_I4		[0;7]				0			Input audio type
        '	EL2AD_ISI_Channels_Config				VT_I4		[0;20]				0			Config channels for AC-3 and MPEG-1,2,2.5
        '	EL2AD_ISI_Mode						    VT_I4		[0;20]				0			Layer for MPEG-1,2,2.5 and mode for AAC
        '	EL2AD_ISI_Quantization				    VT_I4		16,20,24			0			LPCM quantization
        '	EL2AD_ISI_Number_Channels				VT_I4		   -				0			Number of channels in source stream
        '
        '		General:
        '	ELAUDEC_Bit_Per_Sample					VT_I4		[0:1]				0			Output bits per sample
        '  ELAUDEC_Mute							    VT_I4		[0:1]				0			Mute output
        '
        '
        '		MPEG:
        '	EL2AD_MAL_Output_Channels_Config		VT_I4		[0:2]				0			Output channels config
        '
        '		MPEG:
        '	EL2AD_MPEG_Channels_Config			    VT_I4		[0:2]				2			MPEG channels config for reproduction (default MPEG_CHANNELS_CONFIG_Both)
        '

#End Region 'GUIDS:AUDIO

#Region "GUIDS:SCALER"

        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        '	GUID						Value Type	Available range		Default Val		Note
        '	ImScaler_CropImage			VT_UINT		1,0			        0			    1 - crop is needed    
        '	ImScaler_ResizeImage		VT_UINT		1,0			        0		        1 - resize is needed    
        '	ImScaler_ResizeType			VT_UI1		[0 - 3]				Mitchel			0 - BSpline simple, 1 - Notch, 2 - CatmullRom spline, 3 - Mitchell-Netravali
        '	ImScaler_AppendixMode		VT_UINT		2,1,0			    0			    1 - addition stripes are needed   0 - do not add 2 - Add black stripes to keep picture proportions
        '	ImScaler_AppendixStripesType VT_UINT	2,1,0			    0			    0 - Add horizontal stripes(at top and bottom), 1 - Add verical stripes (at left and right), 2- Add vertical and horisontal stripes			
        '	ImScaler_CropImageXLeft		VT_I4							0				Left top X coordinate of crop rectangle (in pixels)
        '	ImScaler_CropImageYTop		VT_I4							0				Left top Y coordinate of crop rectangle (in pixels)
        '	ImScaler_CropImageXRight	VT_I4							0				Right bottom X coordinate of crop rectangle (in pixels)
        '	ImScaler_CropImageYBot		VT_I4							0				Right bottom Y coordinate of crop rectangle (in pixels)
        '	ImScaler_DestHeight			VT_I4		[0 - 2000]			0				Destinatiom height of video (if resize is needed) 			
        '	ImScaler_DestWidth			VT_I4		[0 - 2000]			0				Destinatiom width of video (if resize is needed) 			
        '	ImScaler_StripesSizeValue	VT_UINT							0				Size of additional stripes (in pixels)
        '	ImScaler_StripesSizeValue2  VT_UINT						    0				Size of additional stripes (in pixels), used if both vertical and horizontal stripes exist
        '	ImScaler_PictureType		VT_UINT		[0 - 2]				AutoDetecting	Type of input Video Interlaced / Progressive or auto detecting(see "PictureType" enum)
        '	ImScaler_SourceWidth		VT_I4											Width of input frame 
        '	ImScaler_SourceHeight		VT_I4											Height of input frame
        '	ImScaler_Align				VT_UINT											Align value (read only)
        '	ImScaler_MaxResizeWidth		VT_UINT											Maximum resize size by width value (read only)
        '	ImScaler_MaxResizeHeight	VT_UINT											Maximum resize size by height value (read only)
        '	ImScaler_MinResizeWidth		VT_UINT											Minimum resize size by width value (read only)
        '	ImScaler_MinResizeHeight	VT_UINT											Minimum resize size by height value (read only)
        '	ImScaler_FilterState        VT_UINT											Filter state (read only)
        '	ImScaler_SetAspectRatio		VT_BOOL		TRUE,FALSE			FALSE			TRUE - set current picture aspect ratio 
        '	ImScaler_BindCrop			VT_BOOL		TRUE,FALSE			FALSE			TRUE - bind crop values to source picture size
        '	ImScaler_CropDefinitionMode	VT_UINT		0,1,2				0				Crop parameters definition mode
        '   ImScaler_UseAspectRatio	    VT_BOOL		TRUE,FALSE			FALSE			TRUE - Use aspect ratio for resizing with "Keep picture proportions" mode
        '//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        Public Enum PictureType
            Interlaced = 0                      ' type of input video is interlaced
            Progressive = Interlaced + 1        ' type of input video is progressive
            AutoDetecting = Progressive + 1     ' auto detecting input video type
        End Enum

        Public Enum ResizeType
            eSpline = 0
            eNotch = eSpline + 1
            eCatmullRom = eNotch + 1
            eMitchel = eCatmullRom + 1
        End Enum

        Public Shared ReadOnly ImScaler_CropImage As Guid = New Guid("EDD0D20C-D35E-4da8-B9F9-E6F07E0FE861")
        Public Shared ReadOnly ImScaler_ResizeImage As Guid = New Guid("A68F13F1-CC33-436a-BCBF-0A5C95EBE27F")
        Public Shared ReadOnly ImScaler_ResizeType As Guid = New Guid("B102C6E1-DCF7-441b-8890-B884B0949902")
        Public Shared ReadOnly ImScaler_AppendixMode As Guid = New Guid("E0CAC972-450E-44ae-9825-BC6E2448CD67")
        Public Shared ReadOnly ImScaler_AppendixStripesType As Guid = New Guid("B23C99E0-D86F-4d50-85E4-7A0D6F5C9C61")
        Public Shared ReadOnly ImScaler_CropImageXLeft As Guid = New Guid("57F15B02-E645-4d90-8834-31AEEF960A07")
        Public Shared ReadOnly ImScaler_CropImageYTop As Guid = New Guid("C2F3CF68-417F-413b-926C-A09D67AAB84D")
        Public Shared ReadOnly ImScaler_CropImageXRight As Guid = New Guid("C038AA00-17DC-4c5b-A3FE-F31B0F68A75A")
        Public Shared ReadOnly ImScaler_CropImageYBot As Guid = New Guid("9DC1C8D4-0870-40bc-B78B-F00190E03112")
        Public Shared ReadOnly ImScaler_DestHeight As Guid = New Guid("32A5E96D-2AC3-4d46-BF70-431BEE62DF35")
        Public Shared ReadOnly ImScaler_DestWidth As Guid = New Guid("04B86B7E-C04B-448f-BFF5-2A50F1945ACD")
        Public Shared ReadOnly ImScaler_StripesSizeValue As Guid = New Guid("BFC35D7A-B763-439a-89CE-4E20F6192B99")
        Public Shared ReadOnly ImScaler_StripesSizeValue2 As Guid = New Guid("E9F189CB-5ACB-4916-94CA-498D2A92E9E6")
        Public Shared ReadOnly ImScaler_PictureType As Guid = New Guid("2E1DD59C-78B4-4f17-A0BA-41DEC7756EE1")
        Public Shared ReadOnly ImScaler_FilterState As Guid = New Guid("0F414B44-5B63-43cd-8813-4B34A9A84C67")
        Public Shared ReadOnly ImScaler_SourceWidth As Guid = New Guid("E35D983E-2004-45e2-A5FB-59AE94FCC258")
        Public Shared ReadOnly ImScaler_SourceHeight As Guid = New Guid("ECCA3864-E79E-4e77-A46E-D823CA882309")
        Public Shared ReadOnly ImScaler_Align As Guid = New Guid("8803E341-A76D-4874-B5CC-711C92BC385D")
        Public Shared ReadOnly ImScaler_MaxResizeWidth As Guid = New Guid("A7767F9E-F2DD-4b51-BFDE-626852DD8DA3")
        Public Shared ReadOnly ImScaler_MaxResizeHeight As Guid = New Guid("97A2B1E2-EB2E-469c-A3BF-61E287578E38")
        Public Shared ReadOnly ImScaler_MinResizeWidth As Guid = New Guid("4FE96158-A190-44e1-BF99-599E292FD999")
        Public Shared ReadOnly ImScaler_MinResizeHeight As Guid = New Guid("69189676-63D0-4b5c-8BEB-FAD9A6775B6B")
        Public Shared ReadOnly ImScaler_SetAspectRatio As Guid = New Guid("5CE39F30-35B9-4953-A23A-0FA6B1F356D1")
        Public Shared ReadOnly ImScaler_BindCrop As Guid = New Guid("B14F5971-1EA7-46e8-9B6E-42B0F3E37F7E")
        Public Shared ReadOnly ImScaler_CropDefinitionMode As Guid = New Guid("1F9739D4-EAC4-40e2-9BF5-97A618073785")
        Public Shared ReadOnly ImScaler_SetAspect4_3 As Guid = New Guid("D70EC5EF-C271-4728-A5BF-C0B6BD4F6D3E")
        Public Shared ReadOnly ImScaler_SetAspect16_9 As Guid = New Guid("6E7CFA3C-7395-4df9-A3D2-7E1DBE442B42")
        Public Shared ReadOnly ImScaler_UseAspectRatio As Guid = New Guid("E0248902-C2ED-4633-B3F5-3CBC5BFC80F7")

#End Region 'GUIDS:SCALER

#End Region 'GUIDS

#Region "ENUMS"

#Region "ENUMS:SHARED"

        Public Enum eState As Integer
            State_Off = 0
            State_On = 1
        End Enum

        Public Enum eDeinterlaceCondition As Integer
            DeinterlaceCondition_Always = 0     ' Execute deinterlace regardless of picture structute
            DeinterlaceCondition_Interlace = 1      ' Execute deinterlace only progressive flag is "false"
            DeinterlaceCondition_Progressive = 2    ' Execute deinterlace only progressive flag is "true"
        End Enum

        Public Enum eDeinterlaceMode As Integer
            Deinterlace_Weave = 0           ' No deinterlace
            Deinterlace_VerticalFilter = 1  ' Interpolation with using info from both fields
            Deinterlace_FieldInterpolation = 2      ' Stretch one of fields
            Deinterlace_VMR = 3
            Deinterlace_Auto = 4
        End Enum

        Public Enum eMediaTimeSource As Integer
            InputMediaTime = 0
            GOPTimeCode = 1
        End Enum

        Public Enum eDimensionsAdjustingMode As Integer
            DimensionsAdjusting_Off = 0
            DimensionsAdjusting_720x486 = 1
            DimensionsAdjusting_1280x720 = 2
            DimensionsAdjusting_1920x1080 = 3
        End Enum

        Public Enum eErrorConcealmentMode As Integer
            ErrorConcealment_Off = 0
            ErrorConcealment_NotShowFramesWithErrors = 1
            ErrorConcealment_Temporal = 2
            ErrorConcealment_Spatial = 3
            ErrorConcealment_Smart = 4
        End Enum

        Public Enum eFormatVideoInfo As Integer
            FormatVideoInfo_1 = 0
            FormatVideoInfo_2 = 1
            FormatVideoInfo_Both = 2
        End Enum

        Public Enum eFieldReorderingConditionMode As Integer
            FieldReorderingCondition_Always = 0
            FieldReorderingCondition_TopFirst_True = 1
            FieldReorderingCondition_TopFirst_False = 2
        End Enum

        Public Enum eFieldReorderingMode As Integer
            FieldReordering_Off = 0
            FieldReordering_FlagsInverting = 1
            FieldReordering_FieldsInverting = 2
            FieldReordering_Auto = 3
        End Enum

        Public Enum eDecodeOrderMode As Integer
            DecodeOrder_Stream = 0
            DecodeOrder_Display = 1
        End Enum

        Public Enum eIDCTPrecisionMode As Integer
            IDCTPrecision_Integer = 0
            IDCTPrecision_Float = 1
        End Enum

        Public Enum eResolutionMode As Integer
            Resolution_Full = 0
            Resolution_HalfHorizontal = 1
            Resolution_HalfVertical = 2
            Resolution_Quarter = 3
        End Enum

        Public Enum eEMC_Quality As Integer
            ObeyQualityMessages = 0
            I_Frame_Only = 1
            IP_Frames = 2
            IPB_Frames = 3
        End Enum

        Public Enum eMCVC1VD_SkipMode As Integer
            auto
            none
            decode_reference
            decode_intra
        End Enum

        Public Enum eEM2VD_Synchronizing As Integer
            PTS = 0
            IgnorePTS_NotRef = 1
            IgnorePTS_All = 2
            DirectTimestamps = 3
        End Enum

        Public Enum eErrorResilienceMode_VC1 As Integer
            Error_resilience_mode_proceed_anyway = 0
            Error_resilience_mode_skip_till_intra
        End Enum

        Public Enum eErrorResilienceMode_AVC As Integer
            ErrorResilienceMode_Skip_till_Intra = 0
            ErrorResilienceMode_Skip_till_IDR
            ErrorResilienceMode_Decode_Anyway
        End Enum

        Public Enum eDeblockMode As Integer
            DeblockMode_Default = 0
            DeblockMode_OnlyRef
            DeblockMode_Off
        End Enum

        Public Enum eRateMode As Integer
            RateMode_Field = 0
            RateMode_Frame = 1
        End Enum

        Public Enum eStreamType As Integer
            StreamType_UNKNOWN = 0
            StreamType_SYSTEM = 1
            StreamType_PROGRAM = 2
            StreamType_TRANSPORT = 3
            StreamType_MPEG2V = 4
            StreamType_PVA = 5
            StreamType_MPEG2A = 6
            StreamType_AC3 = 7
            StreamType_H264 = 8
            StreamType_MPEG4V = 9
            StreamType_VC1 = 10
            StreamType_H263 = 11
            StreamType_MP4 = 12
            StreamType_AAC = 13
            StreamType_MPEG1V = 14
            StreamType_MPEG1A = 15
            StreamType_DIV3 = 16
            StreamType_DIV4 = 17
            StreamType_DIV5 = 18
            StreamType_VSSH = 19
            StreamType_PCM = 20
            StreamType_DVDSUB = 21
        End Enum

#End Region 'ENUMS:SHARED

#Region "ENUMS:AUDIO"

        '//////////////////////////////////////////////////////////////////////////
        '// General settings
        '//////////////////////////////////////////////////////////////////////////

        Public Enum eBITS_PER_SAMPLE
            BPS_16 = 0
            BPS_32 = 1
        End Enum

        Public Enum eeEnabled
            DISABLE = 0
            ENABLE = 1
        End Enum

        Public Enum eOffOn
            OFF = 0
            [ON] = 1
        End Enum


        '//////////////////////////////////////////////////////////////////////////
        '// Input stream info (ISI)
        '//////////////////////////////////////////////////////////////////////////

        '// Input audio types
        Public Enum eAUDIO_TYPE
            AUDIOTYPE_Unknow = 0
            AUDIOTYPE_DolbyAC3 = 1
            AUDIOTYPE_MPEG1 = 2
            AUDIOTYPE_MPEG2 = 3
            AUDIOTYPE_MPEG25 = 4
            AUDIOTYPE_LPCM = 5
            AUDIOTYPE_PLII = 6
            AUDIOTYPE_AAC = 7
            AUDIOTYPE_AES3 = 8
        End Enum

        '// Layers for MPEG and modes for AAC
        Public Enum eMODE
            MODE_Unknow = 0
            MODEMPEG_Layer1 = 1
            MODEMPEG_Layer2 = 2
            MODEMPEG_Layer3 = 3

            MODEAAC_Main = 4
            MODEAAC_LC = 5
            MODEAAC_SSR = 6
            MODEAAC_LTP = 7
            MODEAAC_Scalable = 8
            MODEAAC_TwinVQ = 9
            MODEAAC_CELP = 10
            MODEAAC_HVXC = 11
            MODEAAC_TTSI = 12
            MODEAAC_Main_Synthetic = 13
            MODEAAC_Wavetable_Synthesis = 14
            MODEAAC_General_MIDI = 15
            MODEAAC_Algorithmic_Syntesis_and_Audio_FX = 16

            MODEAAC_ER_LC = 17
            MODEAAC_ER_LTP = 17
            MODEAAC_ER_Scalable = 17
            MODEAAC_ER_TwinEQ = 17
            MODEAAC_ER_BSAC = 17
            MODEAAC_ER_LD = 18
            MODEAAC_ER_HVXC = 19
            MODEAAC_ER_CELP = 18
            MODEAAC_ER_HILN = 19
            MODEAAC_ER_Parametric = 20
        End Enum

        '// Channels mode for MPEG and AC-3
        Public Enum eCHANNELS_MODE
            CHANMODE_Unknow = 0
            CHANMODEAC3_1_1_Dual = 1
            CHANMODEAC3_1_1_Dual_LFE = 2
            CHANMODEAC3_1_0_Center = 3
            CHANMODEAC3_1_0_Center_LFE = 4
            CHANMODEAC3_2_0_Stereo = 5
            CHANMODEAC3_2_0_Stereo_LFE = 6
            CHANMODEAC3_3_0_Front = 7
            CHANMODEAC3_3_0_Front_LFE = 8
            CHANMODEAC3_2_1_Surround = 9
            CHANMODEAC3_2_1_Surround_LFE = 10
            CHANMODEAC3_3_1_Surround = 11
            CHANMODEAC3_3_1_Surround_LFE = 12
            CHANMODEAC3_2_2_Quadro = 13
            CHANMODEAC3_2_2_Quadro_LFE = 14
            CHANMODEAC3_3_2_5Channels = 15
            CHANMODEAC3_3_2_5Channels_LFE = 16
            CHANMODEMPEG_SingleChannel = 17
            CHANMODEMPEG_DualChannel = 18
            CHANMODEMPEG_JointStereo = 19
            CHANMODEMPEG_Stereo = 20
        End Enum


        '//////////////////////////////////////////////////////////////////////////
        '// AC-3 decoder settings
        '//////////////////////////////////////////////////////////////////////////

        Public Enum eAC3DEC_STEREO_MODE
            AC3DEC_STEREO_MODE_Auto = 0
            AC3DEC_STEREO_MODE_LtRt = 1
            AC3DEC_STEREO_MODE_LoRo = 2
        End Enum

        Public Enum eAC3DEC_DUALMONO_MODE
            AC3DEC_DUALMONO_MODE_Stereo = 0
            AC3DEC_DUALMONO_MODE_Left_mono = 1
            AC3DEC_DUALMONO_MODE_Right_mono = 2
            AC3DEC_DUALMONO_MODE_Mixed_mono = 3
        End Enum

        Public Enum eAC3DEC_COMPRESSION_MODE
            AC3DEC_COMPRESSION_MODE_Custom_analog = 0
            AC3DEC_COMPRESSION_MODE_Custom_digital = 1
            AC3DEC_COMPRESSION_MODE_Line = 2
            AC3DEC_COMPRESSION_MODE_RF = 3
        End Enum

        Public Enum eAC3DEC_CHANNEL_CONFIG
            AC3DEC_CHANNEL_CONFIG_C = 0   '// 1/0 Center
            AC3DEC_CHANNEL_CONFIG_L_R = 1   '// 2/0 Stereo
            AC3DEC_CHANNEL_CONFIG_L_R_C = 2  '// 3/0 Front
            AC3DEC_CHANNEL_CONFIG_L_R_Ls = 3  '// 2/1 Surround
            AC3DEC_CHANNEL_CONFIG_L_R_C_Ls = 4  '// 3/1 Surround
            AC3DEC_CHANNEL_CONFIG_L_R_Ls_Rs = 5  '// 2/2 Quadro
            AC3DEC_CHANNEL_CONFIG_L_R_C_Ls_Rs = 6   '// 3/2 5 Channels
        End Enum

        Public Enum eAC3DEC_KARAOKE_MODE
            AC3DEC_KARAOKE_MODE_No_vocal = 0
            AC3DEC_KARAOKE_MODE_Left_vocal = 1
            AC3DEC_KARAOKE_MODE_Right_vocal = 2
            AC3DEC_KARAOKE_MODE_Both_vocal = 3
        End Enum

        Public Enum eAC3DEC_PANNING_MODE
            AC3DEC_PANNING_MODE_Center = 0
            AC3DEC_PANNING_MODE_Left = 1
            AC3DEC_PANNING_MODE_Right = 2
        End Enum


        '//////////////////////////////////////////////////////////////////////////
        '// Pro Logic II decoder settings
        '//////////////////////////////////////////////////////////////////////////

        Public Enum ePL2DEC_DECODE_MODE
            PL2DEC_DECODE_MODE_Pro_Logic_emulation = 0
            PL2DEC_DECODE_MODE_Virtual_compatible = 1
            PL2DEC_DECODE_MODE_Music = 2
            PL2DEC_DECODE_MODE_Movie = 3
            PL2DEC_DECODE_MODE_Matrix = 4
            PL2DEC_DECODE_MODE_Custom = 5
        End Enum

        Public Enum ePL2DEC_DIMENSION_SETTINGS
            PL2DEC_DIMENSION_SETTINGS_minus_3 = 0
            PL2DEC_DIMENSION_SETTINGS_minus_2 = 1
            PL2DEC_DIMENSION_SETTINGS_minus_1 = 2
            PL2DEC_DIMENSION_SETTINGS_neutral = 3
            PL2DEC_DIMENSION_SETTINGS_plus_1 = 4
            PL2DEC_DIMENSION_SETTINGS_plus_2 = 5
            PL2DEC_DIMENSION_SETTINGS_plus_3 = 6
        End Enum

        Public Enum ePL2DEC_CENTER_WIDHT_CONTROL
            PL2DEC_CENTER_WIDHT_CONTROL_0_deg = 0
            PL2DEC_CENTER_WIDHT_CONTROL_20_8_deg = 1
            PL2DEC_CENTER_WIDHT_CONTROL_28_deg = 2
            PL2DEC_CENTER_WIDHT_CONTROL_36_deg = 3
            PL2DEC_CENTER_WIDHT_CONTROL_54_deg = 4
            PL2DEC_CENTER_WIDHT_CONTROL_62_deg = 5
            PL2DEC_CENTER_WIDHT_CONTROL_69_2_deg = 6
            PL2DEC_CENTER_WIDHT_CONTROL_90_deg = 7
        End Enum

        Public Enum ePL2DEC_CHANNELS_CONFIG
            PL2DEC_CHANNELS_CONFIG_L_R_C = 0
            PL2DEC_CHANNELS_CONFIG_L_R_C_Ls = 1
            PL2DEC_CHANNELS_CONFIG_L_R_Ls_Rs = 2
            PL2DEC_CHANNELS_CONFIG_L_R_C_Ls_Rs = 3
        End Enum


        '//////////////////////////////////////////////////////////////////////////
        '// MPEGAACLPCMAES3
        '//////////////////////////////////////////////////////////////////////////

        Public Enum eMAL_CHANNELS_CONFIG
            MAL_CHANNELS_CONFIG_Auto = 0
            MAL_CHANNELS_CONFIG_Mono = 1
            MAL_CHANNELS_CONFIG_Stereo = 2
        End Enum


        '//////////////////////////////////////////////////////////////////////////
        '// MPEG
        '//////////////////////////////////////////////////////////////////////////

        Public Enum eMPEG_CHANNELS_CONFIG
            MPEG_CHANNELS_CONFIG_First = 0
            MPEG_CHANNELS_CONFIG_Second = 1
            MPEG_CHANNELS_CONFIG_Both = 2
        End Enum

        Public Enum eLTMode
            LTMode_not_activate = 0
            LTMode_demo = 1
            LTMode_evaluation = 2
            LTMode_evaluation_expired = 3
            LTMode_full = 4
        End Enum

#End Region 'ENUMS:AUDIO

#End Region 'ENUMS

#Region "STRUCTURES"

        Public Structure EM2VD_CapturePictureInfo
            Public pBuffer As IntPtr 'VOID *
            Public szBuffer As System.Drawing.Size 'SIZE_T 
        End Structure

        Public Structure EM2VD_BufferInfo
            Public pBuffer As IntPtr 'VOID * 
            Public szBuffer As System.Drawing.Size 'SIZE_T
        End Structure

#End Region 'STRUCTURES

    End Class

End Namespace
