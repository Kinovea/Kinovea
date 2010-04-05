
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports ExpTreeLib.CShItem
Imports ExpTreeLib.ShellDll

'''<Summary>The CDragWrapper class deals with the mechanics of receiving a
''' Drag/Drop operation.  In effect, it implements the IDropTarget interface
''' for a Control.  It is designed to handle either a TreeView or a ListView control
''' which MUST have CShItems in the Tags of the TreeNodes or ListViewItems contained
''' in the control.
''' The class recieves the DragEnter, DragLeave, DragOver, and DragDrop events for
''' the associated control, performs the Drag specific processing, and raises corresponding 
''' Events for the associated control to allow the control to do any control related
''' processing.
''' The interesting part of this class is that it makes no decisions about the drag
''' nor does any Drop related processing itself. Instead, it acts as a broker between
''' the .Net Drag/Drop operation and the IDropTarget interface of the underlying 
''' Shell Folder.  This allows the Shell Folder, which may be a Shell Extention, to
''' perform whatever action it needs to in order to effect the Drag/Drop.
''' The benefit of this approach is that Drag/Drop targets need not be part of the
''' File System.
''' Since we us the COM Interface of the .Net IDataObject, such Drops are done 
''' Synchronously -- not returning from the DragDrop call until the operation 
''' has completed.
''' </Summary>
Public Class CDragWrapper

#Region "   Private Fields"

    Private m_View As Control               'The control that is client to this instance
    Private m_DragData As IntPtr            'The COM interface to IDragData - saved in DragEnter
    Private m_LastTarget As IDropTarget     'Of most recent Folder dragged over
    Private m_LastNode As Object            'Most recent node dragged over
    Private m_IsTreeView As Boolean         'True if working with a TreeView, otherwise assume ListView
    Private m_DragList As ArrayList         'CShItems of Items dragged

#End Region

#Region "   Public Events"
    Public Event ShDragEnter(ByVal DragList As ArrayList, _
                             ByVal e As DragEventArgs)   'note:control cannot change

    Public Event ShDragOver(ByVal Node As Object, _
                            ByVal ClientPoint As System.Drawing.Point, _
                            ByVal grfKeyState As Integer, _
                            ByVal pdwEffect As Integer)   'note:control cannot change

    Public Event ShDragLeave()

    Public Event ShDragDrop(ByVal DragList As ArrayList, _
                            ByVal Node As Object, _
                            ByVal grfKeyState As Integer, _
                            ByVal pdwEffect As Integer)   'note:control cannot change

#End Region

#Region "   Public Enum -- KeyStates"
    Public Enum KeyStates
        LButtonMask = 1
        RButtonMask = 2
        ShiftMask = 4
        CtrlMask = 8
        MButtonMask = 16
        AltMask = 32
    End Enum
#End Region

#Region "   Constructor"
    Public Sub New(ByVal ctl As Control)
        If ctl.GetType.Equals(GetType(TreeView)) Then
            m_IsTreeView = True
        ElseIf ctl.GetType.Equals(GetType(ListView)) Then
            m_IsTreeView = False
        Else
            m_IsTreeView = False
            Throw New Exception("CDragWrapper cannot handle " & ctl.GetType.FullName)
        End If
        m_View = ctl
        'set up the Event handlers
        AddHandler m_View.DragEnter, AddressOf DragEnter
        AddHandler m_View.DragLeave, AddressOf DragLeave
        AddHandler m_View.DragOver, AddressOf DragOver
        AddHandler m_View.DragDrop, AddressOf DragDrop

    End Sub
#End Region

#Region "   ResetPreviousTarget -- a utility/cleanup Method"
    Private Sub ResetPrevTarget()
        If Not IsNothing(m_LastTarget) Then
            Dim hr As Integer = m_LastTarget.DragLeave
            Marshal.ReleaseComObject(m_LastTarget)
            m_LastTarget = Nothing
        End If
        m_LastNode = Nothing
    End Sub

#End Region

#Region "       DragEnter"

    Private Sub DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs)
        Debug.WriteLine("Enter DragEnter  AllowedEffect = " & e.AllowedEffect.ToString)
        Dim fmts() As String = e.Data.GetFormats
        Dim F As String
        For Each F In fmts
            Dim stmp As String
            Debug.Write("Fmt = " & F & " Present? " & e.Data.GetDataPresent(F))
            If e.Data.GetData(F) Is Nothing Then
                stmp = " Actual Data is Nothing"
                Debug.WriteLine(stmp)
            Else
                stmp = " Actual Data is " & e.Data.GetData(F).GetType.Name
                Debug.WriteLine(stmp)
                If F.StartsWith("Shell") Or F.StartsWith("Uniform") Or _
                   F.StartsWith("FileG") Then
                    Debug.WriteLine("DataFormat " & F)
                    Dim ms As System.IO.MemoryStream = e.Data.GetData(F)
                    Dim b() As Byte = ms.ToArray
                    DumpHex(b)
                End If
            End If
        Next
        Dim comDO As New CMyDataObject(e.Data)
        If Not comDO.IsValid Then
            e.Effect = DragDropEffects.None
        Else
            m_DragData = comDO.DataObject
            m_DragList = comDO.DragList
            RaiseEvent ShDragEnter(m_DragList, e)
        End If
    End Sub

