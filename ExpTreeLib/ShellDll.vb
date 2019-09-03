
Imports System.Runtime.InteropServices
Imports System.Text

Public Class ShellDll

#Region "   Shell Constants"
    Public Const MAX_PATH As Integer = 260
    Public Const FILE_ATTRIBUTE_NORMAL As Integer = &H80
    Public Const FILE_ATTRIBUTE_DIRECTORY As Integer = &H10
    Public Const NOERROR As Integer = 0
    Public Const S_OK As Integer = 0
    Public Const S_FALSE As Integer = 1
#End Region

#Region "   Shell Enumerations"

#Region "   SFGAO"
    <Flags()> _
    Public Enum SFGAO
        CANCOPY = &H1                    ' Objects can be copied    
        CANMOVE = &H2                    ' Objects can be moved     
        CANLINK = &H4                    ' Objects can be linked    
        STORAGE = &H8                    ' supports BindToObject(IID_IStorage)
        CANRENAME = &H10                 ' Objects can be renamed
        CANDELETE = &H20                 ' Objects can be deleted
        HASPROPSHEET = &H40              ' Objects have property sheets
        DROPTARGET = &H100               ' Objects are drop target
        CAPABILITYMASK = &H177           ' This flag is a mask for the capability flags.
        ENCRYPTED = &H2000               ' object is encrypted (use alt color)
        ISSLOW = &H4000                  ' 'slow' object
        GHOSTED = &H8000                 ' ghosted icon
        LINK = &H10000                   ' Shortcut (link)
        SHARE = &H20000                  ' shared
        RDONLY = &H40000               ' read-only
        HIDDEN = &H80000                 ' hidden object
        DISPLAYATTRMASK = &HFC000        ' This flag is a mask for the display attributes.
        FILESYSANCESTOR = &H10000000     ' may contain children with FILESYSTEM
        FOLDER = &H20000000              ' support BindToObject(IID_IShellFolder)
        FILESYSTEM = &H40000000          ' is a win32 file system object (file/folder/root)
        HASSUBFOLDER = &H80000000        ' may contain children with FOLDER
        CONTENTSMASK = &H80000000        ' This flag is a mask for the contents attributes.
        VALIDATE = &H1000000             ' invalidate cached information
        REMOVABLE = &H2000000            ' is this removeable media?
        COMPRESSED = &H4000000           ' Object is compressed (use alt color)
        BROWSABLE = &H8000000            ' supports IShellFolder but only implements CreateViewObject() (non-folder view)
        NONENUMERATED = &H100000         ' is a non-enumerated object
        NEWCONTENT = &H200000            ' should show bold in explorer tree
        CANMONIKER = &H400000            ' defunct
        HASSTORAGE = &H400000            ' defunct
        STREAM = &H400000                ' supports BindToObject(IID_IStream)
        STORAGEANCESTOR = &H800000       ' may contain children with STORAGE or STREAM
        STORAGECAPMASK = &H70C50008      ' for determining storage capabilities ie for open/save semantics
    End Enum
#End Region

#Region "   SHGFI"
    <Flags()> _
    Public Enum SHGFI
        ICON = &H100                ' get icon 
        DISPLAYNAME = &H200         ' get display name 
        TYPENAME = &H400            ' get type name 
        ATTRIBUTES = &H800          ' get attributes 
        ICONLOCATION = &H1000       ' get icon location 
        EXETYPE = &H2000            ' return exe type 
        SYSICONINDEX = &H4000       ' get system icon index 
        LINKOVERLAY = &H8000        ' put a link overlay on icon 
        SELECTED = &H10000          ' show icon in selected state 
        ATTR_SPECIFIED = &H20000    ' get only specified attributes 
        LARGEICON = &H0             ' get large icon 
        SMALLICON = &H1             ' get small icon 
        OPENICON = &H2              ' get open icon 
        SHELLICONSIZE = &H4         ' get shell size icon 
        PIDL = &H8                  ' pszPath is a pidl 
        USEFILEATTRIBUTES = &H10    ' use passed dwFileAttribute 
        ADDOVERLAYS = &H20          ' apply the appropriate overlays
        OVERLAYINDEX = &H40         ' Get the index of the overlay
    End Enum
#End Region

#Region "   CSIDL"
    Public Enum CSIDL As Integer
        DESKTOP = &H0
        INTERNET = &H1
        PROGRAMS = &H2
        CONTROLS = &H3
        PRINTERS = &H4
        PERSONAL = &H5
        FAVORITES = &H6
        STARTUP = &H7
        RECENT = &H8
        SENDTO = &H9
        BITBUCKET = &HA
        STARTMENU = &HB
        MYDOCUMENTS = &HC
        MYMUSIC = &HD
        MYVIDEO = &HE
        DESKTOPDIRECTORY = &H10
        DRIVES = &H11
        NETWORK = &H12
        NETHOOD = &H13
        FONTS = &H14
        TEMPLATES = &H15
        COMMON_STARTMENU = &H16
        COMMON_PROGRAMS = &H17
        COMMON_STARTUP = &H18
        COMMON_DESKTOPDIRECTORY = &H19
        APPDATA = &H1A
        PRINTHOOD = &H1B
        LOCAL_APPDATA = &H1C
        ALTSTARTUP = &H1D
        COMMON_ALTSTARTUP = &H1E
        COMMON_FAVORITES = &H1F
        INTERNET_CACHE = &H20
        COOKIES = &H21
        HISTORY = &H22
        COMMON_APPDATA = &H23
        WINDOWS = &H24
        SYSTEM = &H25
        PROGRAM_FILES = &H26
        MYPICTURES = &H27
        PROFILE = &H28
        SYSTEMX86 = &H29
        PROGRAM_FILESX86 = &H2A
        PROGRAM_FILES_COMMON = &H2B
        PROGRAM_FILES_COMMONX86 = &H2C
        COMMON_TEMPLATES = &H2D
        COMMON_DOCUMENTS = &H2E
        COMMON_ADMINTOOLS = &H2F
        ADMINTOOLS = &H30
        CONNECTIONS = &H31
        COMMON_MUSIC = &H35
        COMMON_PICTURES = &H36
        COMMON_VIDEO = &H37
        RESOURCES = &H38
        RESOURCES_LOCALIZED = &H39
        COMMON_OEM_LINKS = &H3A
        CDBURN_AREA = &H3B
        COMPUTERSNEARME = &H3D
        FLAG_PER_USER_INIT = &H800
        FLAG_NO_ALIAS = &H1000
        FLAG_DONT_VERIFY = &H4000
        FLAG_CREATE = &H8000
        FLAG_MASK = &HFF00
    End Enum
