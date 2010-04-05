
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports ExpTreeLib.ShellDll
Imports ExpTreeLib.CShItem

Public Class ComDataObject
    Implements IDisposable

#Region "   Instance Private Fields"
    Private m_Folder As CShItem                     'the Item to which this belongs
    Private m_DataObject As ShellDll.IDataObject    'the COM Interface
    Private m_hg As IntPtr                          'a block of HGLobal mem MUST FREE
    Private m_IsValid As Boolean = False            'True if obtained with no error
    Private m_Disposed As Boolean = False           'True if Item Disposed
#End Region

#Region "   Destructor"
    ''' <summary>
    ''' Summary of Dispose.
    ''' </summary>
    ''' 
    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        ' Take yourself off of the finalization queue
        ' to prevent finalization code for this object
        ' from executing a second time.
        GC.SuppressFinalize(Me)
    End Sub
    ''' <summary>
    ''' Release the COM IDataObject Interface
    ''' </summary>
    ''' <param name=disposing></param>
    ''' 
    Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
        ' Allow your Dispose method to be called multiple times,
        ' but throw an exception if the object has been disposed.
        ' Whenever you do something with this class, 
        ' check to see if it has been disposed.
        If Not (m_Disposed) Then
            ' If disposing equals true, dispose all managed 
            ' and unmanaged resources.
            m_Disposed = True
            If (disposing) Then
            End If
            ' Release unmanaged resources. If disposing is false,
            ' only the following code is executed. 
            If Not IsNothing(m_DataObject) Then
                Marshal.ReleaseComObject(m_DataObject)
            End If
            If Not m_hg.Equals(IntPtr.Zero) Then
                Marshal.FreeHGlobal(m_hg)
            End If
        Else
            Throw New Exception("ComDataObject Disposed more than once")
        End If
    End Sub

    ' This Finalize method will run only if the 
    ' Dispose method does not get called.
    ' By default, methods are NotOverridable. 
    ' This prevents a derived class from overriding this method.
    ''' <summary>
    ''' Summary of Finalize.
    ''' </summary>
    ''' 
    Protected Overrides Sub Finalize()
        ' Do not re-create Dispose clean-up code here.
        ' Calling Dispose(false) is optimal in terms of
        ' readability and maintainability.
        Dispose(False)
    End Sub


#End Region

    'UINT RegisterClipboardFormat(LPCTSTR lpszFormat)

    Declare Auto Function RegisterClipboardFormat Lib "User32" (ByVal lpszFormat As String) As Integer

    'int GetClipboardFormatName(          UINT format,
    'LPTSTR lpszFormatName,
    'int cchMaxCount

    Declare Auto Function GetClipboardFormatName Lib "User32" (ByVal format As Integer, _
                    <Out(), MarshalAs(UnmanagedType.LPTStr)> _
                      ByRef lpszFormatName As StringBuilder, _
                      ByVal cchMaxCount As Integer) As Integer
#Region "   Constructor"
    Sub New(ByVal CSI As CShItem, ByVal CSIList As ArrayList)
        Dim HR As Integer
        Try
            'HR = Me.Parent.m_Folder.GetUIObjectOf(tnH, 1, apidl, ShellDll.IID_IDropTarget, 0, theInterface)
            Dim apidl(0) As IntPtr
            apidl(0) = CShItem.GetLastID(CSI.PIDL)
            HR = CSI.Parent.Folder.GetUIObjectOf(IntPtr.Zero, 1, apidl, ShellDll.IID_IDataObject, 0, m_DataObject)

            'HR = CSI.Folder.CreateViewObject(IntPtr.Zero, IID_IDataObject, m_DataObject)
            If HR = 0 Then
                m_IsValid = True
            Else
                Marshal.ThrowExceptionForHR(HR)
            End If
        Catch ex As Exception
        End Try
        If m_IsValid Then
            Dim cf As Integer = RegisterClipboardFormat("Shell IDList Array")
            If cf <> 0 Then
                Dim mem As MemoryStream
                mem = CDragWrapper.MakeShellIDArray(CSIList)
                Dim fmtetc As New FORMATETC()
                Dim stg As New STGMEDIUM()
                With fmtetc
                    .cfFormat = cf
                    .lindex = -1
                    .dwAspect = 1
                    .ptd = IntPtr.Zero
                    .Tymd = TYMED.HGLOBAL
                End With
                m_hg = Marshal.AllocHGlobal(mem.Length)
                Marshal.Copy(mem.ToArray, 0, m_hg, mem.Length)
                With stg
                    .tymed = TYMED.HGLOBAL
                    .hGlobal = m_hg
                    .pUnkForRelease = IntPtr.Zero
                End With

                HR = m_DataObject.SetData(fmtetc, stg, True)

                Dim fEnum As IEnumFORMATETC

                HR = m_DataObject.EnumFormatEtc(2, fEnum)
                If HR = NOERROR Then
                    Dim fmt As New FORMATETC()
                    Dim itemCnt As Integer
                    HR = fEnum.GetNext(1, fmt, itemCnt)
                    If HR = NOERROR Then
                        Do While itemCnt > 0
                            Debug.WriteLine("Returned FMT. = " & fmt.cfFormat)
                            Dim SB As New StringBuilder(256)
                            Try
                                HR = GetClipboardFormatName(fmt.cfFormat, SB, 256)
                            Catch ex As Exception
                                Debug.WriteLine(ex.ToString)
                            End Try
                            If HR > 0 Then      'zero is failed to get
                                Debug.WriteLine("Format is: " & SB.ToString)
                            End If
                            If Not fmt.ptd.Equals(IntPtr.Zero) Then
                                Marshal.FreeCoTaskMem(fmt.ptd)
                            End If
                            HR = fEnum.GetNext(1, fmt, itemCnt)
                        Loop
                        Marshal.ReleaseComObject(fEnum)
                    Else
                    End If
                Else
                End If
            Else
            End If
        End If
    End Sub
#End Region
End Class