#End Region

#Region "   DragLeave"
    Private Sub DragLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        Debug.WriteLine("Enter DragLeave")
        If Not m_DragData.Equals(IntPtr.Zero) Then
            Marshal.Release(m_DragData)
            m_DragData = IntPtr.Zero
        End If
        RaiseEvent ShDragLeave()
        ResetPrevTarget()
    End Sub
#End Region

#Region "  DragOver"
    Private Sub DragOver(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs)
        Debug.WriteLine("Enter DragOver")
        Dim tn As Object
        Dim ptClient As System.Drawing.Point = m_View.PointToClient(New System.Drawing.Point(e.X, e.Y))
        If m_IsTreeView Then
            tn = CType(m_View, TreeView).GetNodeAt(ptClient)
        Else
            tn = CType(m_View, ListView).GetItemAt(ptClient.X, ptClient.Y)
        End If
        If IsNothing(tn) Then  'not over a sub-element(TreeNode or ListViewItem)
            ResetPrevTarget()
        Else   'currently over Treenode or ListViewItem
            If Not IsNothing(m_LastNode) Then   'not the first, check if same
                If tn Is m_LastNode Then
                    'Debug.WriteLine("DragOver: Same node")
                    Exit Sub        'all set up anyhow
                Else
                    ResetPrevTarget()
                    m_LastNode = tn
                End If
            Else    'is the first
                ResetPrevTarget()   'just in case
                m_LastNode = tn     'save current node
            End If

            'Drag is now over a new node with new capabilities
            ' If the underlying CShItem is NOT a Folder, then
            '  it will be properly handled by the .IsDropTarget test below
            'IDropTarget::Drag... need a POINTL structure with screen coordinates
            Dim pt As ShellDll.POINT
            pt.x = e.X
            pt.y = e.Y
            'IDropTarget::Drag... use an in,out parameter for drageffects
            Dim pdwEffect As Integer = e.AllowedEffect
            Dim CSI As CShItem = tn.Tag
            If CSI.IsDropTarget Then
                m_LastTarget = CSI.GetDropTargetOf(m_View)
                If Not IsNothing(m_LastTarget) Then
                    Dim res As Integer = m_LastTarget.DragEnter(m_DragData, e.KeyState, pt, pdwEffect)
                    If res = 0 AndAlso pdwEffect <> DragDropEffects.None Then
                        pdwEffect = e.AllowedEffect    'reset to original
                        res = m_LastTarget.DragOver(e.KeyState, pt, pdwEffect)
                    End If
                    If res <> 0 Then
                        pdwEffect = 0   'error, so report None
                        ' and release the IDropTarget
                        Marshal.Release(m_DragData)
                        m_DragData = IntPtr.Zero
#If Debug Then
                        Marshal.ThrowExceptionForHR(res)
#End If
                    End If
                Else
                    pdwEffect = 0 'couldn't get IDropTarget, so report effect None
                End If
            Else  'Might not even be a Folder
                pdwEffect = 0   'couldn't get IDropTarget, so report effect None
            End If
            RaiseEvent ShDragOver(tn, ptClient, e.KeyState, pdwEffect)
            e.Effect = pdwEffect
        End If
    End Sub
#End Region

#Region "   DragDrop"
    Private Sub DragDrop(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DragEventArgs)
        Debug.WriteLine("Enter DragDrop -- e.Effect = " & e.Effect.ToString)
        Dim res As Integer
        Dim pdwEffect As Integer = e.AllowedEffect  'reset to original 
        Debug.Assert(Not IsNothing(m_LastTarget), "In DragDrop with LastTarget not set")
        'If e.Data.GetDataPresent("Shell IDList Array") Then
        '    Dim ms As System.IO.MemoryStream
        '    ms = e.Data.GetData("Shell IDList Array")
        '    Dim b(ms.Length - 1) As Byte
        '    ms.Seek(0L, IO.SeekOrigin.Begin)
        '    Dim R = ms.Read(b, 0, ms.Length)
        '    DumpHex(b)
        'End If
        If Not IsNothing(m_LastTarget) Then
            Debug.Assert(e.Effect <> 0, "In DragDrop with pdwEffect = 0")

            'IDropTarget::Drag... need a POINTL structure with screen coordinates
            Dim pt As ShellDll.POINT
            pt.x = e.X
            pt.y = e.Y
            Debug.WriteLine("DragDrop prior to Drop on Folder -- pdwEffect = " & pdwEffect)
            Try
                res = m_LastTarget.DragDrop(m_DragData, e.KeyState, pt, pdwEffect)
            Catch ex As Exception
                Debug.WriteLine("Exception:" & ex.ToString)
                res = 255    'ensure we take error path
                e.Effect = DragDropEffects.None
                pdwEffect = e.Effect
            End Try
            Debug.WriteLine("DragDrop after Drop on Folder -- " & _
                            "res = " & res & vbCrLf & "pdwEffect = " & pdwEffect & _
                            vbCrLf & "e.Effect = " & e.Effect)
            If res <> 0 Then
                Debug.WriteLine("Error in dropping on DropTarget. res = " & Hex(res))
                'in case of error, let the control know
                RaiseEvent ShDragDrop(New ArrayList(), m_LastNode, e.KeyState, DragDropEffects.None)
            Else
                'Since I am passing the COM interface of the .Net IDataObject, the
                ' operation completes before return. Therefore, the DragDropEffect 
                'returned in pdwEffect can be relied on  EXCEPT in the case of
                'an Optimized Move -- Normally the e.Effect=2 and pdwEffect=0 in
                ' this case -- indicating that the Move may still be happening
                ' OR that it will never happen -- tell the control what
                ' we intended to happen and hope for the best.
                If e.Effect = DragDropEffects.Move And pdwEffect = 0 Then
                    pdwEffect = e.Effect
                End If

                RaiseEvent ShDragDrop(m_DragList, m_LastNode, e.KeyState, pdwEffect)
            End If
            e.Effect = pdwEffect
        End If
        ResetPrevTarget()
        Marshal.Release(m_DragData)  'get rid of cnt added in DragEnter
        m_DragData = IntPtr.Zero
    End Sub
