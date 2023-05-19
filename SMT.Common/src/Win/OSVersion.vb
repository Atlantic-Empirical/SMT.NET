Imports SMT.Win.WinAPI.Kernel32

Namespace Win

    Public Module OSVersion

        ''' <summary>
        ''' Return the Windows platform.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property WindowsPlatform() As eWindowsPlatform
            Get
                Dim OSInfo As OSVERSIONINFO
                Dim Result As Long

                OSInfo.dwOSVersionInfoSize = 148
                OSInfo.szCSDVersion = Space$(128)
                Result = GetVersion(OSInfo)
                Select Case OSInfo.dwPlatformId
                    Case 1
                        Return eWindowsPlatform._95
                    Case 2
                        Return eWindowsPlatform._NT
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Return the Windows product (Vista and later versions only).
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property WindowsProduct() As eWindowsProduct
            Get
                If WindowsVersion >= 6 Then
                    Dim Product As Long
                    GetProductInfo(WindowsVersion, 0, 0, 0, Product)
                    Return Product
                End If
            End Get
        End Property

        Public ReadOnly Property WindowsVersion() As eWindowsVersion
            Get
                'Dim OSInfo As OSVERSIONINFO
                'OSInfo.dwOSVersionInfoSize = 148
                'OSInfo.szCSDVersion = Space$(128)
                'Dim Result As Long = GetVersion(OSInfo)

                Select Case Environment.OSVersion.Platform
                    Case 1
                        Select Case Environment.OSVersion.Version.Minor
                            Case 0
                                Return eWindowsVersion.Windows95
                            Case 10
                                Return eWindowsVersion.Windows98
                            Case 90
                                Return eWindowsVersion.WindowsMe
                        End Select
                    Case 2
                        Select Case Environment.OSVersion.Version.Major
                            Case 3
                                Return eWindowsVersion.WindowsNT35
                            Case 4
                                Return eWindowsVersion.WindowsNT4
                            Case 5
                                Select Case Environment.OSVersion.Version.Minor
                                    Case 0
                                        Return eWindowsVersion.Windows2000
                                    Case 1
                                        Return eWindowsVersion.WindowsXP
                                    Case 2
                                        Return eWindowsVersion.WindowsServer2003
                                End Select
                            Case 6
                                Select Case Environment.OSVersion.Version.Minor
                                    Case 0
                                        Return eWindowsVersion.WindowsVista
                                    Case 1
                                        Return eWindowsVersion.Windows7
                                End Select
                            Case 7
                                Return eWindowsVersion.Windows7
                        End Select
                End Select
            End Get
        End Property

        Public Enum eWindowsPlatform
            _95
            _NT
        End Enum

        Public Enum eWindowsVersion
            WindowsVersionInvalid
            Windows95
            Windows98
            WindowsMe
            WindowsNT35
            WindowsNT4
            Windows2000
            WindowsXP
            WindowsServer2003
            WindowsVista
            Windows7
        End Enum

        Public Enum eWindowsProduct
            Business = 6
            BusinessN = 16
            ClusterServer = 18
            DatacenterServer = 8
            DatacenterServerCore = 12
            Enterprise = 4
            EnterpriseN = 27
            EnterpriseServer = 10
            EnterpriseServerCore = 14
            EnterpriseServerIa64 = 15
            HomeBasic = 2
            HomeBasicN = 5
            HomePremium = 3
            HomePremiumN = 26
            HomeServer = 19
            ServerForSmallbusiness = 24
            SmallbusinessServer = 9
            SmallbusinessServerPremium = 25
            StandardServer = 7
            StandardServerCore = 13
            Starter = 11
            StorageEnterpriseServer = 23
            StorageExpressServer = 20
            StorageStandardServer = 21
            StorageWorkgroupServer = 22
            Ultimate = 1
            UltimateN = 28
            Undefined = 0
            WebServer = 17
            WebServerCore = 29
        End Enum

        Public Structure OSVERSIONINFO
            Public dwOSVersionInfoSize As Long
            Public dwMajorVersion As Long
            Public dwMinorVersion As Long
            Public dwBuildNumber As Long
            Public dwPlatformId As Long '1 = Windows 95 2 = Windows NT
            Public szCSDVersion As String '* 128
        End Structure

    End Module

End Namespace
