'Customisation:
'GetIcon will retrieve a small image:
'Old Method:
'    Public Shared Function GetIcon(ByVal Index As Integer) As Icon
'        Initializer()
'        Dim icon As Icon = Nothing
'        Dim hIcon As IntPtr = IntPtr.Zero
'        hIcon = ImageList_GetIcon(m_lgImgList, Index, 0)
'        If Not IsNothing(hIcon) Then
'            icon = System.Drawing.Icon.FromHandle(hIcon)
'        End If
'        Return icon
'    End Function

Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Drawing
Imports ExpTreeLib.ShellDll

Public Class SystemImageListManager

#Region "       ImageList Related Constants"
    ' For ImageList manipulation
    Private Const LVM_FIRST As Integer = &H1000
    Private Const LVM_SETIMAGELIST As Integer = (LVM_FIRST + 3)

    Private Const LVSIL_NORMAL As Integer = 0
    Private Const LVSIL_SMALL As Integer = 1
    Private Const LVSIL_STATE As Integer = 2

    Private Const TV_FIRST As Integer = &H1100
    Private Const TVM_SETIMAGELIST As Integer = (TV_FIRST + 9)

    Private Const TVSIL_NORMAL As Integer = 0
    Private Const TVSIL_STATE As Integer = 2
#End Region

#Region "   Private Fields"
    Private Shared m_Initialized As Boolean = False
    Private Shared m_smImgList As IntPtr = IntPtr.Zero 'Handle to System Small ImageList
    Private Shared m_lgImgList As IntPtr = IntPtr.Zero 'Handle to System Large ImageList
    Private Shared m_Table As New Hashtable(128)

    Private Shared m_Mutex As New Mutex()
#End Region

#Region "   New"
    ''' <summary>
    ''' Summary of Initializer.
    ''' </summary>
    ''' 
    Private Shared Sub Initializer()
        If m_Initialized Then
            Exit Sub
        End If

        Dim dwFlag As Integer = SHGFI.USEFILEATTRIBUTES Or _
                        SHGFI.SYSICONINDEX Or _
                        SHGFI.SMALLICON
        Dim shfi As New SHFILEINFO()
        m_smImgList = SHGetFileInfo(".txt", _
                           FILE_ATTRIBUTE_NORMAL, _
                           shfi, _
                           cbFileInfo, _
                           dwFlag)
        Debug.Assert((Not m_smImgList.Equals(IntPtr.Zero)), "Failed to create Image Small ImageList")
        If m_smImgList.Equals(IntPtr.Zero) Then
            Throw New Exception("Failed to create Small ImageList")
        End If

        dwFlag = SHGFI.USEFILEATTRIBUTES Or _
                        SHGFI.SYSICONINDEX Or _
                        SHGFI.LARGEICON
        m_lgImgList = SHGetFileInfo(".txt", _
                           FILE_ATTRIBUTE_NORMAL, _
                           shfi, _
                           cbFileInfo, _
                           dwFlag)
        Debug.Assert((Not m_lgImgList.Equals(IntPtr.Zero)), "Failed to create Image Small ImageList")
        If m_lgImgList.Equals(IntPtr.Zero) Then
            Throw New Exception("Failed to create Large ImageList")
        End If

        m_Initialized = True
    End Sub
#End Region

#Region "   Public Properties"
    Public Shared ReadOnly Property hSmallImageList() As IntPtr
        Get
            Return m_smImgList
        End Get
    End Property
    Public Shared ReadOnly Property hLargeImageList() As IntPtr
        Get
            Return m_lgImgList
        End Get
    End Property
#End Region

