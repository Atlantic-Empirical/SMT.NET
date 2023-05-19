Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Utility.Timecode
Imports System.Windows.Forms

Namespace DotNet.AppConsole

    Public Class cConsoleItemPlayLocation

        Public DVDPL As DvdPlayLocation
        Public SrcTc As cTimecode

        Public Overloads Overrides Function ToString() As String
            Dim outstr As String = ""
            Dim pl As String = DVDPlayLocToString(DVDPL)
            If pl <> "N/A" Then
                outstr &= pl
            End If
            If Not SrcTc Is Nothing AndAlso SrcTc.ToString <> "" Then
                If outstr = "" Then
                    outstr = "Source: " & SrcTc.ToString
                Else
                    outstr &= " | Source: " & SrcTc.ToString
                End If
            End If
            Return outstr
        End Function

        Public Sub New(ByVal nDVDPL As DvdPlayLocation, ByVal nSTC As cTimecode)
            DVDPL = nDVDPL
            SrcTc = nSTC
        End Sub

        Public Sub New()
        End Sub

    End Class

    Public Class cConsoleItem

        Public Type As eConsoleItemType
        Public Msg As String
        Public ExtendedInfo As String
        Public Location As cConsoleItemPlayLocation

        Public Sub New()

        End Sub

        Public Sub New(ByVal nType As eConsoleItemType, ByVal nMsg As String, ByVal nExtendedInfo As String, ByVal nLocation As cConsoleItemPlayLocation)
            Try
                Me.Type = nType
                Me.Msg = nMsg
                Me.ExtendedInfo = nExtendedInfo
                Me.Location = nLocation
            Catch ex As Exception
                Throw New Exception("Problem with New() cConsoleItem. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class colConsoleItem
        Inherits CollectionBase

        Public Function Add(ByVal cPF As ListViewItem) As Integer
            Return MyBase.List.Add(cPF)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As ListViewItem
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As ListViewItem)
            MyBase.List.Remove(Item)
        End Sub

    End Class

    Public Enum eConsoleItemType
        NOTICE
        WARNING
        [ERROR]
        [EVENT]
        CLOSEDCAPTION
        UOPCHANGE
        MCS3
        MACRO
        SPRM_CHANGE
        GPRM_CHANGE
        NAV_CMD
    End Enum

End Namespace