#End Region

#Region "   E_STRRET"

    <Flags()> _
    Private Enum E_STRRET : int
        WSTR = &H0          ' Use STRRET.pOleStr
        OFFSET = &H1        ' Use STRRET.uOffset to Ansi
        C_STR = &H2         ' Use STRRET.cStr
    End Enum
#End Region

#Region "   SHCONTF"
    <Flags()> _
    Public Enum SHCONTF
        EMPTY = 0                      ' used to zero a SHCONTF variable
        FOLDERS = &H20                 ' only want folders enumerated (FOLDER)
        NONFOLDERS = &H40              ' include non folders
        INCLUDEHIDDEN = &H80           ' show items normally hidden
        INIT_ON_FIRST_NEXT = &H100     ' allow EnumObject() to return before validating enum
        NETPRINTERSRCH = &H200         ' hint that client is looking for printers
        SHAREABLE = &H400              ' hint that client is looking sharable resources (remote shares)
        STORAGE = &H800                ' include all items with accessible storage and their ancestors
    End Enum
#End Region

#Region "   SHGDN"
    <Flags()> _
    Public Enum SHGDN
        NORMAL = 0
        INFOLDER = 1
        FORADDRESSBAR = 16384
        FORPARSING = 32768
    End Enum
#End Region

#Region "   ILD --- Flags controlling how the Image List item is drawn"
    '/// <summary>
    '/// Flags controlling how the Image List item is 
    '/// drawn
    '/// </summary>
    '[Flags]	
    '   Public Enum ImageListDrawItemConstants : int
    '{
    '	/// <summary>
    '	/// Draw item normally.
    '	/// </summary>
    '	ILD_NORMAL = 0x0,
    '	/// <summary>
    '	/// Draw item transparently.
    '	/// </summary>
    '	ILD_TRANSPARENT = 0x1,
    '	/// <summary>
    '	/// Draw item blended with 25% of the specified foreground colour
    '	/// or the Highlight colour if no foreground colour specified.
    '	/// </summary>
    '	ILD_BLEND25 = 0x2,
    '	/// <summary>
    '	/// Draw item blended with 50% of the specified foreground colour
    '	/// or the Highlight colour if no foreground colour specified.
    '	/// </summary>
    '	ILD_SELECTED = 0x4,
    '	/// <summary>
    '	/// Draw the icon's mask
    '	/// </summary>
    '	ILD_MASK = 0x10,
    '	/// <summary>
    '	/// Draw the icon image without using the mask
    '	/// </summary>
    '	ILD_IMAGE = 0x20,
    '	/// <summary>
    '	/// Draw the icon using the ROP specified.
    '	/// </summary>
    '	ILD_ROP = 0x40,
    '	/// <summary>
    '	/// Preserves the alpha channel in dest. XP only.
    '	/// </summary>
    '	ILD_PRESERVEALPHA = 0x1000,
    '	/// <summary>
    '	/// Scale the image to cx, cy instead of clipping it.  XP only.
    '	/// </summary>
    '	ILD_SCALE = 0x2000,
    '	/// <summary>
    '	/// Scale the image to the current DPI of the display. XP only.
    '	/// </summary>
    '	ILD_DPISCALE = 0x4000
    '/// <summary>
    '/// Flags controlling how the Image List item is 
    '/// drawn
    '/// </summary>
    <Flags()> _
    Public Enum ILD
        NORMAL = &H0
        TRANSPARENT = &H1
        BLEND25 = &H2
        SELECTED = &H4
        MASK = &H10
        IMAGE = &H20
        ROP = &H40
        PRESERVEALPHA = &H1000
        SCALE = &H2000
        DPISCALE = &H4000
    End Enum
#End Region

#Region "   ILS --- XP ImageList Draw State options"
    '   /// <summary>
    '/// Enumeration containing XP ImageList Draw State options
    '/// </summary>

    '/// <summary>
    '/// The image state is not modified. 
    '/// </summary>
    'ILS_NORMAL = (0x00000000),
    '/// <summary>
    '/// Adds a glow effect to the icon, which causes the icon to appear to glow 
    '/// with a given color around the edges. (Note: does not appear to be
    '/// implemented)
    '/// </summary>
    'ILS_GLOW = (0x00000001), //The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
    '/// <summary>
    '/// Adds a drop shadow effect to the icon. (Note: does not appear to be
    '/// implemented)
    '/// </summary>
    'ILS_SHADOW = (0x00000002), //The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
    '/// <summary>
    '/// Saturates the icon by increasing each color component 
    '/// of the RGB triplet for each pixel in the icon. (Note: only ever appears
    '/// to result in a completely unsaturated icon)
    '/// </summary>
    'ILS_SATURATE = (0x00000004), // The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
    '/// <summary>
    '/// Alpha blends the icon. Alpha blending controls the transparency 
    '/// level of an icon, according to the value of its alpha channel. 
    '/// (Note: does not appear to be implemented).
    '/// </summary>
    'ILS_ALPHA = (0x00000008) //The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255, with 0 being completely transparent, and 255 being completely opaque. 
    Public Enum ILS
        NORMAL = (&H0)      'The image state is not modified.
        GLOW = (&H1)        'The color for the glow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
        SHADOW = (&H2)      'The color for the drop shadow effect is passed to the IImageList::Draw method in the crEffect member of IMAGELISTDRAWPARAMS. 
        SATURATE = (&H4)    'The amount to increase is indicated by the frame member in the IMAGELISTDRAWPARAMS method. 
        ALPHA = (&H8)       'The value of the alpha channel is indicated by the frame member in the IMAGELISTDRAWPARAMS method. The alpha channel can be from 0 to 255 with 0 being completely transparent and 255 being completely opaque. 
    End Enum
#End Region

