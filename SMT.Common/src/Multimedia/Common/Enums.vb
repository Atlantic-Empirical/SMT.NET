
Namespace Multimedia.Enums

    Public Enum eVideoStandard
        NTSC = 0
        PAL = 1
    End Enum

    Public Enum eVideoResolution
        _720x480
        _720x486
        _720x576
        _1280x720
        _1920x1080
    End Enum

    Public Enum eScalingMode
        Native
        Native_ScaleToAR
        Maximize_NativeAR
        Maximize_ScaleToAR
    End Enum

    Public Enum eTransportControlTypes
        Play
        Pause
        [Stop]
        FastForward
        Rewind
    End Enum

    Public Enum ePlayState
        Init
        Playing
        Paused
        Stopped
        FastForwarding
        Rewinding
        SlowRewinding
        SlowForwarding
        FrameStepping
        SystemJP
        Ejecting
        ProjectJP
        ColorBars
        FilePlayback
        VariSpeed
        DoubleStopped
        ABLoop
    End Enum

    Public Enum eShuttleSpeed As Byte
        OneX = 1
        TwoX = 2
        FourX = 4
        EightX = 8
        SixteenX = 16
        ThirtyTwoX = 32
        Not_Specified = 255
    End Enum

    Public Enum eViewerSize
        NTSC_Half_360x243
        NTSC_720x486
        NTSC_Anamorphic_853x486
        PAL_Half_360x288
        PAL_720x576
        PAL_Anamorphic_853x576
        SubHD_Half_640x360
        SubHD_1280x720
        HD_Quarter_480x270
        HD_Half_960x540
        HD_1920x1080
        Fullscreen
    End Enum

    'Public Enum eBuildGraphResult
    '    Success
    '    FilterCheckFailed
    '    UnspecifiedError
    'End Enum

    Public Enum ePlayerType As Byte
        DVD
        M2TS
        M2V
        MPG
        AVC
        VC1
        AVI
        WMV
        AC3DTS
        PCM
        M1V
        MPA
        MOV
        NOT_SPECIFIED = 255
    End Enum

    Public Enum eAVMode
        DesktopVMR
        Intensity
        Decklink
    End Enum

    Public Enum eAlphabet
        A
        B
        C
        D
        E
        F
        G
        H
        I
        J
        K
        L
        M
        N
        O
        P
        Q
        R
        S
        T
        U
        V
        W
        X
        Y
        Z
    End Enum
    
    Public Enum eForceDecoderPALConnection
        eFalse = 0
        eTrue = 1
    End Enum

    Public Enum eFrameGrabContent
        Video_Only = 0
        Subpicture_Only = 1
        Closed_Caption_Only = 2
        Full_Mix = 3
        Video_and_Subpicture = 4
        MultiFrame = 5
    End Enum

    Public Enum eFrameGrabType
        BMP
        JPEG
        GIF
        PNG
        TIF
    End Enum

    Public Enum eLetterboxColors
        DarkBlack
        LightBlack
        Blue
        Green
        Red
    End Enum

    Public Enum eActionTitleSafeColors
        Grey
        White
        DarkGrey
        DarkBlue
        DarkRed
    End Enum

    Public Enum eCountries
        AFGHANISTAN
        ALBANIA
        ALGERIA
        AMERICAN_SAMOA
        ANDORRA
        ANGOLA
        ANGUILLA
        ANTARCTICA
        ANTIGUA_AND_BARBUDA
        ARGENTINA
        ARMENIA
        ARUBA
        AUSTRALIA
        AUSTRIA
        AZERBAIJAN
        BAHAMAS
        BAHRAIN
        BANGLADESH
        BARBADOS
        BELARUS
        BELGIUM
        BELIZE
        BENIN
        BERMUDA
        BHUTAN
        BOLIVIA
        BOSNIA_AND_HERZEGOVINA
        BOTSWANA
        BOUVET_ISLAND
        BRAZIL
        BRITISH_INDIAN_OCEAN_TERRITORY
        BRUNEI_DARUSSALAM
        BULGARIA
        BURKINA_FASO
        BURUNDI
        CAMBODIA
        CAMEROON
        CANADA
        CAPE_VERDE
        CAYMAN_ISLANDS
        CENTRAL_AFRICAN_REPUBLIC
        CHAD
        CHILE
        CHINA
        CHRISTMAS_ISLAND
        COCOS_KEELING_ISLANDS
        COLOMBIA
        COMOROS
        CONGO
        COOK_ISLANDS
        COSTA_RICA
        COTE_DIVOIRE
        CROATIA_loca_name_Hrvatska
        CUBA
        CYPRUS
        CZECH_REPUBLIC
        DENMARK
        DJIBOUTI
        DOMINICA
        DOMINICAN_REPUBLIC
        EAST_TIMOR
        ECUADOR
        EGYPT
        EL_SALVADOR
        EQUATORIAL_GUINEA
        ERITREA
        ESTONIA
        ETHIOPIA
        FALKLAND_ISLANDS_MALVINAS
        FAROE_ISLANDS
        FIJI
        FINLAND
        FRANCE
        FRENCH_GUIANA
        FRENCH_POLYNESIA
        FRENCH_SOUTHERN_TERRITORIES
        GABON
        GAMBIA
        GEORGIA
        GERMANY
        GHANA
        GIBRALTAR
        GREECE
        GREENLAND
        GRENADA
        GUADELOUPE
        GUAM
        GUATEMALA
        GUINEA
        GUINEA_BISSAU
        GUYANA
        HAITI
        HEARD_ISLAND_MCDONALD_ISLANDS
        HONDURAS
        HONG_KONG
        HUNGARY
        ICELAND
        INDIA
        INDONESIA
        IRAQ
        IRELAND
        ISRAEL
        ITALY
        JAMAICA
        JAPAN
        JORDAN
        KAZAKHSTAN
        KENYA
        KIRIBATI
        KOREA_DEMOCRATIC_PEOPLES_REPUBLIC_OF
        KOREA_REPUBLIC_OF
        KUWAIT
        KYRGYZSTAN
        LAOS_PEOPLES_DEMOCRATIC_REPUBLIC
        LATVIA
        LEBANON
        LESOTHO
        LIBERIA
        LIBYAN_ARAB_JAMAHIRIYA
        LIECHTENSTEIN
        LITHUANIA
        LUXEMBOURG
        MACAU
        MACEDONIA
        MADAGASCAR
        MALAWI
        MALAYSIA
        MALDIVES
        MALI
        MALTA
        MARSHALL_ISLANDS
        MARTINIQUE
        MAURITANIA
        MAURITIUS
        MAYOTTE
        MEXICO
        MICRONESIA_FEDERATED_STATES_OF
        MOLDOVA_REPUBLIC_OF
        MONACO
        MONGOLIA
        MONTSERRAT
        MOROCCO
        MOZAMBIQUE
        MYANMAR
        NAMIBIA
        NAURU
        NEPAL
        NETHERLANDS
        NETHERLANDS_ANTILLES
        NEW_CALEDONIA
        NEW_ZEALAND
        NICARAGUA
        NIGER
        NIGERIA
        NIUE
        NORFOLK_ISLAND
        NORTHERN_MARIANA_ISLANDS
        NORWAY
        OMAN
        PAKISTAN
        PALAU
        PANAMA
        PAPUA_NEW_GUINEA
        PARAGUAY
        PERU
        PHILIPPINES
        PITCAIRN
        POLAND
        PORTUGAL
        PUERTO_RICO
        QATAR
        REUNION
        ROMANIA
        RUSSIAN_FEDERATION
        RWANDA
        SAINT_KITTS_AND_NEVIS
        SAINT_LUCIA
        SAINT_VINCENT_AND_THE_GRENADINES
        SAMOA
        SAN_MARINO
        SAO_TOME_AND_PRINCIPE
        SAUDI_ARABIA
        SENEGAL
        SERBIA
        SEYCHELLES
        SIERRA_LEONE
        SINGAPORE
        SLOVAKIA_Slovak_Republic
        SLOVENIA
        SOLOMON_ISLANDS
        SOMALIA
        SOUTH_AFRICA
        SPAIN
        SRI_LANKA
        SAINT_HELENA
        SAINT_PIERRE_AND_MIQUELON
        SUDAN
        SURINAME
        SVALBARD_AND_JAN_MAYEN_ISLANDS
        SWAZILAND
        SWEDEN
        SWITZERLAND
        SYRIAN_ARAB_REPUBLIC
        TAJIKISTAN
        TANZANIA_UNITED_REPUBLIC_OF
        TATARSTAN
        THAILAND
        TOGO
        TOKELAU
        TONGA
        TRINIDAD_AND_TOBAGO
        TUNISIA
        TURKEY
        TURKMENISTAN
        TURKS_AND_CAICOS_ISLANDS
        TUVALU
        UGANDA
        UKRAINE
        UNITED_ARAB_EMIRATES
        UNITED_KINGDOM
        UNITED_STATES
        UNITED_STATES_MINOR_OUTLYING_ISLANDS
        URUGUAY
        UZBEKISTAN
        VANUATU
        VATICAN_CITY_STATE_HOLY_SEE
        VENEZUELA
        VIET_NAM
        VIRGIN_ISLANDS_BRITISH
        VIRGIN_ISLANDS_US
        WALLIS_AND_FUTUNA_ISLANDS
        WESTERN_SAHARA
        YEMEN
        YUGOSLAVIA
        ZAIRE
        ZAMBIA
        ZIMBABWE
    End Enum

    Public Enum eLanguages
        Arabic
        Afrikaans
        Albanian
        Armenian
        Azeri
        Bulgarian
        Basque
        Belarusian
        Bengali
        Bosnian
        Chinese
        Croatian
        Czech
        Catalan
        Danish
        Dutch
        English
        Estonian
        French
        Finnish
        Farsi
        Faroese
        German
        Greek
        Georgian
        Galician
        Hungarian
        Hebrew
        'Hebrew_IW
        Hindi
        Italian
        Icelandic
        Indonesian
        Japanese
        Korean
        Kirghiz
        Latvian
        Lithuanian
        Malay
        Malayalam
        Maltese
        Maori
        Marathi
        Mongolian
        Norwegian
        Portuguese
        Polish
        Punjabi
        Romanian
        Russian
        Spanish
        Slovak
        Slovenian
        Serbian
        Sanskrit
        Swahili
        Swedish
        Thai
        Turkish
        Tamil
        Tatar
        Ukrainian
        Urdu
        Uzbek
        Vietnamese
        Welsh
        Zulu
    End Enum

    Public Enum eAnalogVideoOutputFormat
        Component
        Composite
    End Enum

    Public Enum eVideoElementaryStreamType
        M2V
        M1V
        AVC
        _264
        h264
        i420
        iYUV
        VC1
    End Enum

    Public Enum eAudioElementaryStreamType
        AC3
        DTS
        PCM
        WAV
        MPA
    End Enum

    Public Enum eProgramStreamType
        AVI
        VOB
        MPG
        MPEG
        MOV
    End Enum

    Public Enum eTransportStreamType
        M2T
        M2TS
    End Enum


End Namespace
