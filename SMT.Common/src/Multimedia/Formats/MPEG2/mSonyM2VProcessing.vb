Imports System.IO

Namespace Multimedia.Formats.MPEG2

    Public Class cSonyM2VProcessing

        Public CancelProcssing As Boolean

        Public Function RemoveSonyData(ByVal SourcePath As String, ByVal TargetPath As String) As Boolean
            Try
                Dim InFile As New FileStream(SourcePath, FileMode.Open, FileAccess.Read)
                If File.Exists(TargetPath) Then File.Delete(TargetPath)
                Dim OutFile As New FileStream(TargetPath, FileMode.Create, FileAccess.Write)

                Dim OnePercentBytes As Long = InFile.Length * 0.01 'each time this many bytes are processed throw the event
                ' Dim CurPosForPercentCalc As Long = 0
                'Dim FileLenForPercentCalc As Long = InFile.Length
                Dim ProgCheckByteCounter As Long = 0
                Dim PercentCounter As Integer = 0

                Dim BiteSize As Integer
                If InFile.Length >= 50000000 Then
                    BiteSize = 50000000
                Else
                    BiteSize = InFile.Length
                End If
                Dim SrcAry(BiteSize - 1) As Byte
                Dim TrgAry(BiteSize - 1) As Byte

                Dim CurSrcAryPos As Long = 0
                Dim CurTrgAryPos As Long = 0

                Dim SonyHeaderCount As Short = 0

                Dim InLastBite As Boolean = False

                Dim NotAtEndOfFile As Boolean = True

                While NotAtEndOfFile

                    'Read the file in BiteSize chunks
ProcessLastBite:
                    InFile.Read(SrcAry, 0, BiteSize)

                    'process the Bite looking for 00 00 01 E0
                    'assumming the seq hdr is dword aligned

                    CurSrcAryPos = 0
                    CurTrgAryPos = 0

                    Dim NotAtEndOfSrcAry As Boolean = True
                    While NotAtEndOfSrcAry
                        If CancelProcssing Then Exit Function
                        Dim DWORDVal As Integer = BitConverter.ToInt32(SrcAry, CurSrcAryPos)
                        If DWORDVal = -536805376 Then '00 00 01 E0 (dec 480)
                            'we've found our header
                            SonyHeaderCount += 1
                            RaiseEvent SonyHeaderFound()
                            'Debug.WriteLine("Sony Header Found: " & SonyHeaderCount)
                            'remove the header plus the next 508 bytes
                            CurSrcAryPos += 512
                            ProgCheckByteCounter += 512
                        Else
                            Array.Copy(SrcAry, CurSrcAryPos, TrgAry, CurTrgAryPos, 4)
                            CurSrcAryPos += 4
                            CurTrgAryPos += 4
                            ProgCheckByteCounter += 4
                        End If

                        If ProgCheckByteCounter > OnePercentBytes Then
                            ProgCheckByteCounter = 0
                            PercentCounter += 1
                            RaiseEvent PercentTick(PercentCounter)
                            'CurPosForPercentCalc = InFile.Position + CurSrcAryPos
                            'RaiseEvent PercentTick(FileLenForPercentCalc / CurPosForPercentCalc)
                        End If

                        NotAtEndOfSrcAry = CurSrcAryPos < ((BiteSize - 1) - 4)
                        If Not NotAtEndOfSrcAry Then
                            Debug.WriteLine("At end of src ary.")
                        End If

                    End While

                    'write the bite to the out file
                    OutFile.Write(TrgAry, 0, CurTrgAryPos + 4)

                    If InLastBite Then
                        Debug.WriteLine("At end of file.")
                        Exit While
                    End If

                    NotAtEndOfFile = InFile.Position < (InFile.Length - BiteSize)
                    If Not NotAtEndOfFile Then
                        Debug.WriteLine("Taking last bite.")

                        'we're within BiteSize of the end of the file
                        BiteSize = InFile.Length - InFile.Position 'how many more bytes shall we process?
                        If BiteSize = 0 Then Exit While
                        InLastBite = True
                        GoTo ProcessLastBite

                    End If

                End While

                InFile.Close()
                OutFile.Close()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with RemoveSonyData(). Error: " & ex.Message)
            End Try
        End Function

        Public Event PercentTick(ByVal Cur As Byte)
        Public Event SonyHeaderFound()

        Public Shared Function StreamHasSonyData(ByVal fPath As String) As Boolean
            Try
                Dim FS As New FileStream(fPath, FileMode.Open)
                Dim ar(3) As Byte
                FS.Read(ar, 0, 4)
                FS.Close()
                Dim DWORDVal As Integer = BitConverter.ToInt32(ar, 0)
                If DWORDVal = -536805376 Then '00 00 01 E0 (dec 480)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Throw New Exception("Problem with StreamHasSonyData(). Error: " & ex.Message)
            End Try
        End Function

    End Class

End Namespace