#Region "   Public Methods"
#Region "       GetIconIndex"
    Private Shared mCnt As Integer
    Private Shared bCnt As Integer

    ''' <summary>
    ''' Summary of GetIconIndex.
    ''' </summary>
    ''' <param name="item"></param>
    ''' <param name="GetOpenIcon"></param>
    ''' 
    Public Shared Function GetIconIndex(ByRef item As CShItem, _
                                        Optional ByVal GetOpenIcon As Boolean = False, _
                                        Optional ByVal GetSelectedIcon As Boolean = False _
                                        ) As Integer

        Initializer()
        Dim HasOverlay As Boolean = False  'true if it's an overlay
        Dim rVal As Integer     'The returned Index

        Dim dwflag As Integer = SHGFI.SYSICONINDEX Or _
                        SHGFI.PIDL Or SHGFI.ICON
        Dim dwAttr As Integer = 0
        'build Key into HashTable for this Item
        Dim Key As Integer = IIf(Not GetOpenIcon, item.IconIndexNormal * 256, _
                                                  item.IconIndexOpen * 256)
        With item
            If .IsLink Then
                Key = Key Or 1
                dwflag = dwflag Or SHGFI.LINKOVERLAY
                HasOverlay = True
            End If
            If .IsShared Then
                Key = Key Or 2
                dwflag = dwflag Or SHGFI.ADDOVERLAYS
                HasOverlay = True
            End If
            If GetSelectedIcon Then
                Key = Key Or 4
                dwflag = dwflag Or SHGFI.SELECTED
                HasOverlay = True   'not really an overlay, but handled the same
            End If
            If m_Table.ContainsKey(Key) Then
                rVal = (m_Table(Key))
                mCnt += 1
            ElseIf Not HasOverlay Then  'for non-overlay icons, we already have
                rVal = Key \ 256        '  the right index -- put in table
                m_Table(Key) = rVal
                bCnt += 1
            Else        'don't have iconindex for an overlay, get it. 
                'This is the tricky part -- add overlaid Icon to systemimagelist
                '  use of SmallImageList from Calum McLellan
                Dim shfi As New SHFILEINFO
                Dim shfi_small As New SHFILEINFO
                Dim HR As IntPtr
                Dim HR_SMALL As IntPtr
                If .IsFileSystem And Not .IsDisk And Not .IsFolder Then
                    dwflag = dwflag Or SHGFI.USEFILEATTRIBUTES
                    dwAttr = FILE_ATTRIBUTE_NORMAL
                End If
                HR = SHGetFileInfo(.PIDL, dwAttr, shfi, cbFileInfo, dwflag)
                HR_SMALL = SHGetFileInfo(.PIDL, dwAttr, shfi_small, cbFileInfo, dwflag Or SHGFI.SMALLICON)
                m_Mutex.WaitOne()
                rVal = ImageList_ReplaceIcon(m_smImgList, -1, shfi_small.hIcon)
                Debug.Assert(rVal > -1, "Failed to add overlaid small icon")
                Dim rVal2 As Integer
                rVal2 = ImageList_ReplaceIcon(m_lgImgList, -1, shfi.hIcon)
                Debug.Assert(rVal2 > -1, "Failed to add overlaid large icon")
                Debug.Assert(rVal = rVal2, "Small & Large IconIndices are Different")
                m_Mutex.ReleaseMutex()
                DestroyIcon(shfi.hIcon)
                DestroyIcon(shfi_small.hIcon)
                If rVal < 0 OrElse rVal <> rVal2 Then
                    Throw New ApplicationException("Failed to add Icon for " & item.DisplayName)
                End If
                m_Table(Key) = rVal
            End If
        End With
        Return rVal
    End Function
    'Private Shared Sub DebugShowImages(ByVal useSmall As Boolean, ByVal iFrom As Integer, ByVal iTo As Integer)
    '    Dim RightIcon As Icon = GetIcon(iFrom, Not useSmall)
    '    Dim rightIndex As Integer = iFrom
    '    Do While iFrom <= iTo
    '        Dim ico As Icon = GetIcon(iFrom, useSmall)
    '        Dim fShow As New frmDebugShowImage(rightIndex, RightIcon, ico, IIf(useSmall, "Small ImageList", "Large ImageList"), iFrom)
    '        fShow.ShowDialog()
    '        fShow.Dispose()
    '        iFrom += 1
    '    Loop
    'End Sub
#End Region

