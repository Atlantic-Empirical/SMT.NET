Imports SMT.Win.WinAPI

Namespace Win.Globalization

    Public Module LCID

        'Example Call:
        'MAKELCID(MakeLangID(LANG_FRENCH, SUBLANG_FRENCH), SORT_DEFAULT)

        Private Const SubLangShift As Integer = &H400&
        Private Const SORT_DEFAULT As Integer = &H0 ' Sorting default

        Public Function MAKELCID(ByVal LanguageID As Integer) As Integer
            Return (SORT_DEFAULT * &H10000) Or LanguageID
        End Function

        ''old, SortID not needed 
        'Public Function MAKELCID(ByVal LanguageID As Integer, ByVal SortID As Integer) As Integer
        '    Return (SortID * &H10000) Or LanguageID
        'End Function

        ' Not fully implemented because the primary/secondary enums below are not complete.
        Public Function MakeLangID(ByVal PrimaryLanguageID As Integer, ByVal SecondaryLanguageID As Integer) As Integer
            Return (SecondaryLanguageID * SubLangShift) Or PrimaryLanguageID
        End Function

        'Use these to call MAKELCID
        Public Enum eLanguageIdentifiers
            LanguageNeutral = &H0
            InvariantLocale = &H7F                      'The language for the invariant locale (LOCALE_INVARIANT). See MAKELCID. 
            ProcessOrUserDefault = &H400                '&h0400 Process or User Default Language
            SystemDefault = &H800                       '&h0800 System Default Language 
            Afrikaans = &H436                           '&h0436 Afrikaans
            Albanian = &H41C                            '&h041c Albanian 
            Arabic_SaudiArabia = &H401                  '&h0401 Arabic (Saudi Arabia) 
            Arabic_Iraq = &H801                         '&h0801 Arabic (Iraq) 
            Arabic_Egypt = &HC01                        '&h0c01 Arabic (Egypt) 
            Arabic_Libya = &H1001                       '&h1001 Arabic (Libya)
            Arabic_Algeria = &H1401                     '&h1401 Arabic (Algeria) 
            Arabic_Morocco = &H1801                     '&h1801 Arabic (Morocco) 
            Arabic_Tunisia = &H1C01                     '&h1c01 Arabic (Tunisia) 
            Arabic_Oman = &H2001                        '&h2001 Arabic (Oman) 
            Arabic_Yemen = &H2401                       '&h2401 Arabic (Yemen) 
            Arabic_Syria = &H2801                       '&h2801 Arabic (Syria) 
            Arabic_Jordan = &H2C01                      '&h2c01 Arabic (Jordan) 
            Arabic_Lebanon = &H3001                     '&h3001 Arabic (Lebanon) 
            Arabic_Kuwait = &H3401                      '&h3401 Arabic (Kuwait) 
            Arabic_UAE = &H3801                         '&h3801 Arabic (U.A.E.) 
            Arabic_Bahrain = &H3C01                     '&h3c01 Arabic (Bahrain) 
            Arabic_Qatar = &H4001                       '&h4001 Arabic (Qatar) 
            Azeri_Latin = &H42C                         '&h042c Azeri (Latin)
            Azeri_Cyrillic = &H82C                      '&h082c Azeri (Cyrillic) 
            Basque = &H42D                              '&h042d Basque 
            Belarusian = &H423                          '&h0423 Belarusian 
            Bengali_India = &H445                       '&h0445 Bengali (India) 
            Bosnian_Bosnia_Herzegovina = &H141A         '&h141a Bosnian (Bosnia and Herzegovina) 
            Bulgarian = &H402                           '&h0402 Bulgarian 
            Burmese = &H455                             '&h0455 Burmese 
            Catalan = &H403                             '&h0403 Catalan 
            Chinese_HongKongSAR_PRC = &HC04             '&h0c04 Chinese (Hong Kong SAR, PRC) 
            Chinese_PRC = &H804                         '&h0804 Chinese (PRC) 
            Chinese_Singapore = &H1004                  '&h1004 Chinese (Singapore) 
            Chinese_Taiwan = &H404                      '&h0404 Chinese (Taiwan) 
            Croatian = &H41A                            '&h041a Croatian 
            Croatian_Bosnia_Herzegovina = &H101A        '&h101a Croatian (Bosnia and Herzegovina) 
            Czech = &H405                               '&h0405 Czech 
            Danish = &H406                              '&h0406 Danish 
            Dutch_Belgium = &H813                       '&h0813 Dutch (Belgium) 
            Dutch_Netherlands = &H413                   '&h0413 Dutch (Netherlands) 
            English_Australian = &HC09                  '&h0c09 English (Australian) 
            English_Belize = &H2809                     '&h2809 English (Belize) 
            English_Canadian = &H1009                   '&h1009 English (Canadian)
            English_Caribbean = &H2409                  '&h2409 English (Caribbean) 
            English_Ireland = &H1809                    '&h1809 English (Ireland) 
            English_Jamaica = &H2009                    '&h2009 English (Jamaica) 
            English_NZ = &H1409                         '&h1409 English (New Zealand) 
            English_SouthAfrica = &H1C09                '&h1c09 English (South Africa) 
            English_Trinidad = &H2C09                   '&h2c09 English (Trinidad) 
            English_UK = &H809                          '&h0809 English (United Kingdom) 
            English_US = &H409                          '&h0409 English (United States) 
            Estonian = &H425                            '&h0425 Estonian 
            Faeroese = &H438                            '&h0438 Faeroese 
            Farsi = &H429                               '&h0429 Farsi 
            Finnish = &H40B                             '&h040b Finnish 
            French_Belgian = &H80C                      '&h080c French (Belgian) 
            French_Canadian = &HC0C                     '&h0c0c French (Canadian) 
            French_Luxembourg = &H140C                  '&h140c French (Luxembourg) 
            French_Switzerland = &H100C                 '&h100c French (Switzerland) 
            French_Standard = &H40C                     '&h040c French (Standard)
            German_Austria = &HC07                      '&h0c07 German (Austria) 
            German_Liechtenstein = &H1407               '&h1407 German (Liechtenstein)
            German_Luxembourg = &H1007                  '&h1007 German (Luxembourg) 
            German_Standard = &H407                     '&h0407 German (Standard) 
            German_Switzerland = &H807                  '&h0807 German (Switzerland) 
            Greek = &H408                               '&h0408 Greek 
            Hebrew = &H40D                              '&h040d Hebrew 
            Hungarian = &H40E                           '&h040e Hungarian 
            Icelandic = &H40F                           '&h040f Icelandic 
            Indonesian = &H421                          '&h0421 Indonesian 
            isiXhosa_Xhosa_SouthAfrica = &H434          '&h0434 isiXhosa/Xhosa (South Africa) 
            isiZulu_Zulu_SouthAfrica = &H435            '&h0435 isiZulu/Zulu (South Africa) 
            Italian_Standard = &H410                    '&h0410 Italian (Standard) 
            Italian_Switzerland = &H810                 '&h0810 Italian (Switzerland) 
            Japanese = &H411                            '&h0411 Japanese 
            Korean = &H412                              '&h0412 Korean 
            Latvian = &H426                             '&h0426 Latvian 
            Lithuanian = &H427                          '&h0427 Lithuanian 
            Macedonian_FYROM = &H42F                    '&h042f Macedonian (FYROM) 
            Malay_BruneiDarussalam = &H83E              '&h083e Malay (Brunei Darussalam) 
            Malay_Malaysian = &H43E                     '&h043e Malay (Malaysian) 
            Malayalam_India = &H44C                     '&h044c Malayalam (India) 
            Maori_NZ = &H481                            '&h0481 Maori (New Zealand) 
            Maltese_Malta = &H43A                       '&h043a Maltese (Malta) 
            Norwegian_Bokmal = &H414                    '&h0414 Norwegian (Bokmal) 
            Norwegian_Nynorsk = &H814                   '&h0814 Norwegian (Nynorsk) 
            Polish = &H415                              '&h0415 Polish 
            Portuguese_Brazil = &H416                   '&h0416 Portuguese (Brazil) 
            Portuguese_Portugal = &H816                 '&h0816 Portuguese (Portugal) 
            Quechua_Bolivia = &H46B                     '&h046b Quechua (Bolivia) 
            Quechua_Ecuador = &H86B                     '&h086b Quechua (Ecuador) 
            Quechua_Peru = &HC6B                        '&h0c6b Quechua (Peru) 
            Romanian = &H418                            '&h0418 Romanian 
            Russian = &H419                             '&h0419 Russian 
            Sami_Inari_Finland = &H243B                 '&h243b Sami, Inari (Finland)
            Sami_Lule_Norway = &H103B                   '&h103b Sami, Lule (Norway) 
            Sami_Lule_Sweden = &H143B                   '&h143b Sami, Lule (Sweden) 
            Sami_Northern_Finland = &HC3B               '&h0c3b Sami, Northern (Finland) 
            Sami_Northern_Norway = &H43B                '&h043b Sami, Northern (Norway) 
            Sami_Northern_Sweden = &H83B                '&h083b Sami, Northern (Sweden) 
            Sami_Southern_Norway = &H183B               '&h183b Sami, Southern (Norway) 
            Sami_Southern_Sweden = &H1C3B               '&h1c3b Sami, Southern (Sweden) 
            Sami_Skolt_Finland = &H203B                 '&h203b Sami, Skolt (Finland) 
            Serbian_Cyrillic = &HC1A                    '&h0c1a Serbian (Cyrillic) 
            Serbian_Cyrillic_Bosnia_Herzegovina = &H1C1A    '&h1c1a Serbian (Cyrillic, Bosnia, and Herzegovina) 
            Serbian_Latin = &H81A                       '&h081a Serbian (Latin) 
            Serbian_Latin_Bosnia_Herzegovina = &H181A   '&h181a Serbian (Latin, Bosnia, and Herzegovina) 
            SesothoSaLeboa_NorthernSotho_SouthAfrica = &H46C    '&h046c Sesotho sa Leboa/Northern Sotho (South Africa) 
            Setswana_Tswana_SouthAfrica = &H432         '&h0432 Setswana/Tswana (South Africa) 
            Slovak = &H41B                              '&h041b Slovak 
            Slovenian = &H424                           '&h0424 Slovenian 
            Spanish_Argentina = &H2C0A                  '&h2c0a Spanish (Argentina) 
            Spanish_Bolivia = &H400A                    '&h400a Spanish (Bolivia) 
            Spanish_Chile = &H340A                      '&h340a Spanish (Chile) 
            Spanish_Colombia = &H240A                   '&h240a Spanish (Colombia) 
            Spanish_CostaRica = &H140A                  '&h140a Spanish (Costa Rica)
            Spanish_DominicanRepublic = &H1C0A          '&h1c0a Spanish (Dominican Republic) 
            Spanish_Ecuador = &H300A                    '&h300a Spanish (Ecuador)
            Spanish_ElSalvador = &H440A                 '&h440a Spanish (El Salvador) 
            Spanish_Guatemala = &H100A                  '&h100a Spanish (Guatemala)
            Spanish_Honduras = &H480A                   '&h480a Spanish (Honduras) 
            Spanish_Mexican = &H80A                     '&h080a Spanish (Mexican) 
            Spanish_Nicaragua = &H4C0A                  '&h4c0a Spanish (Nicaragua)
            Spanish_Panama = &H180A                     '&h180a Spanish (Panama) 
            Spanish_Paraguay = &H3C0A                   '&h3c0a Spanish (Paraguay)
            Spanish_Peru = &H280A                       '&h280a Spanish (Peru)
            Spanish_PuertoRico = &H500A                 '&h500a Spanish (Puerto Rico) 
            Spanish_Spain_ModernSort = &HC0A            '&h0c0a Spanish (Spain, Modern Sort) 
            Spanish_Spain_TraditionalSort = &H40A       '&h040a Spanish (Spain, Traditional Sort)
            Spanish_Uruguay = &H380A                    '&h380a Spanish (Uruguay) 
            Spanish_Venezuela = &H200A                  '&h200a Spanish (Venezuela)
            Sutu = &H430                                '&h0430 Sutu 
            Swahili_Kenya = &H441                       '&h0441 Swahili (Kenya) 
            Swedish = &H41D                             '&h041d Swedish 
            Swedish_Finland = &H81D                     '&h081d Swedish (Finland) 
            Tatar_Tatarstan = &H444                     '&h0444 Tatar (Tatarstan) 
            Thai = &H41E                                '&h041e Thai 
            Turkish = &H41F                             '&h041f Turkish 
            Ukrainian = &H422                           '&h0422 Ukrainian 
            Urdu_India = &H820                          '&h0820 Urdu (India) 
            Uzbek_Latin = &H443                         '&h0443 Uzbek (Latin) 
            Uzbek_Cyrillic = &H843                      '&h0843 Uzbek (Cyrillic) 
            Welsh_UK = &H452                            '&h0452 Welsh (United Kingdom)
            Windows2000_XP_Armenian_Unicode = &H42B     '&h042b Windows 2000/XP: Armenian. This is Unicode only. 
            Windows2000_XP_Georgian_Unicode = &H437     '&h0437 Windows 2000/XP: Georgian. This is Unicode only. 
            Windows2000_XP_Hindi_Unicode = &H439        '&h0439 Windows 2000/XP: Hindi. This is Unicode only.
            Windows2000_XP_Konkani_Unicode = &H457      '&h0457 Windows 2000/XP: Konkani. This is Unicode only. 
            Windows2000_XP_Marathi_Unicode = &H44E      '&h044e Windows 2000/XP: Marathi. This is Unicode only.
            Windows2000_XP_Sanskirt_Unicode = &H44F     '&h044f Windows 2000/XP: Sanskrit. This is Unicode only. 
            Windows2000_XP_Tamil_Unicode = &H449        '&h0449 Windows 2000/XP: Tamil. This is Unicode only.
            Windows95_NT4_Korean_Johab = &H812          '&h0812 Windows 95, Windows NT 4.0 only: Korean (Johab) 
            Windows98_Lithuanian_Classic = &H827        '&h0827 Windows 98 only: Lithuanian (Classic) 
            Windows98_Me_2000_XP_Chinese_MacaoSAR = &H1404  '&h1404 Windows 98/Me, Windows 2000/XP: Chinese (Macao SAR) 
            Windows98_Me_2000_XP_English_Philippines = &H3409   '&h3409 Windows 98/Me, Windows 2000/XP: English (Philippines) 
            Windows98_Me_2000_XP_English_Zimbabwe = &H3009  '&h3009 Windows 98/Me, Windows 2000/XP: English (Zimbabwe) 
            Windows98_Me_2000_XP_French_Monaco = &H180C  '&h180c Windows 98/Me, Windows 2000/XP: French (Monaco) 
            Windows98_Me_2000_XP_Urdu_Pakistan = &H420   '&h0420 Windows 98/Me, Windows 2000/XP: Urdu (Pakistan)
            Windows98_Me_NT4_Later_Vietnamese = &H42A    '&h042a Windows 98/Me, Windows NT 4.0 and later: Vietnamese 
            WindowxXP_Divehi_Unicode = &H465             '&h0465 Windows XP: Divehi. This is Unicode only.
            WindowsXP_Galician = &H456                   '&h0456 Windows XP: Galician 
            WindowsXP_Gujarati_Unicode = &H447           '&h0447 Windows XP: Gujarati. This is Unicode only. 
            WindowsXP_Kannada_Unicode = &H44B            '&h044b Windows XP: Kannada. This is Unicode only.
            WindowsXP_Kyrgyz = &H440                     '&h0440 Windows XP: Kyrgyz.
            WindowsXP_Mongolian = &H450                  '&h0450 Windows XP: Mongolian
            WindowsXP_Punjabi_Unicode = &H446            '&h0446 Windows XP: Punjabi. This is Unicode only.
            WindowsXP_Syriac_Unicode = &H45A             '&h045a Windows XP: Syriac. This is Unicode only.
            WindowsXP_Telugu_Unicode = &H44A             '&h044a Windows XP: Telugu. This is Unicode only. 
        End Enum

        Public Enum ePrimaryLanguageIdentifiers
            LANG_NEUTRAL = &H0  'Neutral
            LANG_ARABIC = &H1 'Arabic 
            LANG_BULGARIAN = &H2 'Bulgarian 
            LANG_CATALAN = &H3 'Catalan  
            LANG_CHINESE = &H4 'Chinese 
            '&h05 LANG_CZECH Czech 
            '&h06 LANG_DANISH Danish 
            '&h07 LANG_GERMAN German 
            '&h08 LANG_GREEK Greek 
            '&h09 LANG_ENGLISH English  
            '&h0a LANG_SPANISH Spanish  
            '&h0b LANG_FINNISH Finnish 
            '&h0c LANG_FRENCH French 
            '&h0d LANG_HEBREW Hebrew 
            '&h0e LANG_HUNGARIAN Hungarian 
            '&h0f LANG_ICELANDIC Icelandic 
            '&h10 LANG_ITALIAN Italian  
            '&h11 LANG_JAPANESE Japanese 
            '&h12 LANG_KOREAN Korean 
            '&h13 LANG_DUTCH Dutch 
            '&h14 LANG_NORWEGIAN Norwegian 
            '&h15 LANG_POLISH Polish 
            '&h16 LANG_PORTUGUESE Portuguese 
            '&h18 LANG_ROMANIAN Romanian 
            '&h19 LANG_RUSSIAN Russian 
            '&h1a LANG_CROATIAN Croatian 
            '&h1a LANG_SERBIAN Serbian 
            '&h1b LANG_SLOVAK Slovak 
            '&h1c LANG_ALBANIAN Albanian 
            '&h1d LANG_SWEDISH Swedish  
            '&h1e LANG_THAI Thai 
            '&h1f LANG_TURKISH Turkish  
            '&h20 LANG_URDU Urdu 
            '&h21 LANG_INDONESIAN Indonesian 
            '&h22 LANG_UKRAINIAN Ukrainian 
            '&h23 LANG_BELARUSIAN Belarusian 
            '&h24 LANG_SLOVENIAN Slovenian 
            '&h25 LANG_ESTONIAN Estonian 
            '&h26 LANG_LATVIAN Latvian 
            '&h27 LANG_LITHUANIAN Lithuanian 
            '&h29 LANG_FARSI Farsi 
            '&h2a LANG_VIETNAMESE Vietnamese 
            '&h2b LANG_ARMENIAN Armenian 
            '&h2c LANG_AZERI Azeri 
            '&h2d LANG_BASQUE Basque 
            '&h2f LANG_MACEDONIAN Macedonian (FYROM) 
            '&h36 LANG_AFRIKAANS Afrikaans 
            '&h37 LANG_GEORGIAN Georgian 
            '&h38 LANG_FAEROESE Faeroese 
            '&h39 LANG_HINDI Hindi 
            '&h3e LANG_MALAY Malay 
            '&h3f LANG_KAZAK Kazak 
            '&h40 LANG_KYRGYZ Kyrgyz 
            '&h41 LANG_SWAHILI Swahili 
            '&h43 LANG_UZBEK Uzbek 
            '&h44 LANG_TATAR Tatar 
            '&h45 LANG_BENGALI Not supported. 
            '&h46 LANG_PUNJABI Punjabi 
            '&h47 LANG_GUJARATI Gujarati 
            '&h48 LANG_ORIYA Not supported. 
            '&h49 LANG_TAMIL Tamil 
            '&h4a LANG_TELUGU Telugu 
            '&h4b LANG_KANNADA Kannada 
            '&h4c LANG_MALAYALAM Not supported. 
            '&h4d LANG_ASSAMESE Not supported. 
            '&h4e LANG_MARATHI Marathi 
            '&h4f LANG_SANSKRIT Sanskrit 
            '&h50 LANG_MONGOLIAN Mongolian 
            '&h56 LANG_GALICIAN Galician 
            '&h57 LANG_KONKANI Konkani 
            '&h58 LANG_MANIPURI Not supported. 
            '&h59 LANG_SINDHI Not supported. 
            '&h5a LANG_SYRIAC Syriac 
            '&h60 LANG_KASHMIRI Not supported. 
            '&h61 LANG_NEPALI Not supported. 
            '&h65 LANG_DIVEHI Divehi 
            '&h7f LANG_INVARIANT   

        End Enum

        Public Enum eSublanguageIdentifiers
            SUBLANG_NEUTRAL = &H0 'Language neutral 
            '&h01 SUBLANG_DEFAULT User Default 
            '&h02 SUBLANG_SYS_DEFAULT System Default 
            '&h01 SUBLANG_ARABIC_SAUDI_ARABIA Arabic (Saudi Arabia) 
            '&h02 SUBLANG_ARABIC_IRAQ Arabic (Iraq) 
            '&h03 SUBLANG_ARABIC_EGYPT Arabic (Egypt) 
            '&h04 SUBLANG_ARABIC_LIBYA Arabic (Libya) 
            '&h05 SUBLANG_ARABIC_ALGERIA Arabic (Algeria) 
            '&h06 SUBLANG_ARABIC_MOROCCO Arabic (Morocco) 
            '&h07 SUBLANG_ARABIC_TUNISIA Arabic (Tunisia) 
            '&h08 SUBLANG_ARABIC_OMAN Arabic (Oman) 
            '&h09 SUBLANG_ARABIC_YEMEN Arabic (Yemen) 
            '&h0a SUBLANG_ARABIC_SYRIA Arabic (Syria) 
            '&h0b SUBLANG_ARABIC_JORDAN Arabic (Jordan) 
            '&h0c SUBLANG_ARABIC_LEBANON Arabic (Lebanon) 
            '&h0d SUBLANG_ARABIC_KUWAIT Arabic (Kuwait) 
            '&h0e SUBLANG_ARABIC_UAE Arabic (U.A.E.) 
            '&h0f SUBLANG_ARABIC_BAHRAIN Arabic (Bahrain) 
            '&h10 SUBLANG_ARABIC_QATAR Arabic (Qatar) 
            '&h01 SUBLANG_AZERI_LATIN Azeri (Latin) 
            '&h02 SUBLANG_AZERI_CYRILLIC Azeri (Cyrillic) 
            '&h01 SUBLANG_CHINESE_TRADITIONAL Chinese (Traditional) 
            '&h02 SUBLANG_CHINESE_SIMPLIFIED Chinese (Simplified) 
            '&h03 SUBLANG_CHINESE_HONGKONG Chinese (Hong Kong SAR, PRC) 
            '&h04 SUBLANG_CHINESE_SINGAPORE Chinese (Singapore) 
            '&h05 SUBLANG_CHINESE_MACAU Chinese (Macao SAR) 
            '&h01 SUBLANG_DUTCH Dutch 
            '&h02 SUBLANG_DUTCH_BELGIAN Dutch (Belgian)  
            '&h01 SUBLANG_ENGLISH_US English (US) 
            '&h02 SUBLANG_ENGLISH_UK English (UK) 
            '&h03 SUBLANG_ENGLISH_AUS English (Australian) 
            '&h04 SUBLANG_ENGLISH_CAN English (Canadian) 
            '&h05 SUBLANG_ENGLISH_NZ English (New Zealand) 
            '&h06 SUBLANG_ENGLISH_EIRE English (Ireland) 
            '&h07 SUBLANG_ENGLISH_SOUTH_AFRICA English (South Africa) 
            '&h08 SUBLANG_ENGLISH_JAMAICA English (Jamaica) 
            '&h09 SUBLANG_ENGLISH_CARIBBEAN English (Caribbean) 
            '&h0a SUBLANG_ENGLISH_BELIZE English (Belize) 
            '&h0b SUBLANG_ENGLISH_TRINIDAD English (Trinidad) 
            '&h0c SUBLANG_ENGLISH_ZIMBABWE English (Zimbabwe) 
            '&h0d SUBLANG_ENGLISH_PHILIPPINES English (Philippines) 
            '&h01 SUBLANG_FRENCH French 
            '&h02 SUBLANG_FRENCH_BELGIAN French (Belgian) 
            '&h03 SUBLANG_FRENCH_CANADIAN French (Canadian) 
            '&h04 SUBLANG_FRENCH_SWISS French (Swiss) 
            '&h05 SUBLANG_FRENCH_LUXEMBOURG French (Luxembourg) 
            '&h06 SUBLANG_FRENCH_MONACO French (Monaco) 
            '&h01 SUBLANG_GERMAN German 
            '&h02 SUBLANG_GERMAN_SWISS German (Swiss) 
            '&h03 SUBLANG_GERMAN_AUSTRIAN German (Austrian) 
            '&h04 SUBLANG_GERMAN_LUXEMBOURG German (Luxembourg) 
            '&h05 SUBLANG_GERMAN_LIECHTENSTEIN German (Liechtenstein) 
            '&h01 SUBLANG_ITALIAN Italian 
            '&h02 SUBLANG_ITALIAN_SWISS Italian (Swiss) 
            '&h01 SUBLANG_KOREAN Korean 
            '&h01 SUBLANG_LITHUANIAN Lithuanian 
            '&h01 SUBLANG_MALAY_MALAYSIA Malay (Malaysia) 
            '&h02 SUBLANG_MALAY_BRUNEI_DARUSSALAM Malay (Brunei Darassalam) 
            '&h01 SUBLANG_NORWEGIAN_BOKMAL Norwegian (Bokmal) 
            '&h02 SUBLANG_NORWEGIAN_NYNORSK Norwegian (Nynorsk) 
            '&h01 SUBLANG_PORTUGUESE_BRAZILIAN Portuguese (Brazil) 
            '&h02 SUBLANG_PORTUGUESE Portuguese (Portugal) 
            '&h02 SUBLANG_SERBIAN_LATIN Serbian (Latin) 
            '&h03 SUBLANG_SERBIAN_CYRILLIC Serbian (Cyrillic) 
            '&h01 SUBLANG_SPANISH Spanish (Castilian) 
            '&h02 SUBLANG_SPANISH_MEXICAN Spanish (Mexican) 
            '&h03 SUBLANG_SPANISH_MODERN Spanish (Spain) 
            '&h04 SUBLANG_SPANISH_GUATEMALA Spanish (Guatemala) 
            '&h05 SUBLANG_SPANISH_COSTA_RICA Spanish (Costa Rica) 
            '&h06 SUBLANG_SPANISH_PANAMA Spanish (Panama) 
            '&h07 SUBLANG_SPANISH_DOMINICAN_REPUBLIC Spanish (Dominican Republic) 
            '&h08 SUBLANG_SPANISH_VENEZUELA Spanish (Venezuela) 
            '&h09 SUBLANG_SPANISH_COLOMBIA Spanish (Colombia) 
            '&h0a SUBLANG_SPANISH_PERU Spanish (Peru) 
            '&h0b SUBLANG_SPANISH_ARGENTINA Spanish (Argentina) 
            '&h0c SUBLANG_SPANISH_ECUADOR Spanish (Ecuador) 
            '&h0d SUBLANG_SPANISH_CHILE Spanish (Chile) 
            '&h0e SUBLANG_SPANISH_URUGUAY Spanish (Uruguay) 
            '&h0f SUBLANG_SPANISH_PARAGUAY Spanish (Paraguay) 
            '&h10 SUBLANG_SPANISH_BOLIVIA Spanish (Bolivia) 
            '&h11 SUBLANG_SPANISH_EL_SALVADOR Spanish (El Salvador) 
            '&h12 SUBLANG_SPANISH_HONDURAS Spanish (Honduras) 
            '&h13 SUBLANG_SPANISH_NICARAGUA Spanish (Nicaragua) 
            '&h14 SUBLANG_SPANISH_PUERTO_RICO Spanish (Puerto Rico) 
            '&h01 SUBLANG_SWEDISH Swedish 
            '&h02 SUBLANG_SWEDISH_FINLAND Swedish (Finland) 
            '&h01 SUBLANG_URDU_PAKISTAN Urdu (Pakistan) 
            '&h02 SUBLANG_URDU_INDIA Urdu (India) 
            '&h01 SUBLANG_UZBEK_LATIN Uzbek (Latin) 
            '&h02 SUBLANG_UZBEK_CYRILLIC Uzbek (Cyrillic) 
        End Enum

        Public Function GetLanguageByLCID(ByVal LCID As Long) As String
            Try
                Dim Lang As String = GetUserLocaleInfo(LCID, LOCALE_SENGLANGUAGE)
                If InStr(Lang, "Norwegian (Bokmål)") Then
                    Lang = "Norwegian "
                End If
                Return Microsoft.VisualBasic.Left(Lang, Lang.Length - 1)
            Catch ex As Exception
                Return ("error getting language. error: " & ex.Message)
            End Try
        End Function

        Public Function GetLanguageTwoCharByLCID(ByVal LCID As Long) As String
            Try
                Dim Lang As String = GetUserLocaleInfo(LCID, LOCALE_SISO639LANGNAME)
                Return Microsoft.VisualBasic.Left(Lang, Lang.Length - 1)
            Catch ex As Exception
                Return ("error getting language. error: " & ex.Message)
            End Try
        End Function

        'found in WinNLs.h
        Public Const LOCALE_SENGLANGUAGE As Long = &H1001          'English name of lang
        Public Const LOCALE_SISO639LANGNAME As Long = &H59   'ISO abbreviated language name

        Public Function GetUserLocaleInfo(ByVal dwLocaleID As Long, ByVal dwLCType As Long) As String
            Dim sReturn As String
            Dim r As Long

            'call the function passing the Locale type
            'variable to retrieve the required size of
            'the string buffer needed
            r = kernel32.GetLocaleInfo(dwLocaleID, dwLCType, sReturn, Len(sReturn))

            'if successful..
            If r Then

                'pad the buffer with spaces
                sReturn = Space$(r)

                'and call again passing the buffer
                r = GetLocaleInfo(dwLocaleID, dwLCType, sReturn, Len(sReturn))

                'if successful (r > 0)
                If r Then

                    'r holds the size of the string
                    'including the terminating null
                    GetUserLocaleInfo = Microsoft.VisualBasic.Left(sReturn, r - 1)

                End If

            End If
            Return sReturn
        End Function

    End Module

End Namespace
