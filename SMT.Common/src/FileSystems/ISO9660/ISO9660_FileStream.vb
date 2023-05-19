Imports System.IO
Imports System.Math

Namespace FileSystems.ISO9660

    Public Class cISO9660_FileStream
        Inherits Stream

#Region "PROPERTIES"

        Private SectorReader As cISO9660_SectorReader
        Private FileData() As Byte
        Private FileIndex As Long

#End Region 'PROPERTIES

#Region "CONSTRUCTOR"

        Public Sub New(ByVal pLBN As Integer, ByVal pLength As Long, ByVal SourceA As String, Optional ByVal SourceB As String = "")
            Me.SectorReader = New cISO9660_SectorReader(SourceA, eSectorSizeEnum.WithHeader, SourceB)

            'TODO: FINISH INITIALIZATION OF THE STREAM HERE
            ReDim FileData(pLength - 1)
            Dim lengthInLB As Integer = Math.Floor(pLength / eSectorSizeEnum.Standard)
            Dim offset As Integer = 0
            If Me.SectorReader.SectorSize = eSectorSizeEnum.WithHeader Then
                offset = 6
            End If

            For i As Integer = 1 To lengthInLB
                Me.SectorReader.SeekASector(pLBN + i, False)
                Me.Write(Me.SectorReader.sectorData, offset, eSectorSizeEnum.Standard)
            Next

            Dim bytesRemaining As Integer = pLength Mod eSectorSizeEnum.Standard
            If bytesRemaining > 0 Then
                Me.SectorReader.SeekASector(pLBN + lengthInLB + 1, False)
                Me.Write(Me.SectorReader.sectorData, offset, bytesRemaining)
            End If

            Me.FileIndex = 0
        End Sub

#End Region 'CONSTRUCTOR

#Region "OVERRIDES"

        Public Overrides ReadOnly Property CanRead() As Boolean
            Get

            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek() As Boolean
            Get

            End Get
        End Property

        Public Overrides ReadOnly Property CanTimeout() As Boolean
            Get
                Return MyBase.CanTimeout
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite() As Boolean
            Get

            End Get
        End Property

        Public Overrides ReadOnly Property Length() As Long
            Get
                If Not Me.FileData Is Nothing AndAlso Me.FileData.Length > 0 Then
                    Return Me.FileData.Length
                End If
                Return 0
            End Get
        End Property

        Public Overrides Property Position() As Long
            Get
                Return Me.FileIndex
            End Get
            Set(ByVal value As Long)
                Me.FileIndex = value
            End Set
        End Property

        Public Overrides Sub Flush()

        End Sub

        Public Overrides Function Read(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer) As Integer
            If Not Me.FileData Is Nothing And Me.Length > 0 Then
                Array.ConstrainedCopy(Me.FileData, offset, buffer, 0, count)
                Me.Position = Me.Position = count
            End If
        End Function

        Public Overrides Function Seek(ByVal offset As Long, ByVal origin As System.IO.SeekOrigin) As Long
            If origin = System.IO.SeekOrigin.Begin Then
                If offset < Me.Length Then
                    Return offset
                End If
            ElseIf origin = System.IO.SeekOrigin.Current Then
                If offset + Me.Position < Me.Length Then
                    Return offset + Me.Position
                End If
            Else
                If Me.Length - offset - 1 >= 0 Then
                    Return Me.Length - offset - 1
                End If
            End If
        End Function

        Public Overrides Sub SetLength(ByVal value As Long)
            ReDim Me.FileData(value - 1)
        End Sub

        Public Overrides Sub Write(ByVal buffer() As Byte, ByVal offset As Integer, ByVal count As Integer)
            Array.ConstrainedCopy(buffer, offset, Me.FileData, Me.Position, count)
            Me.Position = Me.Position + count
        End Sub


#End Region 'OVERRIDES

    End Class

End Namespace
