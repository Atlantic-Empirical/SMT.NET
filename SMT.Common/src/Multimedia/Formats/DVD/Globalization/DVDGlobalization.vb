Imports System.IO

Namespace Multimedia.Formats.DVD.Globalization

    Public Class cLanguages

        Public LangCSVPath As String
        Public Languages(-1) As cLanguage

        Public Sub New()
            Dim Assm As System.Reflection.Assembly = Me.GetType.Assembly
            Dim LanguagesCSV As Stream = Assm.GetManifestResourceStream("SMT.Languages.csv")
            Me.PopulateLanguagesFromStream(LanguagesCSV)
        End Sub

        Public Sub New(ByVal PathToLanguagesCSV As String)
            Me.LangCSVPath = PathToLanguagesCSV
            PopulateLanguages()
        End Sub

        Public Sub New(ByRef LanguagesCSV As Stream)
            PopulateLanguagesFromStream(LanguagesCSV)
        End Sub

        Public Function GetLanguageByDecimal(ByVal Dec As Short) As String
            For Each L As cLanguage In Languages
                If L.DecimalValue = Dec Then Return L.Name
            Next
        End Function

        Public Function GetLanguageByName(ByVal LanguageName As String) As cLanguage
            For Each L As cLanguage In Languages
                If L.Name = LanguageName Then Return L
            Next
        End Function

        Public Function GetLanguageHex(ByVal LanguageName As String) As String
            For Each L As cLanguage In Languages
                If L.Name = LanguageName Then Return L.HexValue
            Next
        End Function

        Public Function GetLanguageNameHexPairs() As cLanguageNameHexValue()
            Try
                Dim Out(-1) As cLanguageNameHexValue
                Dim LA As cLanguageNameHexValue
                For Each L As cLanguage In Languages
                    If L.HexValue <> "" And L.HexValue <> "ISO 3166" Then
                        ReDim Preserve Out(UBound(Out) + 1)
                        LA = New cLanguageNameHexValue
                        LA.Name = L.Name
                        LA.HexValue = L.HexValue
                        Out(UBound(Out)) = LA
                    End If
                Next
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetLanguageNameHexPairs(). Error: " & ex.Message)
            End Try
        End Function

        Public Function ShortLangString2LongLangString(ByVal ShortLang As String) As String
            Dim out As String = ""
            For Each L As cLanguage In Languages
                If InStr(L.ShortString, ShortLang) Then Return L.Name
            Next
            Return out
        End Function

        Private Sub AddLanguage(ByVal L As cLanguage)
            ReDim Preserve Languages(UBound(Languages) + 1)
            Languages(UBound(Languages)) = L
        End Sub

        Private Sub PopulateLanguagesFromStream(ByRef CSV As Stream)
            Try
                ReDim Languages(-1)
                Dim SR As New StreamReader(CSV)
                Dim line As String = SR.ReadLine()
                While Not line Is Nothing
                    AddLanguage(LineToLanguage(line))
                    line = SR.ReadLine()
                End While
                SR.Close()
                CSV.Close()
            Catch ex As Exception
                Throw New Exception("Problem with PopulateLanguagesFromStream(). Error: " & ex.Message)
            End Try
        End Sub

        Private Sub PopulateLanguages()
            Try
                ReDim Languages(-1)
                Dim FS As New FileStream(LangCSVPath, FileMode.Open)
                Dim SR As New StreamReader(FS)
                Dim line As String = SR.ReadLine()
                While Not line Is Nothing
                    AddLanguage(LineToLanguage(line))
                    line = SR.ReadLine()
                End While
                SR.Close()
                FS.Close()
            Catch ex As Exception
                Throw New Exception("Problem with PopulateLanguages(). Error: " & ex.Message)
            End Try
        End Sub

        Private Function LineToLanguage(ByVal Line As String) As cLanguage
            Try
                Dim L As New cLanguage
                Dim Ln() As String = Split(Line, ",", -1, CompareMethod.Text)
                If Ln.Length < 4 Then Throw New Exception("hi")
                With L
                    .Name = Ln(0)
                    .ShortString = Ln(1)
                    .HexValue = Ln(2)
                    .DecimalValue = Ln(3)
                End With
                Return L
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

    End Class

    Public Class cLanguageNameHexValue
        Public Name As String
        Public HexValue As String

        Public Sub New()
            Name = ""
            HexValue = ""
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    Public Class cLanguage

        Public Name As String
        Public ShortString As String
        Public HexValue As String
        Public DecimalValue As String

        Public Sub New()
            Name = ""
            ShortString = ""
            HexValue = ""
            DecimalValue = ""
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return Name & ", " & ShortString
        End Function

    End Class

    Public Class cCountries

        Public Countries(-1) As cCountry
        Public CountriesCSVPath As String

        Public Sub New()
            Dim Assm As System.Reflection.Assembly = Me.GetType.Assembly
            Dim CountriesCSV As Stream = Assm.GetManifestResourceStream("SMT.Countries.csv")
            PopulateCountriesFromStream(CountriesCSV)
        End Sub

        Public Sub New(ByVal PathToCountriesCSV As String)
            Me.CountriesCSVPath = PathToCountriesCSV
            PopulateCountriesFromFile()
        End Sub

        Public Sub New(ByRef CountriesCSV As Stream)
            PopulateCountriesFromStream(CountriesCSV)
        End Sub

        Public Function GetCountryByName(ByVal CountryName As String) As cCountry
            CountryName = Replace(CountryName, "_", " ")
            For Each C As cCountry In Countries
                If C.Name = CountryName Then Return C
            Next
        End Function

        Public Function GetCountryAlpha2(ByVal CountryName As String) As String
            CountryName = Replace(CountryName, "_", " ")
            For Each C As cCountry In Countries
                If C.Name = CountryName Then Return C.Alpha2
            Next
        End Function

        Public Function GetCountryFromAlpha(ByVal A2 As String) As String
            For Each c As cCountry In Countries
                If c.Alpha2 = A2 Then Return c.Name
            Next
        End Function

        Public Function GetCountryNameAlpha2Pairs() As cCountryNameAlpha2Pair()
            Try
                Dim Out(-1) As cCountryNameAlpha2Pair
                Dim CA As cCountryNameAlpha2Pair
                For Each C As cCountry In Countries
                    If C.Alpha2 <> "" And C.Alpha2 <> "ISO 3166" Then
                        ReDim Preserve Out(UBound(Out) + 1)
                        CA = New cCountryNameAlpha2Pair
                        CA.Name = C.Name
                        CA.Alpha2 = C.Alpha2
                        Out(UBound(Out)) = CA
                    End If
                Next
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetCountryNameAlpha2Pairs(). Error: " & ex.Message)
            End Try
        End Function

        Private Sub AddCountry(ByVal C As cCountry)
            ReDim Preserve Countries(UBound(Countries) + 1)
            Countries(UBound(Countries)) = C
        End Sub

        Private Sub PopulateCountriesFromStream(ByRef CSVStream As Stream)
            Try
                ReDim Countries(-1)
                Dim SR As New StreamReader(CSVStream)
                Dim line As String = SR.ReadLine()
                While Not line Is Nothing
                    AddCountry(LineToCountry(line))
                    line = SR.ReadLine()
                End While
                SR.Close()
                CSVStream.Close()
            Catch ex As Exception
                Throw New Exception("Problem with PopulateCountriesFromStream(). Error: " & ex.Message)
            End Try
        End Sub

        Private Sub PopulateCountriesFromFile()
            Try
                ReDim Countries(-1)
                Dim FS As New FileStream(CountriesCSVPath, FileMode.Open)
                Dim SR As New StreamReader(FS)
                Dim line As String = SR.ReadLine()
                While Not line Is Nothing
                    AddCountry(LineToCountry(line))
                    line = SR.ReadLine()
                End While
                SR.Close()
                FS.Close()
            Catch ex As Exception
                Throw New Exception("Problem with PopulateCountriesFromFile(). Error: " & ex.Message)
            End Try
        End Sub

        Private Function LineToCountry(ByVal Line As String) As cCountry
            Try
                Dim C As New cCountry
                Dim L() As String = Split(Line, ",", -1, CompareMethod.Text)
                If L.Length < 8 Then Throw New Exception("hi")
                With C
                    .Name = L(0)
                    .Alpha2 = L(1)
                    .Alpha3 = L(2)
                    .Numeric3 = L(3)
                    .WindowsCountryRegion = L(4)
                    .WindowsCode = L(5)
                    .MacName = L(6)
                    .MacCode = L(7)
                End With
                Return C
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

    End Class

    Public Class cCountry

        Public Name As String
        Public Alpha2 As String 'ISO 3166
        Public Alpha3 As String 'ISO 
        Public Numeric3 As String 'UN
        Public WindowsCountryRegion As String
        Public WindowsCode As String
        Public MacName As String
        Public MacCode As String

        Public Overloads Overrides Function ToString() As String
            Return Name
        End Function

    End Class

    Public Class cCountryNameAlpha2Pair
        Public Name As String
        Public Alpha2 As String

        Public Sub New()
            Name = ""
            Alpha2 = ""
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return Name
        End Function
    End Class

End Namespace 'Media.DVD.Globalization