#Region "       GetIcon"
    ''' <summary>
    ''' Returns a GDI+ copy of the icon from the ImageList
    ''' at the specified index.</summary>
    ''' <param name="Index">The IconIndex of the desired Icon</param>
    ''' <param name="smallIcon">Optional, default = False. If True, return the
    '''   icon from the Small ImageList rather than the Large.</param>
    ''' <returns>The specified Icon or Nothing</returns>
    Public Shared Function GetIcon(ByVal Index As Integer, _
                            Optional ByVal smallIcon As Boolean = False) _
                            As Icon
        Initializer()
        Dim icon As Icon = Nothing
        Dim hIcon As IntPtr = IntPtr.Zero
        'Customisation to return a small image
        If smallIcon Then
            hIcon = ImageList_GetIcon(m_smImgList, Index, 0)
        Else
            hIcon = ImageList_GetIcon(m_lgImgList, Index, 0)
        End If
        If Not IsNothing(hIcon) Then
            icon = System.Drawing.Icon.FromHandle(hIcon)
        End If
        Return icon
    End Function
#End Region

#Region "       GetDeskTopIconIndex"
    ' No longer used. Retained for information purposes
    ' <summary>
    ' public int GetDeskTopIconIndex()
    ' Returns the Icon Index for the Desktop. This is not
    '		available using the normal methods and the image
    '		itself is not placed into the ImageList unless this
    '		call is made.
    ' </summary>
    ' <returns>Returns the Icon Index for the Desktop
    ' </returns>
    'Public Shared Function GetDeskTopIconIndex() As Integer
    '    Dim ppidl As IntPtr
    '    Dim hDum As Integer = 0
    '    Dim rVal As Integer

    '    rVal = SHGetSpecialFolderLocation(hDum, CSIDL.DESKTOP, ppidl)
    '    If rVal = 0 Then
    '        Dim dwFlags As Integer = SHGFI.SYSICONINDEX _
    '                                 Or SHGFI.PIDL
    '        Dim dwAttr As Integer = 0

    '        Dim shfi As SHFILEINFO = New SHFILEINFO()
    '        Dim resp As IntPtr
    '        resp = SHGetFileInfo(ppidl, _
    '                                dwAttr, _
    '                                shfi, _
    '                                cbFileInfo, _
    '                                dwFlags)
    '        Marshal.FreeCoTaskMem(ppidl)  ' free the pointer
    '        If resp.Equals(IntPtr.Zero) Then
    '            Debug.Assert(Not resp.Equals(IntPtr.Zero), "Failed to get icon index")
    '            rVal = -1  'Failed to get IconIndex
    '        Else
    '            rVal = shfi.iIcon  'got it, return it
    '        End If
    '    Else        'failed to get DesktopLocation
    '        rVal = -1
    '    End If
    '    If rVal > -1 Then
    '        Return rVal
    '    Else
    '        Throw New ApplicationException("Failed to get Desktop Icon Index")
    '        Return -1
    '    End If
    'End Function
#End Region

#Region "       SetListViewImageList"
    '    <summary>
    '    Associates a SysImageList with a ListView control
    '    </summary>
    '    <param name="listView">ListView control to associate ImageList with</param>
    '    <param name="forLargeIcons">True=Set Large Icon List
    '                   False=Set Small Icon List</param>
    '    <param name="forStateImages">Whether to add ImageList as StateImageList</param>
    ''' <summary>
    ''' Summary of SetListViewImageList.
    ''' </summary>
    ''' <param name="listView"></param>
    ''' <param name="forLargeIcons"></param>
    ''' <param name="forStateImages"></param>
    ''' 
    Public Shared Sub SetListViewImageList( _
            ByVal listView As ListView, _
            ByVal forLargeIcons As Boolean, _
            ByVal forStateImages As Boolean)

        Initializer()
        Dim wParam As Integer = LVSIL_NORMAL
        Dim HImageList As IntPtr = m_lgImgList
        If Not forLargeIcons Then
            wParam = LVSIL_SMALL
            HImageList = m_smImgList
        End If
        If forStateImages Then
            wParam = LVSIL_STATE
        End If
        SendMessage(listView.Handle, _
                    LVM_SETIMAGELIST, _
                    wParam, _
                    HImageList)
    End Sub
#End Region

#Region "       SetTreeViewImageList"
    '/// <summary>
    '/// Associates a SysImageList with a TreeView control
    '/// </summary>
    '/// <param name="treeView">TreeView control to associated ImageList with</param>
    '/// <param name="forStateImages">Whether to add ImageList as StateImageList</param>
    ''' <summary>
    ''' Summary of SetTreeViewImageList.
    ''' </summary>
    ''' <param name="treeView"></param>
    ''' <param name="forStateImages"></param>
    ''' 
    Public Shared Sub SetTreeViewImageList( _
        ByVal treeView As TreeView, _
        ByVal forStateImages As Boolean)

        Initializer()
        Dim wParam As Integer = LVSIL_NORMAL
        If forStateImages Then
            wParam = LVSIL_STATE
        End If
        Dim HR As Integer
        HR = SendMessage(treeView.Handle, _
                    TVM_SETIMAGELIST, _
                    wParam, _
                    m_smImgList)
    End Sub

#End Region

#End Region
End Class
