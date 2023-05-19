Imports System.Drawing

Namespace Utilities.ConversionsAndSuch

    Public Module mConversionsAndSuch

        Public Function SwabBytes(ByVal Bytes() As Byte) As Byte()
            Try
                Dim Out(UBound(Bytes)) As Byte
                For i As Short = 0 To UBound(Bytes) Step 2
                    If i + 1 > UBound(Bytes) Then Return Out 'this crops the last byte of the source array because its length was odd
                    Out(i + 1) = Bytes(i)
                    Out(i) = Bytes(i + 1)
                Next
                Return Out
            Catch ex As Exception
                MsgBox("Problem with SwabBytes. Error: " & ex.Message)
            End Try
        End Function

        Public Function ConvertStringFromUnicodeRemoveExtraBytes(ByVal InStringBuf() As Byte) As String
            Try
                InStringBuf = RemoveExtraBytesFromArray(InStringBuf, True)
                Return Trim(System.Text.Encoding.Unicode.GetString(InStringBuf))

                'Dim tBuf() As Byte = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.ASCII, InStringBuf)
                ''remove the extra characters
                'For i As Short = tBuf.Length - 1 To 0 Step -1
                '    If tBuf(i) = 0 Then
                '        ReDim Preserve tBuf(UBound(tBuf) - 1)
                '    End If
                'Next
                'Dim Out As String = System.Text.Encoding.ASCII.GetString(tBuf)
                'Return Trim(Out)
            Catch ex As Exception
                MsgBox("problem converting string to unicode. error: " & ex.Message)
            End Try
        End Function

        Public Class ParsedDouble
            Public Int As Integer
            Public Dec As Double
        End Class

        Public Function ParseDouble(ByVal D As Double) As ParsedDouble
            Try
                Dim s() As String = Split(CStr(D), ".")
                Dim Out As New ParsedDouble
                Out.Int = s(0)
                If s.Length = 1 Then
                    Out.Dec = CDbl("." & 0)
                Else
                    Out.Dec = CDbl("." & s(1))
                End If
                Return Out
            Catch ex As Exception
                MsgBox("Problem with ParseDouble. Error: " & ex.Message)
            End Try
        End Function

        Public Sub ByteSwap(ByRef Buf() As Byte, ByVal SwapLength As Integer, Optional ByVal Offset As Integer = 0)
            [Array].Reverse(Buf, Offset, SwapLength)
        End Sub

        Public Function BinToDec(ByVal Binary As String) As Long
            Dim n As Long
            Dim s As Integer

            For s = 1 To Len(Binary)
                n = n + (Mid(Binary, Len(Binary) - s + 1, 1) * (2 ^ (s - 1)))
            Next s

            Return n
        End Function

        Public Function DecimalToBinary(ByVal DecimalNum As Long) As String
            Dim tmp As String
            Dim n As Long

            n = DecimalNum

            tmp = Trim(Str(n Mod 2))
            n = n \ 2

            Do While n <> 0
                tmp = Trim(Str(n Mod 2)) & tmp
                n = n \ 2
            Loop

            Return tmp
        End Function

        Public Function DecToBin(ByVal iDecimalNum As Double, Optional ByVal iLen As Integer = 0) As String
            Try
                Dim sBinOut As String
                If iLen = 0 Then iLen = 8

                Do Until Int(iDecimalNum) = 0
                    If iDecimalNum Mod 2 Then
                        sBinOut = "1" & sBinOut
                        iDecimalNum = iDecimalNum - 1
                    Else
                        sBinOut = "0" & sBinOut
                    End If
                    iDecimalNum = iDecimalNum / 2
                Loop

                For A As Integer = 1 To (iLen - Len(sBinOut))
                    sBinOut = "0" & sBinOut
                Next A
                Return sBinOut
            Catch ex As Exception
                MsgBox("problem converting dec to bin. error: " & ex.Message)
                Return "FAIL"
            End Try
        End Function

        Public Function DECtoASCII(ByVal Input As Integer) As String
            Try
                Dim HexString As String = Hex(Input)
                If HexString.Length < 2 Then Return HexString
                Return System.Text.Encoding.ASCII.GetString(HexToByte(HexString))
            Catch ex As Exception
                MsgBox("problem converting dec to ascii. error: " & ex.Message)
            End Try
        End Function

        Public Function ASCIItoDEC(ByVal Input As String) As Integer
            Try
                If Input = "" Then Return "0"
                Dim Bytes() As Byte = System.Text.Encoding.ASCII.GetBytes(Input)
                Dim Out As String = ""
                For Each B As Byte In Bytes
                    Out &= Hex(B)
                Next
                Return HEXtoDEC(Out)
            Catch ex As Exception
                MsgBox("problem converting ascii to dec. error: " & ex.Message)
            End Try
        End Function

        Public Function HEXtoDEC(ByVal Input As String) As Long
            Try
                Return Int64.Parse(Input, System.Globalization.NumberStyles.HexNumber)
                'Return Val("&H" & Input)
            Catch ex As Exception
                MsgBox("problem converting hex to dec. error: " & ex.Message)
            End Try
        End Function

        Public Function DECtoHEX(ByVal Input As Integer) As String
            Return Hex(Input)
        End Function

        Public Function ASCIItoHEX(ByVal Input As String) As String
            Try
                Return DECtoHEX(ASCIItoDEC(Input))
            Catch ex As Exception
                MsgBox("problem converting ascii to hex. error: " & ex.Message)
            End Try
        End Function

        Public Function HEXtoASCII(ByVal Input As String) As String
            Try
                Return DECtoASCII(HEXtoDEC(Input))
            Catch ex As Exception
                MsgBox("problem converting hex to ascii. error: " & ex.Message)
            End Try
        End Function

        Private Function HexToByte(ByVal HexInput As String) As Byte()
            Try
                If HexInput.Length > 0 Then
                    Dim ndx As Integer = 1
                    Dim i As Integer
                    Dim ByteCnt As Integer

                    ByteCnt = HexInput.Length / 2
                    Dim ByteVar(ByteCnt) As Byte
                    For i = 0 To ByteCnt - 1
                        ByteVar(i) = Convert.ToByte(Mid(HexInput, ndx, 2), 16)
                        ndx = ndx + 2
                    Next
                    Return ByteVar
                End If
            Catch
                Err.Raise(Err.Number, "Encryption::HexToByte", Err.Description)
            End Try
        End Function

        Public Function ByteToString(ByVal InByte As Byte) As String
            Try
                Dim O As String = Hex(InByte)
                If O.Length = 1 Then O = "0" & O
                Return O
            Catch ex As Exception
                MsgBox("Problem with GetByte. Error: " & ex.Message)
                Return ""
            End Try
        End Function

        Public Function GetBitsFromArray(ByVal InStr As String, ByVal inBA As BitArray, ByVal Index As Integer, ByVal Count As Integer) As String
            Try
                Dim OutStr As String = InStr
                For i As Short = Index To Index + Count - 1

                    OutStr &= BoolToByte(inBA(i))
                Next
                Return OutStr
            Catch ex As Exception
                MsgBox("Problem with GetBitsFromArray. Error: " & ex.Message)
            End Try
        End Function

        Public Function BoolToByte(ByVal inB As Boolean) As Byte
            If inB Then
                Return 1
            Else
                Return 0
            End If
        End Function

        Public Function GetStringFromASCIIBytes(ByVal Bytes() As Byte) As String
            Try
                Bytes = RemoveExtraBytesFromArray(Bytes, False)
                Dim Out As String = System.Text.Encoding.ASCII.GetString(Bytes)
                If Out.ToString.Length = 0 Then
                    Out = "Null"
                End If
                Return Out
            Catch ex As Exception
                MsgBox("Problem with GetStringFromBytes. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Function RemoveExtraBytesFromArray(ByVal Bytes() As Byte, ByVal LeaveLastByte0 As Boolean) As Byte()
            Try
                For i As Short = Bytes.Length - 1 To 0 Step -1
                    If Bytes(i) = 0 Then
                        ReDim Preserve Bytes(UBound(Bytes) - 1)
                    Else
                        Exit For
                    End If
                Next
                If LeaveLastByte0 Then
                    ReDim Preserve Bytes(UBound(Bytes) + 1)
                End If
                Return Bytes
            Catch ex As Exception
                MsgBox("Problem removing extra bytes from array. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Function stringToByteArray(ByVal str As String) As Byte()
            ' e.g. "1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16" to 
            '{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16}
            Dim s As String()
            s = str.Split(" ")
            Dim b(s.Length - 1) As Byte
            Dim i As Integer
            For i = 0 To s.Length - 1
                b(i) = Convert.ToByte(s(i))
            Next
            Return b
        End Function

        Public Function IsEven(ByVal Num As Integer) As Boolean
            Try
                Dim Remainder As Integer
                Math.DivRem(Num, 2, Remainder)
                If Remainder > 0 Then
                    Return False
                Else
                    Return True
                End If
            Catch ex As Exception
                MsgBox("Problem with IsEven. Error: " & ex.Message)
            End Try
        End Function

        Public Function ConvertDecimalIntoByteArray(ByVal Num As Integer, ByVal MinimumSize As Short) As Byte()
            Try
                Dim HexSize As String = Hex(Num)

                If Not IsEven(Len(HexSize)) Then
                    HexSize = "0" & HexSize
                End If

                Dim TargetArraySize As Short = HexSize.Length / 2
                Dim Out(TargetArraySize) As Byte

                Dim ix As Short = 0
                For i As Short = Len(HexSize) To 1 Step -2
                    Out(ix) = CInt("&H" & Mid(HexSize, i - 1, 2))
                    ix += 1
                Next

                If Out.Length < MinimumSize Then
                    ReDim Preserve Out(MinimumSize)
                End If

                Return Out
            Catch ex As Exception
                MsgBox("Problem with ConvertDecimalIntoByteArray. Error: " & ex.Message)
            End Try
        End Function

        Public Function PadString(ByVal sText As String, ByVal iSize As Integer, ByVal sChar As String, ByVal PadLeft As Boolean) As String
            Dim CharactersToAdd As Integer = iSize - sText.Length
            If CharactersToAdd < 0 Then Return sText
            For i As Short = 1 To CharactersToAdd
                If PadLeft Then
                    sText = sChar & sText
                Else
                    sText &= sChar
                End If
            Next
            Return sText
        End Function

        Public Function RemoveLeadingUnderscore(ByVal inst As String) As String
            Return Microsoft.VisualBasic.Right(inst, inst.Length - 1)
        End Function

        Public Function GetBoolColor(ByVal nBool As Boolean) As Color
            If nBool Then
                Return Color.Green
            Else
                Return Color.DarkRed
            End If
        End Function

        Public Class CSV
            Private _CSV As String = ""

            Public Overloads Overrides Function ToString() As String
                Return _CSV
            End Function

            Public Property CSV() As String
                Get
                    Return _CSV
                End Get
                Set(ByVal Value As String)
                    _CSV = Value
                End Set
            End Property

            Public ReadOnly Property AsStringArray()
                Get
                    Return Split(_CSV, ",", -1, CompareMethod.Text)
                End Get
            End Property

            Public Sub SetByStringArray(ByVal StringArray() As String)
                _CSV = ""
                For Each S As String In StringArray
                    If _CSV = "" Then
                        _CSV = S
                    Else
                        _CSV &= "," & S
                    End If
                Next
            End Sub

            Public Sub New()
            End Sub

            Public Sub AddValue(ByVal Value As String)
                If _CSV = "" Then
                    _CSV = Value
                Else
                    _CSV &= "," & Value
                End If
            End Sub

            Public Sub RemoveValue(ByVal Value As String)
                Replace(_CSV, Value, "", 1, -1, CompareMethod.Text)
                Replace(_CSV, ",,", ",", 1, -1, CompareMethod.Text)
            End Sub

            Public Sub ReplaceValue(ByVal OldValue As String, ByVal NewValue As String)
                Replace(_CSV, OldValue, NewValue, 1, -1, CompareMethod.Text)
            End Sub

        End Class 'CSV

        Public Class cSeqBitArray
            Public SBA() As Short

            Public Function GetBytes(ByVal BitIndex As Integer, ByVal BitCount As Integer) As Byte()
                'bitcount must be evenly divisible by 8
                Dim out((BitCount / 8) - 1) As Byte
                Dim s As String
                For i As Short = BitIndex To BitIndex + BitCount - 1
                    s &= SBA(i)
                Next
                For i As Integer = 1 To s.Length / 8 Step 8
                    out(i) = BinToDec(Microsoft.VisualBasic.Mid(s, i, 8))
                Next
                Return out
            End Function

            Public Function GetBits(ByVal Index As Integer, ByVal Count As Integer) As String
                Dim OutStr As String
                For i As Short = Index To Index + Count - 1
                    OutStr &= SBA(i)
                Next
                Return OutStr
            End Function

            Public Sub New(ByVal b() As Byte)
                ReDim SBA(-1)
                Dim s As String
                For Each bt As Byte In b
                    s = DecimalToBinary(HEXtoDEC(ByteToString(bt)))
                    s = PadString(s, 8, "0", True)
                    For i As Short = 1 To 8
                        ReDim Preserve SBA(UBound(SBA) + 1)
                        SBA(UBound(SBA)) = Microsoft.VisualBasic.Mid(s, i, 1)
                    Next
                Next
            End Sub

            Public Sub New(ByVal b As Byte)
                ReDim SBA(-1)
                Dim s As String
                s = DecimalToBinary(HEXtoDEC(ByteToString(b)))
                s = PadString(s, 8, "0", True)
                For i As Short = 1 To 8
                    ReDim Preserve SBA(UBound(SBA) + 1)
                    SBA(UBound(SBA)) = Microsoft.VisualBasic.Mid(s, i, 1)
                Next
            End Sub

            Public Sub New()
            End Sub

            Public Overloads Overrides Function ToString() As String
                Dim s As String
                For Each c As Short In SBA
                    s &= c
                Next
                Return s
            End Function

            Public Function BitToBool(ByVal Index As Integer, ByVal ReverseVals As Boolean) As Boolean
                If SBA Is Nothing Then Return Nothing
                If ReverseVals Then
                    If SBA(Index) = 0 Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    If SBA(Index) = 0 Then
                        Return False
                    Else
                        Return True
                    End If
                End If
            End Function

        End Class

        Public Function MakeTwoDig(ByVal Val As Short) As String
            Try
                If Val = Nothing Then Return "00"
                Select Case Len(CStr(Val))
                    Case 0
                        Return "00"
                    Case 1
                        Return "0" & Val
                    Case 2
                        Return Val
                    Case Else
                        Throw New Exception("Val is too large for MakeTwoDig")
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with MakeTwoDig(). Error: " & ex.Message)
            End Try
        End Function

        Public Function TicksToMilliseconds(ByVal Ticks As Long) As Long
            Return Ticks / 10000
        End Function

    End Module 'ConversionsAndSuch

End Namespace
