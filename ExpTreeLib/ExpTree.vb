
Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports ExpTreeLib.CShItem
Imports ExpTreeLib.ShellDll
Imports ExpTreeLib.SystemImageListManager


<DefaultProperty("StartUpDirectory"), DefaultEvent("StartUpDirectoryChanged")> _
Public Class ExpTree
    Inherits System.Windows.Forms.UserControl

    Private Root As TreeNode

    Public Event StartUpDirectoryChanged(ByVal newVal As StartDir)

    ''' <summary>
    ''' Kinovea: Event raised before a node is visually expanded but after it has been filled in.
    ''' </summary>
    Public Event TreeViewBeforeExpand(ByVal sender As Object, ByVal e As TreeViewEventArgs)

    Public Event ExpTreeNodeSelected(ByVal SelPath As String, ByVal Item As CShItem)

    Private EnableEventPost As Boolean = True 'flag to supress ExpTreeNodeSelected raising during refresh and 

    Private m_showHiddenFolders As Boolean = False

    Private m_bShortcutsMode As Boolean = False

    Private m_shortcuts As New ArrayList()

    Private m_RootDisplayName As String

    Private m_bManualCollapse As Boolean = False

    Private m_Stopwatch As New Stopwatch()

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

        'setting the imagelist here allows many good things to happen, but
        ' also one bad thing -- the "tooltip" like display of selectednode.text
        ' is made invisible.  This remains a problem to be solved.
        SystemImageListManager.SetTreeViewImageList(tv1, False)
    End Sub
    'ExpTree overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Public WithEvents tv1 As NativeTreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.tv1 = New NativeTreeView()
        Me.SuspendLayout()
        '
        'tv1
        '
        Me.tv1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tv1.HideSelection = False
        Me.tv1.Location = New System.Drawing.Point(0, 0)
        Me.tv1.Name = "tv1"
        Me.tv1.ShowRootLines = False
        Me.tv1.Size = New System.Drawing.Size(200, 264)
        Me.tv1.TabIndex = 0
        '
        'ExpTree
        '
        Me.Controls.Add(Me.tv1)
        Me.Name = "ExpTree"
        Me.Size = New System.Drawing.Size(200, 264)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "   Public Properties"

#Region "       RootItem"
    '<Summary>
    ' RootItem is a Run-Time only Property
    ' Setting this Item via an External call results in
    '  re-setting the entire tree to be rooted in the 
    '  input CShItem
    ' The new CShItem must be a valid CShItem of some kind
    '  of Folder (File Folder or System Folder)
    ' Attempts to set it using a non-Folder CShItem are ignored
    '</Summary>
    <Browsable(False)>
    Public Property RootItem() As CShItem
        Get
            Return Root.Tag
        End Get
        Set(ByVal Value As CShItem)
            If Value.IsFolder And Not m_bShortcutsMode Then
                If Not IsNothing(Root) Then
                    ClearTree()
                End If
                Root = New TreeNode(Value.DisplayName)
                BuildTree(Value.GetDirectories())
                Root.ImageIndex = SystemImageListManager.GetIconIndex(Value, False)
                Root.SelectedImageIndex = Root.ImageIndex
                Root.Tag = Value
                tv1.Nodes.Add(Root)

                Root.Expand()
                tv1.SelectedNode = Root
            End If
        End Set
    End Property
#End Region

#Region "       SelectedItem"
    <Browsable(False)>
    Public ReadOnly Property SelectedItem() As CShItem
        Get
            If Not IsNothing(tv1.SelectedNode) Then
                Return tv1.SelectedNode.Tag
            Else
                Return Nothing
            End If
        End Get
    End Property
#End Region

#Region "       ShowHidden"
    <Category("Options"),
    Description("Show Hidden Directories."),
    DefaultValue(True), Browsable(True)>
    Public Property ShowHiddenFolders() As Boolean
        Get
            Return m_showHiddenFolders
        End Get
        Set(ByVal Value As Boolean)
            m_showHiddenFolders = Value
        End Set
    End Property
#End Region

#Region "       ShowRootLines"
    <Category("Options"),
  Description("Allow Collapse of Root Item."),
  DefaultValue(True), Browsable(True)>
    Public Property ShowRootLines() As Boolean
        Get
            Return tv1.ShowRootLines
        End Get
        Set(ByVal Value As Boolean)
            If Not (Value = tv1.ShowRootLines) Then
                tv1.ShowRootLines = Value
                tv1.Refresh()
            End If
        End Set
    End Property
#End Region

#Region "       StartupDir"

    Public Enum StartDir As Integer
        Desktop = &H0
        Programs = &H2
        Controls = &H3
        Printers = &H4
        Personal = &H5
        Favorites = &H6
        Startup = &H7
        Recent = &H8
        SendTo = &H9
        StartMenu = &HB
        MyDocuments = &HC
        'MyMusic = &HD
        'MyVideo = &HE
        DesktopDirectory = &H10
        MyComputer = &H11
        My_Network_Places = &H12
        'NETHOOD = &H13
        'FONTS = &H14
        ApplicatationData = &H1A
        'PRINTHOOD = &H1B
        Internet_Cache = &H20
        Cookies = &H21
        History = &H22
        Windows = &H24
        System = &H25
        Program_Files = &H26
        MyPictures = &H27
        Profile = &H28
        Systemx86 = &H29
        AdminTools = &H30
        Special = &HFF
    End Enum

#End Region

#Region "       ShortcutMode"
    <Category("Options"),
      Description("The first level of nodes is set up manually."),
      DefaultValue(False), Browsable(True)>
    Public Property ShortcutsMode() As Boolean
        Get
            Return m_bShortcutsMode
        End Get
        Set(ByVal Value As Boolean)
            If Not (Value = m_bShortcutsMode) Then
                m_bShortcutsMode = Value
                tv1.Refresh()
            End If
        End Set
    End Property
#End Region

#Region "       RootDisplayName"
    <Browsable(False)>
    Public Property RootDisplayName() As String
        Get
            Return Root.Text
        End Get
        Set(ByVal Value As String)
            If Root Is Nothing Then
                m_RootDisplayName = Value
                Return
            End If
            'Root.Text = Value
            'tv1.Refresh()
        End Set
    End Property
#End Region

#End Region

#Region "   Public Methods"

#Region "       RefreshTree"
    '''<Summary>RefreshTree Method thanks to Calum McLellan</Summary>
    <Description("Refresh the Tree and all nodes through the currently selected item")>
    Public Sub RefreshTree(Optional ByVal rootCSI As CShItem = Nothing)
        'Modified to use ExpandANode(CShItem) rather than ExpandANode(path)
        'Set refresh variable for BeforeExpand method
        EnableEventPost = False
        'Begin Calum's change -- With some modification
        Dim Selnode As TreeNode
        If IsNothing(Me.tv1.SelectedNode) Then
            Selnode = Me.Root
        Else
            Selnode = Me.tv1.SelectedNode
        End If
        'End Calum's change
        Try
            Me.tv1.BeginUpdate()
            Dim SelCSI As CShItem = Selnode.Tag
            'Set root node
            If IsNothing(rootCSI) Then
                Me.RootItem = Me.RootItem
            Else
                Me.RootItem = rootCSI
            End If

            'Try to expand the node
            If Not Me.ExpandANode(SelCSI) Then
                Dim nodeList As New ArrayList()
                While Not IsNothing(Selnode.Parent)
                    nodeList.Add(Selnode.Parent)
                    Selnode = Selnode.Parent
                End While

                For Each Selnode In nodeList
                    If Me.ExpandANode(CType(Selnode.Tag, CShItem)) Then Exit For
                Next
            End If
            'Reset refresh variable for BeforeExpand method
        Finally
            If m_bShortcutsMode Then
                Root.Text = m_RootDisplayName
            End If
            Me.tv1.EndUpdate()
        End Try
        EnableEventPost = True
    End Sub