#End Region

#Region "   Make Shell ID Array (CIDA)"
    '''<Summary>Shell Folders prefer their IDragData to contain this format which is
    '''  NOT directly supported by .Net.  The underlying structure is the CIDA structure
    '''  which is basically VB & VB.Net Hostile.
    '''If "Make ShortCut(s) here" is the desired or
    '''  POSSIBLE effect of the drag, then this format is REQUIRED -- otherwise the
    '''  Folder will interpret the DragDropEffects.Link to be "Create Document Shortcut"
    '''  which is NEVER the desired effect in this case
    ''' The normal CIDA contains the Absolute PIDL of the source Folder and 
    '''  Relative PIDLs for each Item in the Drag. 
    '''  I cheat a bit an provide the Absolute PIDL of the Desktop (00, a short)
    '''  and the Absolute PIDLs for the Items (all such Absolute PIDLS ar 
    '''  relative to the Desktop.
    ''' </Summary>
    '''<Credit>http://www.dotnetmonster.com/Uwe/Forum.aspx/dotnet-interop/3482/Drag-and-Drop
    '''  The overall concept and much code taken from the above reference
    ''' Dave Anderson's response, translated from C# to VB.Net, was the basis
    ''' of this routine
    ''' An AHA momemnt and a ref to the above url came from
    '''http://www.Planet-Source-Code.com/vb/scripts/ShowCode.asp?txtCodeId=61324&lngWId=1
    '''
    '''</Credit>
    Public Shared Function MakeShellIDArray(ByVal CSIList As ArrayList) As System.IO.MemoryStream
        'ensure that we have an arraylist of only CShItems
        Dim AllowedType As Type = GetType(CShItem)
        Dim oCSI As Object
        For Each oCSI In CSIList
            If Not AllowedType.Equals(oCSI.GetType) Then
                Return Nothing
            End If
        Next
        'ensure at least one item
        If CSIList.Count < 1 Then Return Nothing

        'bArrays is an Array of Byte() each containing the bytes of a PIDL
        Dim bArrays(CSIList.Count - 1) As Object
        Dim CSI As CShItem
        Dim i As Integer = 0
        For Each CSI In CSIList
            bArrays(i) = New cPidl(CSI.PIDL).PidlBytes
        Next

        MakeShellIDArray = New System.IO.MemoryStream()
        Dim BW As New System.IO.BinaryWriter(MakeShellIDArray)

        BW.Write(Convert.ToUInt32(CSIList.Count))   'we don't count the parent (Desktop)
        Dim Desktop As Integer  'we only use the lowval 2 bytes (VB lacks meaninful uint)
        Dim Offset As Integer   'offset into Structure of a PIDL

        ' Calculate and write the offset to each pidl (defined as an array of uint32)
        ' The first pidl is 2 bytes long (0 0) and represents the desktop
        ' The 2 in the statement below is for the offset to the 
        ' folder pidl and the count field in the CIDA structure
        Offset = Marshal.SizeOf(GetType(UInt32)) * (bArrays.Length + 2)
        BW.Write(Convert.ToUInt32(Offset))       'offset to desktop pidl
        Offset += 2 'Marshal.SizeOf(GetType(UInt16)) 'point to the next one
        For i = 0 To bArrays.Length - 1
            BW.Write(Convert.ToUInt32(Offset))
            Offset += CType(bArrays(i), Byte()).Length
        Next
        'done with the array of offsets, write the parent pidl (0 0) = Desktop
        BW.Write(Convert.ToUInt16(Desktop))

        'Write the pidl bytes
        Dim b() As Byte
        For Each b In bArrays
            BW.Write(b)
        Next

        'done, returning the memorystream
        Debug.WriteLine("Done MakeShellIDArray")
    End Function
#End Region
End Class
