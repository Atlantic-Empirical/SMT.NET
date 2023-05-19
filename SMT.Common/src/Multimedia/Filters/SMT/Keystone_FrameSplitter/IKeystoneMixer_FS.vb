Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.Keystone

    <ComVisible(True), ComImport(), Guid("5AA8CFEE-9235-4b4d-A7B2-8C2BA21D0104"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IKeystoneMixer_FS

        <PreserveSig()> _
        Function put_OSD( _
         <[In]()> ByVal pMixImage As Integer, _
         <[In]()> ByVal W As Integer, _
         <[In]()> ByVal H As Integer, _
         <[In]()> ByVal X As Integer, _
         <[In]()> ByVal Y As Integer, _
         <[In]()> ByVal Format As Integer, _
         <[In]()> ByVal KeyColor As Integer, _
         <[In]()> ByVal DurationSecs As Integer) As Integer

        <PreserveSig()> _
        Function ClearOSD() As Integer

        <PreserveSig()> _
        Function GrabLastL21(<Out()> ByVal pL21Sample As Integer) As Integer

        <PreserveSig()> _
        Function GrabLastSubpicture(<Out()> ByVal pSubpictureSample As Integer) As Integer

        <PreserveSig()> _
        Function SetGuides( _
            <[In]()> ByVal Left As Integer, _
            <[In]()> ByVal Top As Integer, _
            <[In]()> ByVal Right As Integer, _
            <[In]()> ByVal Bottom As Integer, _
            <[In]()> ByVal Red As Integer, _
            <[In]()> ByVal Blue As Integer, _
            <[In]()> ByVal Green As Integer) As Integer

        <PreserveSig()> _
        Function ClearGuides() As Integer

        <PreserveSig()> _
        Function SetSPPlacement(ByVal X As Integer, ByVal Y As Integer) As Integer

        <PreserveSig()> _
        Function SetResizeMode(ByVal Mode As Integer) As Integer

        <PreserveSig()> _
        Function SetLBColor(ByVal Red As Integer, ByVal Green As Integer, ByVal Blue As Integer) As Integer

        <PreserveSig()> _
        Function SetL21Placement(ByVal X As Integer, ByVal Y As Integer) As Integer

        <PreserveSig()> _
        Function ReverseFieldOrder(ByVal bReverseIt As Boolean) As Integer

        <PreserveSig()> _
        Function SetJacketPicMode(ByVal bJackPicMode As Boolean) As Integer

        <PreserveSig()> _
        Function BumpFieldsDown(ByVal bBumpFields As Integer) As Integer

        <PreserveSig()> _
        Function BurnGOPTCs(ByVal iBurnGOPTCs As Integer) As Integer

        <PreserveSig()> _
        Function SetActionTitleGuides( _
            <[In]()> ByVal iShowGuides As Integer, _
            <[In]()> ByVal Red As Integer, _
            <[In]()> ByVal Blue As Integer, _
            <[In]()> ByVal Green As Integer) As Integer

        <PreserveSig()> _
        Function FieldSplit(ByVal DoFieldSplit As Integer) As Integer

        <PreserveSig()> _
        Function HighContrastSP(ByVal DoHighContrastSP As Integer) As Integer

        <PreserveSig()> _
        Function SetARFlags(ByVal PS As Short, ByVal LB As Short) As Integer

        <PreserveSig()> _
        Function SetFrameSplit( _
            <[In]()> ByVal AnchorX As Integer, _
            <[In]()> ByVal AnchorY As Integer, _
            <[In]()> ByVal R1_X As Integer, _
            <[In]()> ByVal R1_Y As Integer, _
            <[In]()> ByVal R2_X As Integer, _
            <[In]()> ByVal R2_y As Integer) As Integer

    End Interface

End Namespace
