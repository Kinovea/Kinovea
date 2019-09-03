Imports System.Runtime.InteropServices
Imports System.IO
Imports System.Text
Imports ExpTreeLib.CShItem
Imports ExpTreeLib.ShellDll

'''<Summary>This class takes the IDataObject or .Net DataObjectpassed into DragEnter
'''  and builds a IDataObject that is suitable for use when interacting with the
''' IDropTarget of a Folder.
'''Requirements:
'''  The input IDataObject must contain at least one of the following Formats and Data
'''   1. An ArrayList of CShItems
'''   2. A Shell IDList Array
'''   3. A FileDrop structure
''' A Valid instance of this class will have and expose
'''  1. Dobj -- A COM IDataObject suitable for use in interaction with a Folder's IDropTarget
'''             Dobj will have, at least, a valid Shell IDList Array representing the Dragged Items
'''  2. DragData -- An ArrayList of 1 or more CShItems representing the Dragged Items
'''Processing Steps:
'''  1. Check for presence of one or more of the required Formats with valid Data
'''  2. Build or use the provided ArrayList of CShItems
'''  3. Ensure that all items are of same FileSystem/Non-FileSystem classification
'''  4. Build or use the provided Shell IDList Array 
'''  (Note that we don't necessarily build the FileDrop structure)
'''  5. if classification is FileSystem 
'''  5a.   Store Shell IDList Array into DataObject, if not already there
'''  5b.   Obtain COM IDataObject
'''  6, Else for non-FileSystem classification
'''  6a.   Obtain the IDataObject of the Virtual Folder  (A COM object)
'''  6b.   Store Shell IDList Array into that Object
'''  7. If no errors to this point, set m_IsValid to True
'''  8. Done
''' The class also contains a number of useful shared methods for dealing with
''' IDataObject 
''' </Summary>


Public Class CProcDataObject
#Region "   Private Fields"
    Private m_DataObject As IntPtr          'The built ptr to COM IDataObject
    Private m_Draglist As New ArrayList()   'The built list of items in the original
    Private m_IsValid As Boolean = False    'True once m_DataObject & m_Droplist are OK
    Private m_StreamCIDA As MemoryStream    'A memorystream containing a CIDA
    Private IsNet As Boolean = False        'True if dealing with a .Net DataObject
    Private NetIDO As System.Windows.Forms.IDataObject
    Private IDO As ShellDll.IDataObject

#End Region

#Region "   Public Properties"
    Public ReadOnly Property DataObject() As IntPtr
        Get
            Return m_DataObject
        End Get
    End Property
    Public ReadOnly Property DragList() As ArrayList
        Get
            Return m_Draglist
        End Get
    End Property
    Public ReadOnly Property IsValid() As Boolean
        Get
            Return m_IsValid
        End Get
    End Property
#End Region

#Region "   Contructors"
    '''<Summary>Constructor starting with a .Net Data object
    ''' .Net DataObjects are easy to work with, but are useless
    ''' if the Dragged items are non-FileSystem
    ''' The DragWrapper class will never call this.
    ''' It is here for playing around with the CDragWrapper class
    ''' </Summary>
    Sub New(ByRef NetObject As System.Windows.Forms.DataObject)
        NetIDO = NetObject
        ProcessNetDataObject(NetObject)
        If m_IsValid Then
            Try
                m_DataObject = Marshal.GetComInterfaceForObject(NetObject, Type.GetTypeFromCLSID(New Guid("0000010e-0000-0000-C000-000000000046"), True))
            Catch ex As Exception
                Debug.WriteLine("Failed to get COM IDataObject:" & ex.ToString)
                m_DataObject = IntPtr.Zero
                m_Draglist = New ArrayList()  'let GC clean em up
                m_IsValid = False
            End Try
        End If
    End Sub
    '''<Summary>This constructor takes a pointer to an IDataObject obtained from a
    ''' IDropTarget Interface's DragEnter method.
    ''' If the pointer points to a .Net DataObject (which only happens within the same app),
    ''' convert it and call ProcessNetDataObject
    ''' Otherwise, it from another app, possibly Win Explorer, so
    ''' check it for the required formats and build m_DragList.
    ''' Any error just quits, leaving m_IsValid as False ... Caller must check
    '''</Summary>
    Sub New(ByRef pDataObj As IntPtr)   'Assumed to be a pointer to an IDataObject
        'save it off -- DragWrapper class won't use it
        '              CDragWrapper class may, but should, in this version, use the original
        m_DataObject = pDataObj
        Dim HadError As Boolean = False     'used for various error conditions
        Try
            IDO = Marshal.GetTypedObjectForIUnknown(pDataObj, GetType(ShellDll.IDataObject))
        Catch ex As Exception
            ' Debug.WriteLine("Exception Thrown in CMyDataObject -Getting COM interface: " & vbCrLf & ex.ToString)
            HadError = True
        End Try
        'If it is really a .Net IDataObject, then treat it as such
        If HadError Then
            Try
                NetIDO = Marshal.GetTypedObjectForIUnknown(pDataObj, GetType(System.Windows.Forms.IDataObject))
                IsNet = True
            Catch
                IsNet = False
            End Try
        End If
        If IsNet Then
            'Any error in ProcessNetDataObject will leave m_IsValid as False -- our only Error Indicator
            ProcessNetDataObject(NetIDO)
        Else    'IDataObject not from Net, Do it the hard way
            If HadError Then Exit Sub 'can do no more
            ProcessCOMIDataObject(IDO)
            'It either worked or not.  m_IsValid is set accordingly, so we are done
        End If
    End Sub

#Region "   ProcessCOMIDataObject"

    '''<Summary>Given an IDataObject from some non-net source, Build the 
    ''' m_DragList ArrayList. 
    ''' Also ensure that the IDataObject has "Shell IDList Array" formatted data
    ''' If not, build it in m_StreamCIDA, if so, copy to m_StreamCIDA
    '''If dealing with all FileSystem Items,
    ''' </Summary> 
    Private Sub ProcessCOMIDataObject(ByVal IDO As ShellDll.IDataObject)
        'Don't even look for an ArrayList. If there, we don't know how to access
        'Therefore, we need either a "FileDrop" or a "Shell IDList Array" to 
        'extract the info for m_DragList and to ensure that the IDataObject
        'actually has a "Shell IDList Array"

        Dim HR As Integer       'general use response variable
        Dim fmtEtc As FORMATETC
        Dim stg As STGMEDIUM
        'ensure m_StreamCIDA is nothing  - will test for this later
        m_StreamCIDA = Nothing
        'First check for "Shell IDList Array" -- preferred in this case (and most others)
        Dim cf As Integer = RegisterClipboardFormat("Shell IDList Array")
        If cf <> 0 Then
            With fmtEtc
                .cfFormat = cf
                .lindex = -1
                .dwAspect = ShellDll.DVASPECT.CONTENT
                .ptd = IntPtr.Zero
                .Tymd = TYMED.HGLOBAL
            End With
            With stg
                .hGlobal = IntPtr.Zero
                .pUnkForRelease = IntPtr.Zero
                .tymed = TYMED.HGLOBAL
            End With
            HR = IDO.GetData(fmtEtc, stg)
            If HR = 0 Then
                Dim cidaptr As IntPtr = Marshal.ReadIntPtr(stg.hGlobal)
                m_StreamCIDA = MakeStreamFromCIDA(cidaptr)
                MakeDragListFromCIDA()
                ReleaseStgMedium(stg)       'done with this
            Else
                Try
                    Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                End Try
            End If
        End If

        'Check for "FileDrop" (CF.HDROP) if we have to
        If m_Draglist.Count < 1 Then     'skip this if already have list
            With fmtEtc
                .cfFormat = ShellDll.CF.HDROP
                .lindex = -1
                .dwAspect = ShellDll.DVASPECT.CONTENT
                .ptd = IntPtr.Zero
                .Tymd = TYMED.HGLOBAL
            End With
            With stg
                .hGlobal = IntPtr.Zero
                .pUnkForRelease = IntPtr.Zero
                .tymed = 0
            End With
            HR = IDO.GetData(fmtEtc, stg)
            If HR = 0 Then      'we have an HDROP and stg.hGlobal points to the info
                Dim pHdrop As IntPtr = Marshal.ReadIntPtr(stg.hGlobal)
                Dim nrItems As Integer = DragQueryFile(pHdrop, -1, Nothing, 0)
                Dim i As Integer
                For i = 0 To nrItems - 1
                    Dim plen As Integer = DragQueryFile(pHdrop, i, Nothing, 0)
                    Dim SB As New StringBuilder(plen + 1)
                    Dim flen As Integer = DragQueryFile(pHdrop, i, SB, plen + 1)
                    Debug.WriteLine("Fetched from HDROP: " & SB.ToString)
                    Try  'if GetCShitem returns Nothing(it's failure marker) then catch it
                        m_Draglist.Add(GetCShItem(SB.ToString))
                    Catch ex As Exception  ' in this case, just skip it
                        Debug.WriteLine("Error: CMyDataObject.ProcessComIDataObject - Adding via HDROP" & vbCrLf & vbTab & ex.Message)
                    End Try
                Next
                ReleaseStgMedium(stg)  'done with this stg
                'Else
                '    Marshal.ThrowExceptionForHR(HR)
            End If
        End If
        'Have done what we can to get m_DragList -- exit on failure
        If m_Draglist.Count < 1 Then  'Can't do any more -- Quit
            Exit Sub                  'leaving m_IsValid as False
        End If

        'May not have Shell IDList Array in IDataObject.  If not, give it one
        If IsNothing(m_StreamCIDA) Then  'IDO does not yet have "Shell IDList Array" Data
            m_StreamCIDA = MakeShellIDArray(m_Draglist)
            If IsNothing(m_StreamCIDA) Then Exit Sub 'failed to make it
            'Now put the CIDA into the original IDataObject
            With fmtEtc
                .cfFormat = cf      'registered at routine entry
                .lindex = -1
                .dwAspect = ShellDll.DVASPECT.CONTENT
                .ptd = IntPtr.Zero
                .Tymd = TYMED.HGLOBAL
            End With
            'note the hGlobal item in stg is a pointer to a pointer->Data
            Dim m_hg As IntPtr = Marshal.AllocHGlobal(Convert.ToInt32(m_StreamCIDA.Length))
            Marshal.Copy(m_StreamCIDA.ToArray, 0, m_hg, m_StreamCIDA.Length)
            Dim hg As IntPtr = Marshal.AllocHGlobal(IntPtr.Size)
            Marshal.WriteIntPtr(hg, 0, m_hg)
            With stg
                .tymed = TYMED.HGLOBAL
                .hGlobal = hg
                .pUnkForRelease = IntPtr.Zero
            End With

            HR = IDO.SetData(fmtEtc, stg, True)   'callee responsible for releasing stg
            If HR = 0 Then
                m_IsValid = True
            Else   'failed -- we have to release stg  -- and leave m_IsValid as False
                ReleaseStgMedium(stg)
                Exit Sub        'm_isvalid stays False
            End If
        End If
        m_IsValid = True  'already had a Shell IDList Array, so all is OK
    End Sub
#End Region

#Region "   ProcessNetDataObject"
    'Note: GetdataPresent is not reliable for IDataObject that did not originate 
    'from .Net. This rtn is called if it did, but we still don't use GetdataPresent.
    Private Sub ProcessNetDataObject(ByVal NetObject As System.Windows.Forms.IDataObject)
        If Not IsNothing(NetObject.GetData(GetType(ArrayList))) Then
            Dim AllowedType As Type = GetType(CShItem)
            Dim oCSI As Object
            For Each oCSI In NetObject.GetData(GetType(ArrayList))
                If Not AllowedType.Equals(oCSI.GetType) Then
                    m_Draglist = New ArrayList()
                    Exit For
                Else
                    m_Draglist.Add(oCSI)
                End If
            Next
        End If

        'Shell IDList Array is preferred to HDROP, see if we have one
        If Not IsNothing(NetObject.GetData("Shell IDList Array")) Then
            'Get it and also mark that we had one
            m_StreamCIDA = NetObject.GetData("Shell IDList Array")
            'has one, ASSUME that it matchs what we may have gotten from 
            ' ArrayList, if we had one of those
            If m_Draglist.Count < 1 Then    'if we didn't have an ArrayList, have to build m_DragList
                If Not MakeDragListFromCIDA() Then    'Could not make it
                    Exit Sub        'leaving m_IsValid as false
                End If
            End If
        End If
        'FileDrop is only used to build m_DragList if not already done
        If m_Draglist.Count < 1 Then
            If Not IsNothing(NetObject.GetData("FileDrop")) Then
                Dim S As String
                For Each S In NetObject.GetData("FileDrop", True)
                    Try   'if GetCShitem returns Nothing(it's failure marker) then catch it
                        m_Draglist.Add(GetCShItem(S))
                    Catch ex As Exception   'Some problem, throw the whole thing away
                        Debug.WriteLine("CMyDataObject -- Error in creating CShItem for " & S & _
                                        vbCrLf & "Error is: " & ex.ToString)
                        m_Draglist = New ArrayList()
                    End Try
                Next
            End If
        End If
        'At this point we must have a valid m_DragList
        If m_Draglist.Count < 1 Then Exit Sub 'no list, not valid

        'ensure that DataObject has a Shell IDList Array
        If IsNothing(m_StreamCIDA) Then        'wouldn't be Nothing if it had one
            m_StreamCIDA = MakeShellIDArray(m_Draglist)
            NetObject.SetData("Shell IDList Array", True, m_StreamCIDA)
        End If
        'At this point, we have a valid DragList and have ensured that the DataObject
        ' has a CIDA.  
        m_IsValid = True
        Exit Sub

    End Sub
#End Region

#End Region

#Region "   Make Shell ID Array (CIDA)"
    '''<Summary>
    ''' Shell Folders prefer their IDragData to contain this format which is
    '''  NOT directly supported by .Net.  The underlying structure is the CIDA structure
    '''  which is basically VB and VB.Net Hostile.
    '''If "Make ShortCut(s) here" is the desired or
    '''  POSSIBLE effect of the drag, then this format is REQUIRED -- otherwise the
    '''  Folder will interpret the DragDropEffects.Link to be "Create Document Shortcut"
    '''  which is NEVER the desired effect in this case
    ''' The normal CIDA contains the Absolute PIDL of the source Folder and 
    '''  Relative PIDLs for each Item in the Drag. 
    '''  I cheat a bit an provide the Absolute PIDL of the Desktop (00, a short)
    '''  and the Absolute PIDLs for the Items (all such Absolute PIDLS ar 
    '''  relative to the Desktop.
    '''</Summary>
    '''<Credit>http://www.dotnetmonster.com/Uwe/Forum.aspx/dotnet-interop/3482/Drag-and-Drop
    '''  The overall concept and much code taken from the above reference
    ''' Dave Anderson's response, translated from C# to VB.Net, was the basis
    ''' of this routine
    ''' An AHA momemnt and a ref to the above url came from
    '''http://www.Planet-Source-Code.com/vb/scripts/ShowCode.asp?txtCodeId=61324%26lngWId=1
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
            i += 1
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

#Region "   MakeStreamFromCIDA"
    '''<Summary>Given an IntPtr pointing to a CIDA,
    ''' copy the CIDA to a new MemoryStream</Summary>
    Private Function MakeStreamFromCIDA(ByVal ptr As IntPtr) As MemoryStream
        MakeStreamFromCIDA = Nothing    'assume failure
        If ptr.Equals(IntPtr.Zero) Then Exit Function
        Dim nrItems As Integer = Marshal.ReadInt32(ptr, 0)
        If Not (nrItems > 0) Then Exit Function
        Dim offsets(nrItems) As Integer
        Dim curB As Integer = 4 'already read first 4
        Dim i As Integer
        For i = 0 To nrItems
            offsets(i) = Marshal.ReadInt32(ptr, curB)
            curB += 4
        Next
        Dim pidlLen As Integer
        Dim pidlobjs(nrItems) As Object
        For i = 0 To nrItems
            Dim ipt As New IntPtr(ptr.ToInt64 + offsets(i))
            Dim cp As New cPidl(ipt)
            pidlobjs(i) = cp.PidlBytes
            pidlLen += CType(pidlobjs(i), Byte()).Length
        Next
        MakeStreamFromCIDA = New MemoryStream(pidlLen + (4 * offsets.Length) + 4)
        Dim BW As New BinaryWriter(MakeStreamFromCIDA)
        With BW
            .Write(nrItems)
            For i = 0 To nrItems
                .Write(offsets(i))
            Next
            For i = 0 To nrItems
                .Write(CType(pidlobjs(i), Byte()))
            Next
        End With
        ' DumpHex(MakeStreamFromCIDA.ToArray)
        MakeStreamFromCIDA.Seek(0, SeekOrigin.Begin)
    End Function
#End Region

#Region "   MakeDragListFromCIDA"
    '''<Summary>Builds m_DragList from m_StreamCIDA</Summary>
    '''<returns> True if Successful, otherwise False</returns>
    Private Function MakeDragListFromCIDA() As Boolean
        MakeDragListFromCIDA = False    'assume failure
        If IsNothing(m_StreamCIDA) Then Exit Function
        Dim BR As New BinaryReader(m_StreamCIDA)
        Dim offsets(BR.ReadInt32 + 1) As Integer   '0=parent, last = total length
        offsets(offsets.Length - 1) = BR.BaseStream.Length
        Dim i As Integer
        For i = 0 To offsets.Length - 2
            offsets(i) = BR.ReadInt32
        Next
        Dim bArrays(offsets.Length - 2) As Object   'my objects are byte()
        For i = 0 To bArrays.Length - 1
            Dim thisLen As Integer = offsets(i + 1) - offsets(i)
            bArrays(i) = BR.ReadBytes(thisLen)
        Next
        m_Draglist = New ArrayList()
        For i = 1 To bArrays.Length - 1
            Dim isOK As Boolean = True
            Try   'if GetCShitem returns Nothing(it's failure marker) then catch it
                m_Draglist.Add(GetCShItem(bArrays(0), bArrays(i)))
            Catch ex As Exception
                Debug.Write("Error in making CShiTem from CIDA: " & ex.ToString)
                isOK = False
            End Try
            If Not isOK Then GoTo ERRXIT
        Next
        'on fall thru, all is done OK
        MakeDragListFromCIDA = True
        Exit Function

        'Error cleanup and Exit
ERRXIT: m_Draglist = New ArrayList()
        Debug.WriteLine("MakeDragListFromCIDA failed")
    End Function
#End Region


End Class