#Region "   SLR --- IShellLink.Resolve Flags"
    <Flags()> _
    Public Enum SLR
        NO_UI = &H1
        ANY_MATCH = &H2
        UPDATE = &H4
        NOUPDATE = &H8
        NOSEARCH = &H10
        NOTRACK = &H20
        NOLINKINFO = &H40
        INVOKE_MSI = &H80
        NO_UI_WITH_MSG_PUMP = &H101
    End Enum
#End Region

#Region "   SLGP --- IShellLink.GetPath Flags"
    <Flags()> _
    Public Enum SLGP
        SHORTPATH = &H1
        UNCPRIORITY = &H2
        RAWPATH = &H4
    End Enum
#End Region

#Region "   SHGNLI -- SHGetNewLinkInfo flags"
    <Flags()> _
Public Enum SHGNLI
        PIDL = 1        'pszLinkTo is a pidl
        PREFIXNAME = 2  'Make name "Shortcut to xxx"
        NOUNIQUE = 4    'don't do the unique name generation
        NOLNK = 8       'don't add ".lnk" extension (Win2k or higher,IE5 or higher)
    End Enum

#End Region
#End Region

#Region "   Shell GUIDs"
    Public Shared IID_IMalloc As _
     New Guid("{00000002-0000-0000-C000-000000000046}")
    Public Shared IID_IShellFolder As _
     New Guid("{000214E6-0000-0000-C000-000000000046}")
    Public Shared IID_IFolderFilterSite As _
     New Guid("{C0A651F5-B48B-11d2-B5ED-006097C686F6}")
    Public Shared IID_IFolderFilter As _
     New Guid("{9CC22886-DC8E-11d2-B1D0-00C04F8EEB3E}")
    Public Shared DesktopGUID As _
     New Guid("{00021400-0000-0000-C000-000000000046}")
    Public Shared CLSID_ShellLink As _
     New Guid("{00021401-0000-0000-C000-000000000046}")
    Public Shared CLSID_InternetShortcut As _
     New Guid("{FBF23B40-E3F0-101B-8488-00AA003E56F8}")
    Public Shared IID_IDropTarget As _
     New Guid("{00000122-0000-0000-C000-000000000046}")
    Public Shared IID_IDataObject As _
     New Guid("{0000010e-0000-0000-C000-000000000046}")

#End Region

#Region "   Shell Structures"

#Region "       SHFILEINFO"
    '///<Summary>
    ' SHFILEINFO structure for VB.Net
    '///</Summary>
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
    Public Structure SHFILEINFO
        Public hIcon As IntPtr
        Public iIcon As Integer
        Public dwAttributes As Integer
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=MAX_PATH)> _
        Public szDisplayName As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=80)> _
        Public szTypeName As String
    End Structure
    Private Shared shfitmp As SHFILEINFO   'just used for the following
    Public Shared cbFileInfo As Integer = Marshal.SizeOf(shfitmp.GetType())
#End Region

#Region "       STRRET Structures"
    'both of these formats work in main thread, neither in worker thread
    '<StructLayout(LayoutKind.Sequential)> _
    'Public Structure STRRET
    '    Public uType As Integer
    '    Public pOle As IntPtr
    'End Structure
    <StructLayout(LayoutKind.Explicit)> _
    Public Structure STRRET
        <FieldOffset(0)> _
        Public uType As Integer       ' One of the STRRET_* values
        <FieldOffset(4)> _
          Public pOleStr As Integer ' must be freed by caller of GetDisplayNameOf
        <FieldOffset(4)> _
          Public uOffset As Integer ' Offset into SHITEMID
        <FieldOffset(4)> _
         Public pStr As Integer    ' NOT USED
    End Structure
#End Region

#Region "       W32_FIND_DATA"
    <StructLayoutAttribute(LayoutKind.Sequential, _
     CharSet:=CharSet.Auto)> _
     Public Structure WIN32_FIND_DATA
        Public dwFileAttributes As Integer
        Public ftCreationTime As ComTypes.FILETIME
        Public ftLastAccessTime As ComTypes.FILETIME
        Public ftLastWriteTime As ComTypes.FILETIME
        Public nFileSizeHigh As Integer
        Public nFileSizeLow As Integer
        Public dwReserved0 As Integer
        Public dwReserved1 As Integer
        <MarshalAs(UnmanagedType.ByValTStr, _
                   SizeConst:=MAX_PATH)> _
        Public cFileName As String
        <MarshalAs(UnmanagedType.ByValTStr, _
                   SizeConst:=14)> _
        Public cAlternateFileName As String
    End Structure

#End Region


#End Region

#Region "   Dll Declarations"
#Region "   Shell Dll Declarations"

#Region "       SHGetMalloc"
    '<Summary>
    '  Get an Imalloc Interface
    ' Not required for .Net apps, use Marshal class
    '</Summary>
    Declare Auto Function SHGetMalloc Lib "shell32" ( _
            ByRef pMalloc As IMalloc) As Integer
#End Region

#Region "       SHGetDesktopFolder"
    '<Summary>
    ' Retrieves the IShellFolder interface for the desktop folder, 
    '    which is the root of the Shell's namespace. 
    '<param>
    '  ppshf -- Recieves the IShellFolder interface for the desktop folder
    '</param>
    Declare Auto Function SHGetDesktopFolder Lib "shell32.dll" ( _
                ByRef ppshf As IShellFolder) As Integer
#End Region

#Region "       SHGetSpecialFolderLocation"

    Declare Function SHGetSpecialFolderLocation Lib "Shell32" ( _
        ByVal hWndOwner As Integer, _
        ByVal csidl As Integer, _
        ByRef ppidl As IntPtr) As Integer
#End Region

#Region "       SHGetFileInfo"
    'SHGetFileInfo
    'Retrieves information about an object in the file system,
    ' such as a file, a folder, a directory, or a drive root.

    ' <Summary>
    '  SHGetFileInfo  - for a given Path as a string
    ' </Summary>
    Declare Auto Function SHGetFileInfo Lib "shell32" ( _
         ByVal pszPath As String, _
         ByVal dwFileAttributes As Integer, _
         ByRef sfi As SHFILEINFO, _
         ByVal cbsfi As Integer, _
         ByVal uFlags As Integer) As IntPtr
    ' <Summary>
    '  SHGetFileInfo  - for a given ItemIDList as IntPtr
    ' </Summary>
    Declare Auto Function SHGetFileInfo Lib "shell32" ( _
             ByVal ppidl As IntPtr, _
             ByVal dwFileAttributes As Integer, _
             ByRef sfi As SHFILEINFO, _
             ByVal cbsfi As Integer, _
             ByVal uFlags As Integer) As IntPtr