#End Region

#Region "       ExpandANode"
    Public Function ExpandANode(ByVal newPath As String) As Boolean
        ExpandANode = False     'assume failure
        Dim newItem As CShItem
        Try
            newItem = GetCShItem(newPath)
            If newItem Is Nothing Then Exit Function
            If Not newItem.IsFolder Then Exit Function
        Catch
            Exit Function
        End Try
        Return ExpandANode(newItem)
    End Function

    Public Function ExpandANode(ByVal newItem As CShItem) As Boolean

        log.Debug("Expanding node: " & newItem.Path)

        ExpandANode = False     'assume failure
        Dim baseNode As TreeNode = Root
        tv1.BeginUpdate()
        baseNode.Expand() 'Ensure base is filled in
        'do the drill down -- Node to expand must be included in tree
        Dim testNode As TreeNode
        Dim lim As Integer = CShItem.PidlCount(newItem.PIDL) - CShItem.PidlCount(baseNode.Tag.pidl)
        'TODO: Test ExpandARow again on XP to ensure that the CP problem ix fixed
        Do While lim > 0

            'm_Stopwatch.Restart()

            'log.DebugFormat("Looking for ancestor node at level {0} (PIDL count {1})", CShItem.PidlCount(baseNode.Tag.pidl) + 1, CShItem.PidlCount(newItem.PIDL))

            For Each testNode In baseNode.Nodes

                'log.DebugFormat("Testing node: {0} (PIDL count {1})", testNode.Tag.DisplayName, CShItem.PidlCount(testNode.Tag.PIDL))

                If CShItem.IsAncestorOf(testNode.Tag, newItem, False) Then

                    'log.DebugFormat("Found ancestor node: {0} (PIDL count {1})", testNode.Tag.DisplayName, CShItem.PidlCount(testNode.Tag.PIDL))

                    baseNode = testNode
                    RefreshNode(baseNode)   'ensure up-to-date

                    'log.DebugFormat("Node refreshed: {0} (PIDL count {1})", baseNode.Tag.DisplayName, CShItem.PidlCount(baseNode.Tag.PIDL))
                    'log.DebugFormat("Node refreshed: {0} (PIDL count {1}), time taken: {2} ms", baseNode.Tag.DisplayName, CShItem.PidlCount(baseNode.Tag.PIDL), m_Stopwatch.ElapsedMilliseconds)

                    ' Kinovea: raise an event to allow filtering.
                    Dim args As New TreeViewEventArgs(baseNode, TreeViewAction.Expand)
                    RaiseEvent TreeViewBeforeExpand(Me, args)

                    'log.DebugFormat("After event raised.")


                    baseNode.Expand()

                    'log.DebugFormat("Node expanded: {0} (PIDL count {1})", baseNode.Tag.DisplayName, CShItem.PidlCount(baseNode.Tag.PIDL))

                    lim -= 1
                    GoTo NEXLEV
                End If
            Next

            GoTo XIT     'on falling thru For, we can't find it, so get out
