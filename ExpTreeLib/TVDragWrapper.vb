Imports System.Text
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports ExpTreeLib.ShellDll

Public Class TVDragWrapper
    Implements ShellDll.IDropTarget
#Region "   Private Fields"
    Private m_View As Control
    Private m_Original_Effect As Integer    'Save it
    Private m_OriginalRefCount As Integer   'Set in DragEnter, used in DragDrop
    Private m_DragDataObj As IntPtr         'Saved on DragEnter for use in DragOver
    Private m_LastTarget As ExpTreeLib.ShellDll.IDropTarget     'Of most recent Folder dragged over
    Private m_LastNode As Object            'Most recent node dragged over
    Private m_DropList As ArrayList         'CShItems of Items dragged/dropped
    Private m_MyDataObject As CProcDataObject 'Does parsing of dragged IDataObject
#End Region

#Region "   Public Events"
    Public Event ShDragEnter(ByVal DragItemList As ArrayList, _
                                ByVal pDataObj As IntPtr, _
                                ByVal grfKeyState As Integer, _
                                ByVal pdwEffect As Integer)

    Public Event ShDragOver(ByVal Node As Object, _
                                ByVal ClientPoint As System.Drawing.Point, _
                                ByVal grfKeyState As Integer, _
                                ByVal pdwEffect As Integer)
    Public Event ShDragLeave()

    Public Event ShDragDrop(ByVal DragItemList As ArrayList, _
                            ByVal Node As Object, _
                            ByVal grfKeyState As Integer, _
                            ByVal pdwEffect As Integer)

#End Region

#Region "   Constructor"
    Public Sub New(ByVal ctl As TreeView)
        m_View = ctl
    End Sub