#End Region

#Region "       SHGetNewLinkInfo"
    '''<Summary>Despite its name, the API returns a filename
    ''' for a link to be copied/created in a Target directory,
    ''' with a specific LinkTarget. It will create a unique name
    ''' unless instructed otherwise (SHGLNI_NOUNIQUE).  And add
    ''' the ".lnk" extension, unless instructed otherwise(SHGLNI.NOLNK)
    '''</Summary>
    Declare Ansi Function SHGetNewLinkInfo Lib "shell32" Alias "SHGetNewLinkInfoA" ( _
            ByVal pszLinkTo As String, _
            ByVal pszDir As String, _
            <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszName As StringBuilder, _
            ByRef pfMustCopy As Boolean, _
            ByVal uFlags As SHGNLI) As Integer
    '''<Summary> Same function using a PIDL as the pszLinkTo.
    '''  SHGNLI.PIDL must be set.
    '''</Summary>
    Declare Ansi Function SHGetNewLinkInfo Lib "shell32" Alias "SHGetNewLinkInfoA" ( _
        ByVal pszLinkTo As IntPtr, _
        ByVal pszDir As String, _
        <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszName As StringBuilder, _
        ByRef pfMustCopy As Boolean, _
        ByVal uFlags As SHGNLI) As Integer

#End Region

#Region "       IL functions"
    Declare Auto Function ILIsParent Lib "shell32" Alias "#23" ( _
                                ByVal pidlParent As IntPtr, _
                                ByVal pidlBelow As IntPtr, _
                                ByVal fImmediate As Boolean) _
                                            As Boolean

    Declare Auto Function ILIsEqual Lib "shell32" Alias "#21" ( _
                                ByVal pidl1 As IntPtr, _
                                ByVal pidl2 As IntPtr) As Boolean

#End Region

#End Region

#Region "   Non-Shell Dll Declarations"

#Region "       STRRETtoSomeString"
    ' Accepts a STRRET structure returned by IShellFolder::GetDisplayNameOf that contains or points to a 
    ' string, and then returns that string as a BSTR.
    ' <param>
    '       Pointer to a STRRET structure.
    '       Pointer to an ITEMIDLIST uniquely identifying a file object or subfolder relative
    '       Pointer to a variable of type BSTR that contains the converted string.
    '</param>
    Declare Auto Function StrRetToBSTR Lib "shlwapi.dll" ( _
                ByRef pstr As STRRET, _
                ByVal pidl As IntPtr, _
                <MarshalAs(UnmanagedType.BStr)> _
                ByRef pbstr As String) As Integer

    '<Summary>
    ' Takes a STRRET structure returned by IShellFolder::GetDisplayNameOf, 
    ' converts it to a string, and 
    ' places the result in a buffer. 
    ' <param>
    '       Pointer to a STRRET structure.
    '       Pointer to an ITEMIDLIST uniquely identifying a file object or subfolder relative
    '       Pointer to a Buffer to hold the display name. It will be returned as a null-terminated
    '                   string. If cchBuf is too small, 
    '                   the name will be truncated to fit. 
    '       Size of pszBuf, in characters. 
    '</param>
    '</Summary>
    Declare Auto Function StrRetToBuf Lib "shlwapi.dll" ( _
                        ByVal pstr As IntPtr, _
                        ByVal pidl As IntPtr, _
                        ByVal pszBuf As StringBuilder, _
                        <MarshalAs(UnmanagedType.U4)> _
                        ByVal cchBuf As Integer) As Integer
#End Region

#Region "       SendMessage"
    '<Summary>
    '   Sends a message to some Window
    '</Summary>
    Declare Auto Function SendMessage Lib "user32" ( _
            ByVal hWnd As IntPtr, _
            ByVal wMsg As Integer, _
            ByVal wParam As Integer, _
            ByVal lParam As IntPtr) As Integer
#End Region

#Region "       ImageList_GetIconSize"
    '<Summary>
    '   Gets an IconSize from a ImagelistHandle
    '</Summary>
    Declare Function ImageList_GetIconSize Lib "comctl32" ( _
               ByVal himl As IntPtr, _
               ByRef cx As Integer, _
               ByRef cy As Integer) As Integer
#End Region

#Region "       ImageList_ReplaceIcon"
    Declare Auto Function ImageList_ReplaceIcon Lib "comctl32" _
                                    (ByVal hImageList As IntPtr, _
                                    ByVal IconIndex As Integer, _
                                    ByVal hIcon As IntPtr) _
                                    As Integer

#End Region

#Region "       ImageList_GetIcon"
    Declare Function ImageList_GetIcon Lib "comctl32" ( _
                ByVal himl As IntPtr, _
                ByVal i As Integer, _
                ByVal flags As Integer) As IntPtr
#End Region

#Region "       ImageList_Draw"
    Declare Function ImageList_Draw Lib "comctl32" ( _
        ByVal hIml As IntPtr, _
        ByVal indx As Integer, _
        ByVal hdcDst As IntPtr, _
        ByVal x As Integer, _
        ByVal y As Integer, _
        ByVal fStyle As Integer) As Integer
#End Region

#Region "       DestroyIcon"
    Declare Function DestroyIcon Lib "user32.dll" ( _
                ByVal hIcon As IntPtr) As Integer
#End Region

#Region "       ImageList Structures"
    <StructLayout(LayoutKind.Sequential)> _
    Private Structure RECT
        Dim left As Integer
        Dim top As Integer
        Dim right As Integer
        Dim bottom As Integer
    End Structure
    <StructLayout(LayoutKind.Sequential)> _
     Public Structure POINT
        Dim x As Integer
        Dim y As Integer
    End Structure
    '[StructLayout(LayoutKind.Sequential)]
    '	private struct IMAGELISTDRAWPARAMS				
    '{
    '	public int cbSize;
    '	public IntPtr himl;
    '	public int i;
    '	public IntPtr hdcDst;
    '	public int x;
    '	public int y;
    '	public int cx;
    '	public int cy;
    '	public int xBitmap;        // x offest from the upperleft of bitmap
    '	public int yBitmap;        // y offset from the upperleft of bitmap
    '	public int rgbBk;
    '	public int rgbFg;
    '	public int fStyle;
    '	public int dwRop;
    '	public int fState;
    '	public int Frame;
    '	public int crEffect;
    '}

    '[StructLayout(LayoutKind.Sequential)]
    '	private struct IMAGEINFO
    '{
    '	public IntPtr hbmImage;
    '	public IntPtr hbmMask;
    '	public int Unused1;
    '	public int Unused2;
    '	public RECT rcImage;
    '}

#End Region


#End Region

#End Region

#Region "   Shell Interfaces"

#Region "       Com Interop for IUnknown"
    'Not needed in .Net - use Marshal Class
    <ComImport(), Guid("00000000-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
      Interface IUnknown

        <PreserveSig()> _
        Function QueryInterface(ByRef riid As Guid, ByRef pVoid As IntPtr) As Integer
        <PreserveSig()> _
         Function AddRef() As Integer
        <PreserveSig()> _
       Function Release() As Integer
    End Interface
#End Region

#Region "       Com Interop for IMalloc"
    'Not needed in .Net - use Marshal Class
    <ComImport(), Guid("00000002-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMalloc
        ' Allocates a block of memory.
        ' Return value: a pointer to the allocated memory block.
        <PreserveSig()> _
            Function Alloc( _
                    ByVal cb As Integer) As IntPtr ' Size, in bytes, of the memory block to be allocated.

        ' Changes the size of a previously allocated memory block.
        ' Return value:  Reallocated memory block 
        <PreserveSig()> _
            Function Realloc(ByVal pv As IntPtr, _
                             ByVal cb As Integer) As IntPtr

        ' Frees a previously allocated block of memory.
        <PreserveSig()> _
            Sub Free(ByVal pv As IntPtr) ' Pointer to the memory block to be freed.

        ' This method returns the size (in bytes) of a memory block previously allocated with 
        ' IMalloc::Alloc or IMalloc::Realloc.
        ' Return value: The size of the allocated memory block in bytes 
        <PreserveSig()> _
        Function GetSize(ByVal pv As IntPtr) As Integer ' Pointer to the memory block for which the size is requested.

        ' This method determines whether this allocator was used to allocate the specified block of memory.
        ' Return value: 1 - allocated 0 - not allocated by this IMalloc instance. 
        <PreserveSig()> _
            Function DidAlloc( _
                ByVal pv As IntPtr) As Int16 ' Pointer to the memory block

        ' This method minimizes the heap as much as possible by releasing unused memory to the operating system, 
        ' coalescing adjacent free blocks and committing free pages.
        <PreserveSig()> _
            Sub HeapMinimize()
    End Interface

#End Region

#Region "       COM Interop for IShellFolder"

    <ComImportAttribute(), _
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
    Guid("000214E6-0000-0000-C000-000000000046")> _
Public Interface IShellFolder
        <PreserveSig()> _
        Function ParseDisplayName( _
            ByVal hwndOwner As Integer, _
            ByVal pbcReserved As IntPtr, _
            <MarshalAs(UnmanagedType.LPWStr)> _
            ByVal lpszDisplayName As String, _
            ByRef pchEaten As Integer, _
            ByRef ppidl As IntPtr, _
            ByRef pdwAttributes As Integer) As Integer

        <PreserveSig()> _
        Function EnumObjects( _
            ByVal hwndOwner As Integer, _
            <MarshalAs(UnmanagedType.U4)> ByVal _
            grfFlags As SHCONTF, _
            ByRef ppenumIDList As IEnumIDList) As Integer

        <PreserveSig()> _
        Function BindToObject( _
            ByVal pidl As IntPtr, _
            ByVal pbcReserved As IntPtr, _
            ByRef riid As Guid, _
            ByRef ppvOut As IShellFolder) As Integer

        <PreserveSig()> _
        Function BindToStorage( _
            ByVal pidl As IntPtr, _
            ByVal pbcReserved As IntPtr, _
            ByRef riid As Guid, _
            ByVal ppvObj As IntPtr) As Integer

        <PreserveSig()> _
        Function CompareIDs( _
            ByVal lParam As IntPtr, _
            ByVal pidl1 As IntPtr, _
            ByVal pidl2 As IntPtr) As Integer

        <PreserveSig()> _
        Function CreateViewObject( _
            ByVal hwndOwner As IntPtr, _
            ByRef riid As Guid, _
            ByRef ppvOut As IUnknown) As Integer

        <PreserveSig()> _
        Function GetAttributesOf( _
            ByVal cidl As Integer, _
            <MarshalAs(UnmanagedType.LPArray, sizeparamindex:=0)> _
            ByVal apidl() As IntPtr, _
            ByRef rgfInOut As SFGAO) As Integer

        <PreserveSig()> _
        Function GetUIObjectOf( _
            ByVal hwndOwner As IntPtr, _
            ByVal cidl As Integer, _
            <MarshalAs(UnmanagedType.LPArray, sizeparamindex:=0)> _
            ByVal apidl() As IntPtr, _
            ByRef riid As Guid, _
            ByRef prgfInOut As Integer, _
            ByRef ppvOut As IUnknown) As Integer
        'ByRef ppvOut As IDropTarget) As Integer

        <PreserveSig()> _
        Function GetDisplayNameOf( _
            ByVal pidl As IntPtr, _
            <MarshalAs(UnmanagedType.U4)> _
            ByVal uFlags As SHGDN, _
            ByVal lpName As IntPtr) As Integer

        <PreserveSig()> _
        Function SetNameOf( _
            ByVal hwndOwner As Integer, _
            ByVal pidl As IntPtr, _
            <MarshalAs(UnmanagedType.LPWStr)> ByVal _
            lpszName As String, _
            <MarshalAs(UnmanagedType.U4)> ByVal _
            uFlags As SHCONTF, _
            ByRef ppidlOut As IntPtr) As Integer
    End Interface
#End Region

#Region "       Com Interop for IEnumIDList"
    <ComImportAttribute(), _
     InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
     Guid("000214F2-0000-0000-C000-000000000046")> _
        Public Interface IEnumIDList
        <PreserveSig()> _
        Function GetNext( _
            ByVal celt As Integer, _
            ByRef rgelt As IntPtr, _
            ByRef pceltFetched As Integer) As Integer

        <PreserveSig()> _
        Function Skip( _
            ByVal celt As Integer) As Integer

        <PreserveSig()> _
        Function Reset() As Integer

        <PreserveSig()> _
        Function Clone( _
            ByRef ppenum As IEnumIDList) As Integer
    End Interface

#End Region

#Region "       Com Interop for IPersistFile"
    <ComImportAttribute(), _
      InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
      Guid("0000010B-0000-0000-C000-000000000046")> _
  Public Interface IPersistFile

        'Inheirited from Ipersist
        Sub GetClassID( _
          <Out()> ByRef pClassID As Guid)

        'IPersistFile Interfaces
        <PreserveSig()> _
        Function IsDirty() As Integer

        Function Load( _
          <MarshalAs(UnmanagedType.LPWStr)> ByVal pszFileName As String, _
          ByVal dwMode As Integer) As Integer

        Function Save( _
          <MarshalAs(UnmanagedType.LPWStr)> ByVal pszFileName As String, _
          <MarshalAs(UnmanagedType.Bool)> ByVal fRemember As Boolean) As Integer

        Function SaveCompleted( _
          <MarshalAs(UnmanagedType.LPWStr)> ByVal pszFileName As String) As Integer

        Function GetCurFile( _
          <Out(), MarshalAs(UnmanagedType.LPWStr)> ByRef ppszFileName As String) As Integer
    End Interface
#End Region

#Region "       Com Interop for IShellLink"
    'We define the Ansi version since all Win OSs (95 thru XP) support it
    <ComImportAttribute(), _
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown), _
    Guid("000214EE-0000-0000-C000-000000000046")> _
Public Interface IShellLink

        Function GetPath( _
          <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszFile As StringBuilder, _
          ByVal cchMaxPath As Integer, _
          <Out()> ByRef pfd As WIN32_FIND_DATA, _
          ByVal fFlags As SLGP) As Integer

        Function GetIDList( _
          ByRef ppidl As IntPtr) As Integer

        Function SetIDList( _
          ByVal pidl As IntPtr) As Integer

        Function GetDescription( _
          <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszName As StringBuilder, _
          ByVal cchMaxName As Integer) As Integer

        Function SetDescription( _
          <MarshalAs(UnmanagedType.LPStr)> ByVal pszName As String) As Integer

        Function GetWorkingDirectory( _
          <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszDir As StringBuilder, _
          ByVal cchMaxPath As Integer) As Integer

        Function SetWorkingDirectory( _
          <MarshalAs(UnmanagedType.LPStr)> ByVal pszDir As String) As Integer

        Function GetArguments( _
          <Out(), MarshalAs(UnmanagedType.LPStr)> ByVal pszArgs As StringBuilder, _
          ByVal cchMaxPath As Integer) As Integer

        Function SetArguments( _
          <MarshalAs(UnmanagedType.LPStr)> ByVal pszArgs As String) As Integer

        Function GetHotkey( _
          ByRef pwHotkey As Short) As Integer

        Function SetHotkey( _
          ByVal wHotkey As Short) As Integer

        Function GetShowCmd( _
          ByRef piShowCmd As Integer) As Integer

        Function SetShowCmd( _
          ByVal iShowCmd As Integer) As Integer

        Function GetIconLocation( _
          <Out(), MarshalAs(UnmanagedType.LPStr)> _
          ByVal pszIconPath As StringBuilder, _
          ByVal cchIconPath As Integer, _
          ByRef piIcon As Integer) As Integer

        Function SetIconLocation( _
          <MarshalAs(UnmanagedType.LPStr)> _
          ByVal pszIconPath As String, _
          ByVal iIcon As Integer) As Integer

        Function SetRelativePath( _
          <MarshalAs(UnmanagedType.LPStr)> _
          ByVal pszPathRel As String, _
          ByVal dwReserved As Integer) As Integer

        Function Resolve( _
          ByVal hwnd As IntPtr, _
          ByVal fFlags As SLR) As Integer

        Function SetPath( _
          <MarshalAs(UnmanagedType.LPStr)> _
          ByVal pszFile As String) As Integer
    End Interface
#End Region

#Region "       Drag/Drop Interfaces and other declarations"

#Region "       Drag/Drop related Dlls"

#Region "       RegisterClipboardFormat"

    'UINT RegisterClipboardFormat(LPCTSTR lpszFormat)

    Declare Auto Function RegisterClipboardFormat Lib "User32" (ByVal lpszFormat As String) As Integer

#End Region

#Region "           ReleaseStgMedium"
    Declare Auto Sub ReleaseStgMedium Lib "ole32.dll" (ByRef pmedium As STGMEDIUM)
#End Region

#Region "           RegisterDragDrop, RevokeDragDrop"
    Declare Auto Function RegisterDragDrop Lib "ole32.dll" ( _
            ByVal hWnd As IntPtr, _
            ByVal IdropTgt As IDropTarget) As Integer

    Declare Auto Function RevokeDragDrop Lib "ole32.dll" ( _
            ByVal hWnd As IntPtr) As Integer
#End Region

#Region "           DragQueryFiles"
    'UINT DragQueryFile(HDROP hDrop,
    'UINT iFile,
    'LPTSTR lpszFile,
    'UINT cch
    ');
    Declare Auto Function DragQueryFile Lib "shell32.dll" ( _
                        ByVal hDrop As IntPtr, _
                        ByVal iFile As Integer, _
                   <MarshalAs(UnmanagedType.LPTStr)> _
                        ByVal lpszFile As StringBuilder, _
                        ByVal cch As Integer) As Integer

#End Region
#End Region

#Region "       Drag/Drop Enums and Stuctures"
#Region "           CLIPFORMAT Enum"
    Public Enum CF
        TEXT = 1
        BITMAP = 2
        METAFILEPICT = 3
        SYLK = 4
        DIF = 5
        TIFF = 6
        OEMTEXT = 7
        DIB = 8
        PALETTE = 9
        PENDATA = 10
        RIFF = 11
        WAVE = 12
        UNICODETEXT = 13
        ENHMETAFILE = 14
        HDROP = 15
        LOCALE = 16
        MAX = 17
        OWNERDISPLAY = &H80
        DSPTEXT = &H81
        DSPBITMAP = &H82
        DSPMETAFILEPICT = &H83
        DSPENHMETAFILE = &H8E
        PRIVATEFIRST = &H200
        PRIVATELAST = &H2FF
        GDIOBJFIRST = &H300
        GDIOBJLAST = &H3FF
    End Enum
#End Region

#Region "           DVASPECT Enum"
    <Flags()> _
    Public Enum DVASPECT
        CONTENT = 1
        THUMBNAIL = 2
        ICON = 4
        DOCPRINT = 8
    End Enum
#End Region

#Region "           TYMED Enum"
    <Flags()> _
    Public Enum TYMED
        HGLOBAL = 1
        FILE = 2
        ISTREAM = 4
        ISTORAGE = 8
        GDI = 16
        MFPICT = 32
        ENHMF = 64
        NULL = 0
    End Enum
#End Region

#Region "       ADVF Enum"
    <Flags()> _
    Public Enum ADVF
        NODATA = 1
        PRIMEFIRST = 2
        ONLYONCE = 4
        DATAONSTOP = 64
        CACHE_NOHANDLER = 8
        CACHE_FORCEBUILTIN = 16
        CACHE_ONSAVE = 32
    End Enum
#End Region

#Region "           FORMATETC Structure"
    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
Public Structure FORMATETC
        Public cfFormat As CF
        Public ptd As IntPtr
        Public dwAspect As DVASPECT
        Public lindex As Integer
        Public Tymd As ShellDll.TYMED
    End Structure
#End Region

#Region "           STGMEDIUM Structure"
    <StructLayout(LayoutKind.Sequential)> _
Public Structure STGMEDIUM
        Public tymed As Integer
        Public hGlobal As IntPtr
        Public pUnkForRelease As IntPtr
    End Structure
#End Region

#Region "           DROPFILES Structure"
    <StructLayout(LayoutKind.Sequential)> _
      Public Structure DROPFILES
        Public pFiles As Integer
        Public pt As POINT
        Public fNC As Boolean
        Public fWide As Boolean
    End Structure
#End Region
#End Region

#Region "       Drag/Drop Interfaces"

#Region "       COM Interface for IEumFormatETC"
    '    MIDL_INTERFACE("00000103-0000-0000-C000-000000000046")
    'IEnumFORMATETC : public IUnknown
    'public:
    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE Next( 
    '        /* [in] */ ULONG celt,
    '        /* [length_is][size_is][out] */ FORMATETC *rgelt,
    '        /* [out] */ ULONG *pceltFetched) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE Skip( 
    '        /* [in] */ ULONG celt) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE Reset( void) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE Clone( 
    '        /* [out] */ IEnumFORMATETC **ppenum) = 0;
    <ComImportAttribute(), _
      InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
      Guid("00000103-0000-0000-C000-000000000046")> _
         Public Interface IEnumFORMATETC

        <PreserveSig()> _
        Function GetNext( _
            ByVal celt As Integer, _
            ByRef rgelt As FORMATETC, _
            ByRef pceltFetched As Integer) As Integer

        <PreserveSig()> _
        Function Skip( _
            ByVal celt As Integer) As Integer

        <PreserveSig()> _
        Function Reset() As Integer

        <PreserveSig()> _
        Function Clone( _
            ByRef ppenum As IEnumFORMATETC) As Integer
    End Interface

#End Region

#Region "           Com Interop for IDataObject"
    '    MIDL_INTERFACE("0000010e-0000-0000-C000-000000000046")
    'IDataObject : public IUnknown
    '{
    'public:
    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE GetData( 
    '        /* [unique][in] */ FORMATETC *pformatetcIn,
    '        /* [out] */ STGMEDIUM *pmedium) = 0;

    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE GetDataHere( 
    '        /* [unique][in] */ FORMATETC *pformatetc,
    '        /* [out][in] */ STGMEDIUM *pmedium) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE QueryGetData( 
    '        /* [unique][in] */ FORMATETC *pformatetc) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE GetCanonicalFormatEtc( 
    '        /* [unique][in] */ FORMATETC *pformatectIn,
    '        /* [out] */ FORMATETC *pformatetcOut) = 0;

    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE SetData( 
    '        /* [unique][in] */ FORMATETC *pformatetc,
    '        /* [unique][in] */ STGMEDIUM *pmedium,
    '        /* [in] */ BOOL fRelease) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE EnumFormatEtc( 
    '        /* [in] */ DWORD dwDirection,
    '        /* [out] */ IEnumFORMATETC **ppenumFormatEtc) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE DAdvise( 
    '        /* [in] */ FORMATETC *pformatetc,
    '        /* [in] */ DWORD advf,
    '        /* [unique][in] */ IAdviseSink *pAdvSink,
    '        /* [out] */ DWORD *pdwConnection) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE DUnadvise( 
    '        /* [in] */ DWORD dwConnection) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE EnumDAdvise( 
    '        /* [out] */ IEnumSTATDATA **ppenumAdvise) = 0;

    '};

    <ComImportAttribute(), _
      InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
        Guid("0000010e-0000-0000-C000-000000000046")> _
    Public Interface IDataObject

        <PreserveSig()> _
        Function GetData(ByRef pformatetcIn As FORMATETC, _
                         ByRef pmedium As STGMEDIUM) As Integer
        <PreserveSig()> _
        Function GetDataHere(ByRef pformatetcIn As FORMATETC, _
                             ByRef pmedium As STGMEDIUM) As Integer
        <PreserveSig()> _
         Function QueryGetData(ByRef pformatetc As FORMATETC) As Integer

        <PreserveSig()> _
         Function GetCanonicalFormatEtc(ByVal pformatetc As FORMATETC, _
                                        ByRef pformatetcout As FORMATETC) As Integer
        <PreserveSig()> _
        Function SetData(ByRef pformatetcIn As FORMATETC, _
                         ByRef pmedium As STGMEDIUM, _
                         ByVal frelease As Boolean) As Integer
        <PreserveSig()> _
        Function EnumFormatEtc(ByVal dwDirection As Integer, _
                               ByRef ppenumFormatEtc As IEnumFORMATETC) As Integer
        <PreserveSig()> _
        Function DAdvise(ByRef pformatetc As FORMATETC, _
                         ByVal advf As ADVF, _
                         ByVal pAdvSink As IAdviseSink, _
                         ByRef pdwConnection As Integer) As Integer

        <PreserveSig()> _
        Function DUnadvise(ByVal dwConnection As Integer) As Integer

        <PreserveSig()> _
        Function EnumDAdvise(ByRef ppenumAdvise As Object) As Integer
    End Interface

#End Region

#Region "           Com Interop for IAdviseSink"
    '    MIDL_INTERFACE("0000010f-0000-0000-C000-000000000046")
    'IAdviseSink : public IUnknown
    'public:
    '    virtual /* [local] */ void STDMETHODCALLTYPE OnDataChange( 
    '        /* [unique][in] */ FORMATETC *pFormatetc,
    '        /* [unique][in] */ STGMEDIUM *pStgmed) = 0;

    '    virtual /* [local] */ void STDMETHODCALLTYPE OnViewChange( 
    '        /* [in] */ DWORD dwAspect,
    '        /* [in] */ LONG lindex) = 0;

    '    virtual /* [local] */ void STDMETHODCALLTYPE OnRename( 
    '        /* [in] */ IMoniker *pmk) = 0;

    '    virtual /* [local] */ void STDMETHODCALLTYPE OnSave( void) = 0;

    '    virtual /* [local] */ void STDMETHODCALLTYPE OnClose( void) = 0;
    <ComImportAttribute(), _
    InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown), _
      Guid("0000010f-0000-0000-C000-000000000046")> _
    Public Interface IAdviseSink
        Sub OnDataChange(ByRef pformatetcIn As FORMATETC, _
                         ByRef pmedium As STGMEDIUM)
        Sub OnViewChange(ByVal dwAspect As Integer, _
                         ByVal lindex As Integer)
        Sub OnRename(ByVal pmk As IntPtr)

        Sub OnSave()

        Sub OnClose()

    End Interface
#End Region

#Region "           Com Interop for IDropTarget"
    '    MIDL_INTERFACE("00000122-0000-0000-C000-000000000046")
    'IDropTarget : public IUnknown
    '{
    'public:
    '    virtual HRESULT STDMETHODCALLTYPE DragEnter( 
    '        /* [unique][in] */ IDataObject *pDataObj,
    '        /* [in] */ DWORD grfKeyState,
    '        /* [in] */ POINTL pt,
    '        /* [out][in] */ DWORD *pdwEffect) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE DragOver( 
    '        /* [in] */ DWORD grfKeyState,
    '        /* [in] */ POINTL pt,
    '        /* [out][in] */ DWORD *pdwEffect) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE DragLeave( void) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE Drop( 
    '        /* [unique][in] */ IDataObject *pDataObj,
    '        /* [in] */ DWORD grfKeyState,
    '        /* [in] */ POINTL pt,
    '        /* [out][in] */ DWORD *pdwEffect) = 0;

    <ComImportAttribute(), _
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown), _
   Guid("00000122-0000-0000-C000-000000000046")> _
    Public Interface IDropTarget
        <PreserveSig()> _
        Function DragEnter( _
                ByVal pDataObj As IntPtr, _
                ByVal grfKeyState As Integer, _
                ByVal pt As POINT, _
                ByRef pdwEffect As Integer) _
                As Integer

        <PreserveSig()> _
        Function DragOver( _
                ByVal grfKeyState As Integer, _
                ByVal pt As POINT, _
                ByRef pdwEffect As Integer) _
                As Integer

        <PreserveSig()> _
        Function DragLeave() As Integer

        <PreserveSig()> _
        Function DragDrop( _
                ByVal pDataObj As IntPtr, _
                ByVal grfKeyState As Integer, _
                ByVal pt As POINT, _
                ByRef pdwEffect As Integer) As Integer
    End Interface
#End Region

#End Region




#End Region
#End Region

#Region "   Public Shared Methods"

#Region "       GetSpecialFolderPath"
    Public Shared Function GetSpecialFolderPath(ByVal hWnd As IntPtr, ByVal csidl As Integer) As String
        Dim res As IntPtr
        Dim ppidl As IntPtr
        ppidl = GetSpecialFolderLocation(hWnd, csidl)
        Dim shfi As New SHFILEINFO()
        Dim uFlags As Integer = SHGFI.PIDL Or SHGFI.DISPLAYNAME Or SHGFI.TYPENAME
        'uFlags = uFlags Or SHGFI.SYSICONINDEX
        Dim dwAttr As Integer = 0
        res = SHGetFileInfo(ppidl, dwAttr, shfi, cbFileInfo, uFlags)
        Marshal.FreeCoTaskMem(ppidl)
        Return shfi.szDisplayName & "  (" & shfi.szTypeName & ")"
    End Function
#End Region

#Region "       GetSpecialFolderLocation"
    Public Shared Function GetSpecialFolderLocation(ByVal hWnd As IntPtr, ByVal csidl As Integer) As IntPtr
        Dim rVal As IntPtr
        Dim res As Integer
        res = SHGetSpecialFolderLocation(0, csidl, rVal)
        Return rVal
    End Function
#End Region

#Region "       IsXpOrAbove and Is2KOrAbove"
    Public Shared Function IsXpOrAbove() As Boolean
        Dim rVal As Boolean = False
        If Environment.OSVersion.Version.Major > 5 Then
            rVal = True
        ElseIf Environment.OSVersion.Version.Major = 5 AndAlso _
               Environment.OSVersion.Version.Minor >= 1 Then
            rVal = True
        End If
        'if none of the above tests succeed, then return false
        Return rVal
    End Function
    Public Shared Function Is2KOrAbove() As Boolean
        If Environment.OSVersion.Version.Major >= 5 Then
            Return True
        Else
            Return False
        End If
    End Function
#End Region

#End Region

End Class