NEXLEV: Loop
        'after falling thru here, we have found & expanded the node

        'log.DebugFormat("Node expanded: {0} (PIDL count {1})", baseNode.Tag.DisplayName, CShItem.PidlCount(baseNode.Tag.PIDL))

        Me.tv1.HideSelection = False
        Me.Select()
        Me.tv1.SelectedNode = baseNode
        ExpandANode = True
XIT:    tv1.EndUpdate()
    End Function
#End Region

#Region "       IsOnSelectedItem"
    Public Function IsOnSelectedItem(ByVal pos As Drawing.Point) As Boolean
        IsOnSelectedItem = tv1.SelectedNode.Equals(tv1.GetNodeAt(pos))
    End Function
#End Region

#Region "       SetShortcuts"
    Public Sub SetShortcuts(ByVal shortcuts As ArrayList)
        m_shortcuts.Clear()
        Dim shortcut As String
        For Each shortcut In shortcuts
            m_shortcuts.Add(GetCShItem(shortcut))
        Next
    End Sub

    Public Sub RemoveVirtualShortcut(ByVal oldShortcut As String)

        ' Find the old shortcut in the list and remove it
        ' if it's in first place in the list.
        Dim oldCSI As CShItem = GetCShItem(oldShortcut)
        If m_shortcuts.Count > 0 AndAlso m_shortcuts(0).Equals(oldCSI) Then
            m_shortcuts.RemoveAt(0)
        End If

    End Sub

    Public Sub AddVirtualShortcut(ByVal newShortcut As String)
        ' Insert the new shortcut as the first element of the list.
        Dim newCSI As CShItem = GetCShItem(newShortcut)
        m_shortcuts.Insert(0, newCSI)


    End Sub