#End Region

    Private Sub ResetPrevTarget()
        If Not IsNothing(m_LastTarget) Then
            'ShowCnt("RSP Prior to LT.DragLeave")
            Dim hr As Integer = m_LastTarget.DragLeave
            'ShowCnt("RSP After LT.DragLeave")
            Marshal.ReleaseComObject(m_LastTarget)
            'ShowCnt("RSP After Release of LT")
            m_LastTarget = Nothing
            m_LastNode = Nothing
        End If
    End Sub

    Public Function DragEnter(ByVal pDataObj As IntPtr, _
                                ByVal grfKeyState As Integer, _
                                ByVal pt As POINT, _
                                ByRef pdwEffect As Integer) As Integer _
                        Implements ExpTreeLib.ShellDll.IDropTarget.DragEnter
        Debug.WriteLine("In DragEnter: Effect = " & pdwEffect & " Keystate = " & grfKeyState)
        m_Original_Effect = pdwEffect
        m_DragDataObj = pDataObj
        m_OriginalRefCount = Marshal.AddRef(m_DragDataObj)  'note: includes our count
        Debug.WriteLine("DragEnter: pDataObj RefCnt = " & m_OriginalRefCount)

        m_MyDataObject = New CProcDataObject(pDataObj)

        If m_MyDataObject.IsValid Then
            m_DropList = m_MyDataObject.DragList
            RaiseEvent ShDragEnter(m_DropList, pDataObj, grfKeyState, pdwEffect)
        Else
            pdwEffect = System.Windows.Forms.DragDropEffects.None
        End If
        Return 0

    End Function

    'Private Sub ShowCnt(ByVal S As String)
    '    If Not IsNothing(m_DragData) Then
    '        Debug.WriteLine(S & " RefCnt = " & Marshal.AddRef(m_DragData) - 1)
    '        Marshal.Release(m_DragData)
    '    End If
    'End Sub

    Public Function DragOver(ByVal grfKeyState As Integer, _
                                ByVal pt As POINT, _
                                ByRef pdwEffect As Integer) As Integer _
                        Implements ExpTreeLib.ShellDll.IDropTarget.DragOver
        'Debug.WriteLine("In DragOver: Effect = " & pdwEffect & " Keystate = " & grfKeyState)

        Dim tn As Object
        Dim ptClient As System.Drawing.Point = m_View.PointToClient(New System.Drawing.Point(pt.x, pt.y))
        tn = CType(m_View, TreeView).GetNodeAt(ptClient)
        If IsNothing(tn) Then  'not over a TreeNode
            ResetPrevTarget()
        Else   'currently over Treenode
            If Not IsNothing(m_LastNode) Then   'not the first, check if same
                If tn Is m_LastNode Then
                    'Debug.WriteLine("DragOver: Same node")
                    Return 0        'all set up anyhow
                Else
                    ResetPrevTarget()
                    m_LastNode = tn
                End If
            Else    'is the first
                ResetPrevTarget()   'just in case
                m_LastNode = tn     'save current node
            End If

            'Drag is now over a new node with new capabilities

            Dim CSI As CShItem = tn.Tag
            If CSI.IsDropTarget Then
                m_LastTarget = CSI.GetDropTargetOf(m_View)
                If Not IsNothing(m_LastTarget) Then
                    pdwEffect = m_Original_Effect
                    'ShowCnt("Prior to LT.DragEnter")
                    Dim res As Integer = m_LastTarget.DragEnter(m_DragDataObj, grfKeyState, pt, pdwEffect)
                    If res = 0 Then
                        'ShowCnt("Prior to LT.DragOver")
                        res = m_LastTarget.DragOver(grfKeyState, pt, pdwEffect)
                        'ShowCnt("After LT.DragOver")
                    End If
                    If res <> 0 Then
                        Marshal.ThrowExceptionForHR(res)
                    End If
                Else
                    pdwEffect = 0 'couldn't get IDropTarget, so report effect None
                End If
            Else
                pdwEffect = 0   'CSI not a drop target, so report effect None
            End If
            RaiseEvent ShDragOver(tn, ptClient, grfKeyState, pdwEffect)
        End If
        Return 0
    End Function

    Public Function DragLeave() As Integer Implements ExpTreeLib.ShellDll.IDropTarget.DragLeave
        'Debug.WriteLine("In DragLeave")
        m_Original_Effect = 0
        ResetPrevTarget()
        Dim cnt As Integer = Marshal.Release(m_DragDataObj)
        Debug.WriteLine("DragLeave: cnt = " & cnt)
        m_DragDataObj = IntPtr.Zero
        m_OriginalRefCount = 0      'just in case
        m_MyDataObject = Nothing
        RaiseEvent ShDragLeave()
        Return 0
    End Function

    Public Function DragDrop(ByVal pDataObj As IntPtr, _
                                ByVal grfKeyState As Integer, _
                                ByVal pt As POINT, _
                                ByRef pdwEffect As Integer) As Integer _
                            Implements ExpTreeLib.ShellDll.IDropTarget.DragDrop
        'Debug.WriteLine("In DragDrop: Effect = " & pdwEffect & " Keystate = " & grfKeyState)
        Dim res As Integer
        If Not IsNothing(m_LastTarget) Then
            res = m_LastTarget.DragDrop(pDataObj, grfKeyState, pt, pdwEffect)
            'version 21 change 
            If res <> 0 AndAlso res <> 1 Then
                Debug.WriteLine("Error in dropping on DropTarget. res = " & Hex(res))
            End If 'No error on drop
            ' it is quite possible that the actual Drop has not completed.
            ' in fact it could be Canceled with nothing happening.
            ' All we are going to do is hope for the best
            ' The documented norm for Optimized Moves is pdwEffect=None, so leave it
            RaiseEvent ShDragDrop(m_DropList, m_LastNode, grfKeyState, pdwEffect)
        End If
        ResetPrevTarget()
        Dim cnt As Integer = Marshal.Release(m_DragDataObj)  'get rid of cnt added in DragEnter
        m_DragDataObj = IntPtr.Zero
        Return 0
    End Function
End Class
