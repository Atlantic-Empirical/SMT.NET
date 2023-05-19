Imports System.Drawing

Namespace DotNet.Utility

    Public Module mConversionsAndSuch

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
                Throw New Exception("problem converting string to unicode. error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a unicode string to ASCII and trim leading and trailing white space characters.
        ''' </summary>
        ''' <param name="value">Byte array of double-byte unicode characters</param>
        ''' <returns>Trimmed ASCII string</returns>
        ''' <remarks>Use to convert an array of double-byte unicode characters to a trimmed ASCII string.</remarks>
        Public Function ConvertUnicodeToTrimmedASCII(ByVal value() As Byte) As String
            Dim result As String

            Try
                result = ConvertUnicodeToASCII(value)
                If result IsNot Nothing Then
                    result.Trim()
                End If
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with ConvertUnicodeToTrimmedASCII(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a unicode string to ASCII.
        ''' </summary>
        ''' <param name="value">Byte array of double-byte unicode characters</param>
        ''' <returns>ASCII string</returns>
        ''' <remarks>Use to convert an array of double-byte unicode characters to an ASCII string.</remarks>
        Public Function ConvertUnicodeToASCII(ByVal value() As Byte) As String
            Dim bytes As Byte()
            Dim result As String

            Try
                bytes = RemoveTrailingNullBytes(value, True)
                result = System.Text.Encoding.Unicode.GetString(bytes)
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with ConvertUnicodeToASCII(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert an array of ASCII bytes to a string.
        ''' </summary>
        ''' <param name="Bytes">Array of ASCII bytes</param>
        ''' <returns>String</returns>
        ''' <remarks>Use to convert an array of ASCII bytes to a string.</remarks>
        Public Function ConvertASCIIToString(ByVal bytes() As Byte) As String
            Dim result As String

            Try
                bytes = RemoveTrailingNullBytes(bytes, False)
                result = System.Text.Encoding.ASCII.GetString(bytes)
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with ConvertASCIIToString(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' Put double quotes (Chr(34)) on either side of the given string.
        ''' </summary>
        ''' <param name="aString">String to quote</param>
        ''' <returns>New string with quotes on either side of the original string</returns>
        ''' <remarks>Use to place quotes around an existing string.</remarks>
        Public Function QuoteString(ByVal aString As String)
            Dim result As String

            Try
                result = Chr(34) & aString & Chr(34)
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with ConvertASCIIToString(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Remove all trailing null bytes (0x00) from the given byte array.
        ''' </summary>
        ''' <param name="Bytes">Array of bytes</param>
        ''' <param name="LeaveLastByte0">Add zero byte at end of array (optional, default = <c>False</c>)</param>
        ''' <returns>Modified array of bytes</returns>
        ''' <remarks>Use to trim the trailing null bytes from an array.</remarks>
        Public Function RemoveTrailingNullBytes(ByVal bytes() As Byte, Optional ByVal LeaveLastByte0 As Boolean = False) As Byte()
            Dim i As Integer

            Try
                'Scan from end to first non-null byte
                For i = bytes.Length - 1 To 0 Step -1
                    If bytes(i) <> 0 Then
                        'Truncate array at this point and exit loop
                        ReDim Preserve bytes(i)
                        Exit For
                    End If
                Next
                'Add null byte at end if necessary
                If LeaveLastByte0 Then
                    ReDim Preserve bytes(UBound(bytes) + 1) 'Add a spot
                    bytes(UBound(bytes)) = 0                'Set it to zero
                End If
                'Return result
                Return bytes
            Catch ex As Exception
                Throw New Exception("Problem with RemoveTrailingNullBytes(). Error: " & ex.Message)
            End Try
        End Function

        ' Methods

        ''' <summary>
        ''' UNTESTED: Swaps each odd-numbered byte in an array of Bytes with the subsequent even-numbered byte.
        ''' </summary>
        ''' <param name="Bytes">Array of bytes</param>
        ''' <returns>A new byte array with values swapped.</returns>
        ''' <remarks>Use to swap the order of bytes an array without modifying the original array.</remarks>
        Public Function SwabBytes(ByVal bytes() As Byte) As Byte()
            Dim result(UBound(bytes)) As Byte
            Dim i As Integer

            Try
                For i = 0 To UBound(bytes) Step 2
                    If i + 1 > UBound(bytes) Then Exit For 'skips the last byte if length is odd
                    result(i + 1) = bytes(i)
                    result(i) = bytes(i + 1)
                Next
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with SwabBytes(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a String of binary digits to a Long Integer.  String must contain only 
        ''' binary digits ("0" and "1") and be no more than 64 characters long (additional 
        ''' characters are ignored).
        ''' </summary>
        ''' <param name="value">String of binary digits (e.g. "01100100")</param>
        ''' <param name="isSigned"><c>True</c> Binary represents a signed value (optional, default = <c>False</c>)</param>
        ''' <returns>Long Integer</returns>
        ''' <remarks>Use to convert a string of binary digits to a decimal value.</remarks>
        Public Function BinToDec(ByVal value As String, Optional ByVal isSigned As Boolean = False) As Long
            Dim result As Long
            Dim chars As Char()
            Dim len As Integer
            Dim bit As Long
            Dim i As Integer

            Try
                result = 0
                chars = value.ToCharArray()
                len = chars.Length
                'Step through bits of Long value
                For i = 0 To 63
                    'Get the bit value to use (0 or 1)
                    If i >= len Then
                        If isSigned Then
                            bit = Val(chars(0))     'if signed, repeat value of MSB
                        Else
                            bit = 0                 'if not, just fill with zeros
                        End If
                    Else
                        bit = Val(chars(len - i - 1))
                    End If
                    'Apply the bit
                    result = result Or (bit << i)
                Next
                'Return the result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with BinToDec(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a Long integer to a string of binary digits ("0" and "1").
        ''' </summary>
        ''' <param name="value">Decimal value to convert</param>
        ''' <param name="padDigits">Minimum number of digits to output for left-padding (option, default = <c>0</c></param>
        ''' <returns>String of binary digits ("0" and "1")</returns>
        ''' <remarks>Use to convert a decimal number to a string of binary digits.</remarks>
        Public Function DecToBin(ByVal value As Long, Optional ByVal padDigits As Integer = 0) As String
            Dim result As String

            Try
                'Convert value to binary representation
                result = Convert.ToString(value, 2)
                'Pad with additional digits, as needed
                If value < 0 Then   'Pad negative values with 1's
                    result = PadString(result, padDigits, "1", True)
                Else                'Pad positive values with 0's
                    result = PadString(result, padDigits, "0", True)
                End If
                'Return result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with DecToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert an Integer to a string of binary digits ("0" and "1").
        ''' </summary>
        ''' <param name="value">Decimal value to convert</param>
        ''' <param name="padDigits">Minimum number of digits to output for left-padding (option, default = <c>0</c></param>
        ''' <returns>String of binary digits ("0" and "1")</returns>
        ''' <remarks>Use to convert a decimal number to a string of binary digits.</remarks>
        Public Function DecToBin(ByVal value As Integer, Optional ByVal padDigits As Integer = 0) As String
            Dim result As String

            Try
                'Convert value to binary representation
                result = Convert.ToString(value, 2)
                'Pad with additional digits, as needed
                If value < 0 Then   'Pad negative values with 1's
                    result = PadString(result, padDigits, "1", True)
                Else                'Pad positive values with 0's
                    result = PadString(result, padDigits, "0", True)
                End If
                'Return result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with DecToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a Short integer to a string of binary digits ("0" and "1").
        ''' </summary>
        ''' <param name="value">Decimal value to convert</param>
        ''' <param name="padDigits">Minimum number of digits to output for left-padding (option, default = <c>0</c></param>
        ''' <returns>String of binary digits ("0" and "1")</returns>
        ''' <remarks>Use to convert a decimal number to a string of binary digits.</remarks>
        Public Function DecToBin(ByVal value As Short, Optional ByVal padDigits As Integer = 0) As String
            Dim result As String

            Try
                'Convert value to binary representation
                result = Convert.ToString(value, 2)
                'Pad with additional digits, as needed
                If value < 0 Then   'Pad negative values with 1's
                    result = PadString(result, padDigits, "1", True)
                Else                'Pad positive values with 0's
                    result = PadString(result, padDigits, "0", True)
                End If
                'Return result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with DecToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a Byte to a string of binary digits ("0" and "1").
        ''' </summary>
        ''' <param name="value">Decimal value to convert</param>
        ''' <param name="padDigits">Minimum number of digits to output for left-padding (option, default = <c>0</c></param>
        ''' <returns>String of binary digits ("0" and "1")</returns>
        ''' <remarks>Use to convert a decimal number to a string of binary digits.</remarks>
        Public Function DecToBin(ByVal value As Byte, Optional ByVal padDigits As Integer = 0) As String
            Dim result As String

            Try
                'Convert value to binary representation
                result = Convert.ToString(value, 2)
                'Pad with additional digits, as needed
                If value < 0 Then   'Pad negative values with 1's
                    result = PadString(result, padDigits, "1", True)
                Else                'Pad positive values with 0's
                    result = PadString(result, padDigits, "0", True)
                End If
                'Return result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with DecToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a portion of a Bit Array to a binary string.
        ''' </summary>
        ''' <param name="value">Bit Array to convert</param>
        ''' <param name="index">Starting index within array (zero-based)</param>
        ''' <param name="count">Number of entries to convert</param>
        ''' <returns>Binary string (e.g. "01000010")</returns>
        ''' <remarks>Use to convert a portion of a Bit Array to a binary string representation.</remarks>
        Public Function BitArrayToBin(ByVal value As BitArray, ByVal index As Integer, ByVal count As Integer) As String
            Dim sb As System.Text.StringBuilder
            Dim result As String
            Dim i As Integer

            Try
                sb = New System.Text.StringBuilder("")
                For i = index To count
                    If value(i) Then
                        sb = sb.Append("1")
                    Else
                        sb = sb.Append("0")
                    End If
                Next
                result = sb.ToString
                'Return the result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with BitArrayToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a portion of a Bit Array to a binary string.
        ''' </summary>
        ''' <param name="value">Bit Array to convert</param>
        ''' <param name="index">Starting index within array (optional, default=<c>0</c>)</param>
        ''' <returns>Binary string (e.g. "01000010")</returns>
        ''' <remarks>Use to convert a portion of a Bit Array to a binary string representation.</remarks>
        Public Function BitArrayToBin(ByVal value As BitArray, Optional ByVal index As Integer = 0) As String
            Dim result As String

            Try
                result = BitArrayToBin(value, index, value.Length)
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with BitArrayToBin(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a string representation of a hexidecimal number 
        ''' to a decimal value, ignoring leading and trailing white space.
        ''' </summary>
        ''' <param name="value">Hexidecimal string</param>
        ''' <returns>Long integer</returns>
        ''' <remarks>Use to convert a hexidecimal string to a decimal value</remarks>
        Public Function HexToDec(ByVal value As String) As Long
            Try
                Return Long.Parse(value, System.Globalization.NumberStyles.HexNumber)
            Catch ex As Exception
                Throw New Exception("Problem with HexToDec(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Convert a decimal value to a hexidecimal string.
        ''' </summary>
        ''' <param name="value">Decimal value to convert</param>
        ''' <returns>Hexidecimal string</returns>
        ''' <remarks>Use to convert a decimal value to a hexidecimal string.</remarks>
        Public Function DecToHex(ByVal value As Long) As String
            Dim result As String
            Try
                result = Convert.ToString(value, 16)
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with DecToHex(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Determine if integer value is an even number.
        ''' </summary>
        ''' <param name="value">Value to check</param>
        ''' <returns><c>True</c> if value is even</returns>
        ''' <remarks>Use to determine if a value is an even number.</remarks>
        Public Function IsEven(ByVal value As Long) As Boolean
            Try
                Return (value Mod 2) = 0
            Catch ex As Exception
                Throw New Exception("Problem with IsEven(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' UNTESTED: Remove the leading underscore(s) from a string.
        ''' </summary>
        ''' <param name="value">String to modify</param>
        ''' <returns>New string with leading underscores removed</returns>
        ''' <remarks>Use to remove leading underscore(s) from a string.</remarks>
        Public Function RemoveLeadingUnderscores(ByVal value As String) As String
            Dim count As Integer
            Dim result As String

            Try
                'Count the number of underscores to remove
                count = 0
                While (count < value.Length) And (value(count) = "_")
                    count += 1
                End While
                'Remove them
                result = value.Remove(0, count)
                'Return the result
                Return result
            Catch ex As Exception
                Throw New Exception("Problem with RemoveLeadingUnderscores(). Error: " & ex.Message)
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
                Throw New Exception("Problem with ParseDouble. Error: " & ex.Message)
            End Try
        End Function

        Public Sub ByteSwap(ByRef Buf() As Byte, ByVal SwapLength As Integer, Optional ByVal Offset As Integer = 0)
            [Array].Reverse(Buf, Offset, SwapLength)
        End Sub

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
                Throw New Exception("problem converting dec to bin. error: " & ex.Message)
                Return "FAIL"
            End Try
        End Function

        Public Function DECtoASCII(ByVal Input As Integer) As String
            Try
                Dim HexString As String = Hex(Input)
                If HexString.Length < 2 Then Return HexString
                Return System.Text.Encoding.ASCII.GetString(HexToByte(HexString))
            Catch ex As Exception
                Throw New Exception("problem converting dec to ascii. error: " & ex.Message)
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
                Return HexToDec(Out)
            Catch ex As Exception
                Throw New Exception("problem converting ascii to dec. error: " & ex.Message)
            End Try
        End Function

        Public Function ASCIItoHEX(ByVal Input As String) As String
            Try
                Return DecToHex(ASCIItoDEC(Input))
            Catch ex As Exception
                Throw New Exception("problem converting ascii to hex. error: " & ex.Message)
            End Try
        End Function

        Public Function HEXtoASCII(ByVal Input As String) As String
            Try
                Return DECtoASCII(HexToDec(Input))
            Catch ex As Exception
                Throw New Exception("problem converting hex to ascii. error: " & ex.Message)
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
                Throw New Exception("Problem with GetByte. Error: " & ex.Message)
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
                Throw New Exception("Problem with GetBitsFromArray. Error: " & ex.Message)
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
                Throw New Exception("Problem with GetStringFromBytes. Error: " & ex.Message)
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
                Throw New Exception("Problem removing extra bytes from array. Error: " & ex.Message)
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
                Throw New Exception("Problem with IsEven. Error: " & ex.Message)
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
                Throw New Exception("Problem with ConvertDecimalIntoByteArray. Error: " & ex.Message)
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

        Public Function Int32_to_UInt32(ByVal nSignedInt As Int32) As UInt32
            Dim out As UInt32 = 0
            If nSignedInt < 0 Then out = 2147483648 'set the bit that was the signing
            out = out Or Math.Abs(nSignedInt)
            Return out
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
                    s = DecimalToBinary(HexToDec(ByteToString(bt)))
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
                s = DecimalToBinary(HexToDec(ByteToString(b)))
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