#End Region

#Region "   SelectNode"
    Public Sub SelectNode(ByVal path As String)
        'Find first level node matching path and select it.
        Dim root As TreeNode = tv1.Nodes(0)
        Dim node As TreeNode
        For Each node In root.Nodes
            Dim CSI As CShItem = node.Tag
            If String.Compare(CSI.Path, path) = 0 Then
                tv1.SelectedNode = node
                Exit For
            End If
        Next
    End Sub
#End Region

#Region "   RebuildFromRoot"

    Public Sub RebuildFromRoot()
        If Not IsNothing(Root) Then
            ClearTree()
        End If

        ' Always start at the desktop.
        Dim rootItem As CShItem
        rootItem = GetCShItem(CType(Val(StartDir.Desktop), ShellDll.CSIDL))

        If m_bShortcutsMode Then
            ' Shortcuts mode: add the desktop as the root but don't build its children,
            ' instead the children are from user shortcuts + current directory.
            ' Give root the desktop icon + no text.
            If IsNothing(m_RootDisplayName) Then
                m_RootDisplayName = "Root"
            End If

            Root = New TreeNode(m_RootDisplayName)
            Root.ImageIndex = SystemImageListManager.GetIconIndex(rootItem, False)
            Root.SelectedImageIndex = Root.ImageIndex
            Root.Tag = rootItem

            'log.DebugFormat("Shortcuts tree view: building tree")
            'log.DebugFormat("{0} shortcuts to add.", m_shortcuts.Count)

            ' Add the shortcuts as direct children of the root.
            Dim CSI As CShItem
            For Each CSI In m_shortcuts
                Root.Nodes.Add(MakeNode(CSI))
            Next

        Else
            ' File system mode: build the tree under the startup dir.
            Root = New TreeNode(rootItem.DisplayName)
            Root.ImageIndex = SystemImageListManager.GetIconIndex(rootItem, False)
            Root.SelectedImageIndex = Root.ImageIndex
            Root.Tag = rootItem

            log.Debug("Before BuildTree from OnStartUpDirectoryChanged")
            BuildTree(rootItem.GetDirectories())

        End If

        tv1.Nodes.Add(Root)
        Root.Expand()

    End Sub

    Private Sub BuildTree(ByVal L1 As ArrayList)
        L1.Sort()
        Dim CSI As CShItem
        For Each CSI In L1
            If Not (CSI.IsHidden And Not m_showHiddenFolders) Then
                Root.Nodes.Add(MakeNode(CSI))
            End If
        Next
    End Sub

    Private Function MakeNode(ByVal item As CShItem) As TreeNode

        ' Kinovea: special treatment to rename drives.
        ' From "System (C:)" to "C: (System)".
        ' This way all drive letters are nicely aligned.

        Dim friendlyName As String = item.DisplayName
        If item.IsDisk Then

            Dim name As String = item.DisplayName
            Dim pos1 As Integer = name.IndexOf("("c)
            Dim pos2 As Integer = name.IndexOf(")"c)
            If pos1 > 0 AndAlso pos2 > pos1 Then
                Dim label As String = name.Substring(0, pos1).Trim()
                Dim drive As String = name.Substring(pos1 + 1, pos2 - pos1 - 1).Trim()
                friendlyName = drive & " (" & label & ")"
            End If

        End If

        Dim newNode As New TreeNode(friendlyName)

        newNode.Tag = item
        newNode.ImageIndex = SystemImageListManager.GetIconIndex(item, False)
        newNode.SelectedImageIndex = SystemImageListManager.GetIconIndex(item, True)

        ' Allow expansion by creating a dummy child node.
        ' For Removable disks, always allow expansion.
        ' For all others, allow expansion based on HasSubFolders
        ' We don't care about hidden folders and hidden files.
        If item.IsRemovable Or item.HasSubFolders Then
            newNode.Nodes.Add(New TreeNode(" : "))
        End If

        Return newNode
    End Function

    Private Sub ClearTree()
        tv1.Nodes.Clear()
        Root = Nothing
    End Sub
