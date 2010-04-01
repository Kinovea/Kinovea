Imports System.Runtime.InteropServices
Imports ExpTreeLib.CShItem
Imports ExpTreeLib.ShellDll

Public Class Tests

    Public Shared Sub TestFindCShItem()
        Dim CSI As CShItem
        Dim NetPlaces As CShItem = Nothing
        Dim testcsi As CShItem
        Dim i As Integer

        Debug.WriteLine("TestFindCShItem")
        CSI = GetDeskTop()
        For Each testcsi In CSI.GetDirectories
            If i > 4 Then Exit For
            If testcsi.IsFolder Then
                Debug.WriteLine(testcsi.Path & " -- DisplayName = " & testcsi.DisplayName)
                DumpPidl(testcsi.PIDL)
            End If
            If testcsi.Path.Equals("::{208D2C60-3AEA-1069-A2D7-08002B30309D}") Then
                NetPlaces = testcsi
            End If
        Next

        If Not IsNothing(NetPlaces) Then
            NetPlaces.DebugDump() : DumpPidl(NetPlaces.PIDL)
            CSI = NetPlaces
        End If
        CSI = New CShItem(CSIDL.NETHOOD)
        Debug.WriteLine("Working with NETHOOD")
        CSI.DebugDump() : DumpPidl(CSI.PIDL)
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

        For Each testcsi In CSI.GetDirectories
            Debug.WriteLine(testcsi.Path & " -- DisplayName = " & testcsi.DisplayName)
            DumpPidl(testcsi.PIDL)
        Next

        Dim hr As Integer
        Dim pchEaten As Integer
        Dim tester As IntPtr
        Debug.WriteLine("Using NETHOOD as my base")
        NetPlaces = CSI
        hr = NetPlaces.Folder.ParseDisplayName(0, IntPtr.Zero, "http://localhost/DNSTest/", pchEaten, tester, 0)
        If hr = 0 Then
            Debug.WriteLine("PIDL from NetHOOD Parse")
            DumpPidl(tester)
            testcsi = FindCShItem(tester)
            TestReport(NetPlaces, testcsi)
            If Not tester.Equals(IntPtr.Zero) Then
                Marshal.FreeCoTaskMem(tester)
            End If
        Else
            Try
                Marshal.ThrowExceptionForHR(hr)
            Catch ex As Exception
            End Try
        End If
        'Exit Sub
        'CSI = New CShItem("http://localhost/DNSTest/")
        'CSI.DebugDump() : DumpPidl(CSI.PIDL)
        'testcsi = FindCShItem(CSI.PIDL)
        'TestReport(CSI, testcsi)

        'Dim ipts() As IntPtr = DecomposePIDL(CSI.PIDL)
        ''we know, in this case that ipts(1) has item of interest
        'Dim ptrtocls As New IntPtr(ipts(1).ToInt32 + 4)
        'Dim clb(15) As Byte
        'Marshal.Copy(ptrtocls, clb, 0, 16)
        'Try
        '    Dim G As New Guid(clb)
        '    Dim P As String = "::{" & G.ToString & "}"
        '    Dim CG As New CShItem(P.ToUpper)
        '    Debug.WriteLine("mystery fldr is " & CG.Path & " -- DisplayName = " & CG.DisplayName)
        'Catch ex As Exception
        '    Debug.WriteLine("Error: " & ex.Message)
        'End Try
        'For i = 0 To ipts.Length - 1
        '    Marshal.FreeCoTaskMem(ipts(i))
        'Next

        'Exit Sub

        'CSI = New CShItem("http://localhost/DNSTest/Styles.css")
        'testcsi = FindCShItem(CSI.PIDL)
        'TestReport(CSI, testcsi)

        CSI = New CShItem("C:\ExpTree\ExpTree041105")
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

        CSI = New CShItem("C:\ExpTree\ExpTree041105\ExpTreeLib_src.zip")
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

        CSI = New CShItem(CSIDL.CONTROLS)
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

        CSI = New CShItem(CSIDL.APPDATA)
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

        CSI = New CShItem(CSIDL.DRIVES)
        testcsi = FindCShItem(CSI.PIDL)
        TestReport(CSI, testcsi)

    End Sub

    Private Shared Sub TestReport(ByVal CSI As CShItem, ByVal testCSI As CShItem)
        Debug.WriteLine("Test Item = " & CSI.Path & "-- (" & CSI.DisplayName & ")")
        If IsNothing(testCSI) Then
            Debug.WriteLine(vbTab & "Did Not find item")
            CSI.DebugDump() : DumpPidl(CSI.PIDL)
        Else
            Debug.WriteLine(vbTab & "Found:  " & testCSI.Path)
        End If
    End Sub

    'Public Shared Sub TestPidl()
    '    Dim CSI1 As New CShItem("C:\ExpDragTest\Folder1\TestFolder")
    '    Dim cList As ArrayList = CSI1.GetContents(SHCONTF.FOLDERS Or SHCONTF.INCLUDEHIDDEN Or SHCONTF.NONFOLDERS)
    '    Dim csi As CShItem
    '    Dim R1 As IntPtr
    '    Dim Last As String
    '    For Each csi In cList
    '        Debug.WriteLine(csi.Path)
    '        R1 = GetLastID(csi.PIDL)
    '        Last = csi.Path
    '    Next
    '    Debug.WriteLine("Test for Equality")
    '    For Each csi In cList
    '        Dim f As Boolean
    '        Debug.WriteLine("Compare:" & csi.Path)
    '        Debug.WriteLine("     to:" & Last)
    '        Debug.WriteLine(IIf(IsEqual(GetLastID(csi.PIDL, False), R1), "Is Equal", "Is NOT Equal"))
    '    Next
    'End Sub
End Class