#End Region

#End Region

#Region "   RefreshNode Sub"

    Private Sub RefreshNode(ByVal thisRoot As TreeNode)

        If thisRoot Is Root AndAlso m_bShortcutsMode Then
            'Do not get directories.	
        Else
            'Debug.WriteLine("In RefreshNode: Node = " & thisRoot.Tag.path & " -- " & thisRoot.Tag.displayname)

            'log.DebugFormat("Refreshing node: {0} (PIDL count {1}) --------------------", thisRoot.Tag.DisplayName, CShItem.PidlCount(thisRoot.Tag.PIDL))

            If Not (thisRoot.Nodes.Count = 1 AndAlso thisRoot.Nodes(0).Text.Equals(" : ")) Then
                Dim thisItem As CShItem = thisRoot.Tag
                If thisItem.RefreshDirectories Then   'RefreshDirectories True = the contained list of Directories has changed
                    Dim directoriesToAdd As ArrayList = thisItem.GetDirectories(False) 'suppress 2nd refresh
                    Dim nodesToDelete As New ArrayList()
                    Dim node As TreeNode
                    For Each node In thisRoot.Nodes 'this is the old node contents
                        Dim i As Integer
                        For i = 0 To directoriesToAdd.Count - 1
                            If CType(directoriesToAdd(i), CShItem).Equals(node.Tag) Then
                                directoriesToAdd.RemoveAt(i)   'found it, don't compare again
                                GoTo NXTOLD
                            End If
                        Next
                        ' fall thru = node no longer here
                        nodesToDelete.Add(node)
NXTOLD:             Next

                    If nodesToDelete.Count + directoriesToAdd.Count > 0 Then  'had changes

                        'log.DebugFormat("Node {0} children have changed: {1} nodes to delete, {2} directories to add", thisRoot.Tag.DisplayName, nodesToDelete.Count, directoriesToAdd.Count)
                        m_Stopwatch.Restart()

                        Try
                            tv1.BeginUpdate()

                            ' Remove nodes that are no longer there
                            For Each node In nodesToDelete
                                thisRoot.Nodes.Remove(node)
                            Next

                            'log.DebugFormat("Deleted nodes: {0}, time: {1} ms", nodesToDelete.Count, m_Stopwatch.ElapsedMilliseconds)

                            ' Add directories that are new
                            Dim csi As CShItem
                            For Each csi In directoriesToAdd
                                If Not (csi.IsHidden And Not m_showHiddenFolders) Then
                                    thisRoot.Nodes.Add(MakeNode(csi))
                                End If
                            Next

                            'log.DebugFormat("Added nodes: {0}, time: {1} ms", directoriesToAdd.Count, m_Stopwatch.ElapsedMilliseconds)

                            'we only need to resort if we added
                            'sort is based on CShItem in .Tag
                            If directoriesToAdd.Count > 0 Then
                                Dim tmpA(thisRoot.Nodes.Count - 1) As TreeNode
                                thisRoot.Nodes.CopyTo(tmpA, 0)
                                Array.Sort(tmpA, New TagComparer())
                                thisRoot.Nodes.Clear()
                                thisRoot.Nodes.AddRange(tmpA)
                            End If

                            'log.DebugFormat("Sorted nodes: {0}, time: {1} ms", thisRoot.Nodes.Count, m_Stopwatch.ElapsedMilliseconds)


                        Catch ex As Exception
                            Debug.WriteLine("Error in RefreshNode -- " & ex.ToString _
                                            & vbCrLf & ex.StackTrace)
                        Finally
                            tv1.EndUpdate()
                        End Try
                    End If
                End If
            End If
        End If
    End Sub

#End Region

#Region "   TreeView VisibleChanged Event"
    '''<Summary>When a form containing this control is Hidden and then re-Shown,
    ''' the association to the SystemImageList is lost.  Also lost is the
    ''' Expanded state of the various TreeNodes. 
    ''' The VisibleChanged Event occurs when the form is re-shown (and other times
    '''  as well).  
    ''' We re-establish the SystemImageList as the ImageList for the TreeView and
    ''' restore at least some of the Expansion.</Summary> 
    Private Sub tv1_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles tv1.VisibleChanged
        If tv1.Visible Then
            SystemImageListManager.SetTreeViewImageList(tv1, False)
            If Not Root Is Nothing Then
                Root.Expand()
                If Not IsNothing(tv1.SelectedNode) Then

                    ' Kinovea: raise an event to allow filtering.
                    Dim args As New TreeViewEventArgs(tv1.SelectedNode, TreeViewAction.Expand)
                    RaiseEvent TreeViewBeforeExpand(Me, args)

                    tv1.SelectedNode.Expand()
                Else
                    tv1.SelectedNode = Me.Root
                End If
            End If
        End If
    End Sub
#End Region

#Region "   TreeView BeforeCollapse Event"
    '''<Summary>Should never occur since if the condition tested for is True,
    ''' the user should never be able to Collapse the node. However, it is
    ''' theoretically possible for the code to request a collapse of this node
    ''' If it occurs, cancel it</Summary>
    Private Sub tv1_BeforeCollapse(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewCancelEventArgs) Handles tv1.BeforeCollapse
        If Not tv1.ShowRootLines AndAlso e.Node Is Root Then
            e.Cancel = True
        End If

        m_bManualCollapse = True

    End Sub
#End Region

#Region "   TreeView AfterCollapse Event"
    Private Sub tv1_AfterCollapse(ByVal sender As Object, ByVal e As TreeViewEventArgs) Handles tv1.AfterCollapse
        'Reset the ManualCollapse if we were already selected.
        ' (won't be reseted in AfterSelect as usual).
        If e.Node Is tv1.SelectedNode Then
            m_bManualCollapse = False
        End If
    End Sub
#End Region

#Region "   FindAncestorNode"
    '''<Summary>Given a CShItem, find the TreeNode that belongs to the
    ''' equivalent (matching PIDL) CShItem's most immediate surviving ancestor.
    '''  Note: referential comparison might not work since there is no guarantee
    ''' that the exact same CShItem is stored in the tree.</Summary>
    '''<returns> Me.Root if not found, otherwise the Treenode whose .Tag is
    ''' equivalent to the input CShItem's most immediate surviving ancestor </returns>
    Private Function FindAncestorNode(ByVal CSI As CShItem) As TreeNode
        FindAncestorNode = Nothing
        If Not CSI.IsFolder Then Exit Function 'only folders in tree
        Dim baseNode As TreeNode = Root
        'Dim cp As cPidl = CSI.clsPidl     'the cPidl rep of the PIDL to be found
        Dim testNode As TreeNode
        Dim lim As Integer = PidlCount(CSI.PIDL) - PidlCount(baseNode.Tag.pidl)
        Do While lim > 1
            For Each testNode In baseNode.Nodes
                If CShItem.IsAncestorOf(testNode.Tag, CSI, False) Then
                    baseNode = testNode
                    baseNode.Expand()
                    lim -= 1
                    GoTo NEXTLEV
                End If
            Next
            'CSI's Ancestor may have moved or been deleted, return the last one
            ' found (if none, will return Me.Root)
            Return baseNode
NEXTLEV: Loop
        'on fall thru, we have it
        Return baseNode
    End Function
#End Region

#Region "   Propagation of treeview events"
    Private Sub tv1_MouseEnter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tv1.MouseEnter
        MyBase.OnMouseEnter(e)
    End Sub
    Private Sub tv1_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles tv1.MouseDown
        MyBase.OnMouseDown(e)
    End Sub
    Private Sub tv1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tv1.Click
        MyBase.OnClick(e)
    End Sub
    Private Sub tv1_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tv1.DoubleClick
        MyBase.OnDoubleClick(e)
    End Sub
#End Region

End Class
