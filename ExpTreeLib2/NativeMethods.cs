using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Kinovea.ExpTreeLib2
{
    public static class NativeMethods
    {
        #region Shell Constants
        public const int MAX_PATH = 260;
        public const int FILE_ATTRIBUTE_NORMAL = 0x80;
        public const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        public const int NOERROR = 0;
        public const int S_OK = 0;
        public const int S_FALSE = 1;
        #endregion

        #region Shell Enumerations

        [Flags]
        public enum SFGAO
        {
            CANCOPY = 0x1,
            CANMOVE = 0x2,
            CANLINK = 0x4,
            STORAGE = 0x8,
            CANRENAME = 0x10,
            CANDELETE = 0x20,
            HASPROPSHEET = 0x40,
            DROPTARGET = 0x100,
            CAPABILITYMASK = 0x177,
            ENCRYPTED = 0x2000,
            ISSLOW = 0x4000,
            GHOSTED = 0x8000,
            LINK = 0x10000,
            SHARE = 0x20000,
            RDONLY = 0x40000,
            HIDDEN = 0x80000,
            DISPLAYATTRMASK = 0xFC000,
            FILESYSANCESTOR = unchecked((int)0x10000000),
            FOLDER = unchecked((int)0x20000000),
            FILESYSTEM = unchecked((int)0x40000000),
            HASSUBFOLDER = unchecked((int)0x80000000),
            CONTENTSMASK = unchecked((int)0x80000000),
            VALIDATE = 0x1000000,
            REMOVABLE = 0x2000000,
            COMPRESSED = 0x4000000,
            BROWSABLE = 0x8000000,
            NONENUMERATED = 0x100000,
            NEWCONTENT = 0x200000,
            CANMONIKER = 0x400000,
            HASSTORAGE = 0x400000,
            STREAM = 0x400000,
            STORAGEANCESTOR = 0x800000,
            STORAGECAPMASK = 0x70C50008
        }

        [Flags]
        public enum SHGFI
        {
            ICON = 0x100,
            DISPLAYNAME = 0x200,
            TYPENAME = 0x400,
            ATTRIBUTES = 0x800,
            ICONLOCATION = 0x1000,
            EXETYPE = 0x2000,
            SYSICONINDEX = 0x4000,
            LINKOVERLAY = 0x8000,
            SELECTED = 0x10000,
            ATTR_SPECIFIED = 0x20000,
            LARGEICON = 0x0,
            SMALLICON = 0x1,
            OPENICON = 0x2,
            SHELLICONSIZE = 0x4,
            PIDL = 0x8,
            USEFILEATTRIBUTES = 0x10,
            ADDOVERLAYS = 0x20,
            OVERLAYINDEX = 0x40
        }

        public enum CSIDL : int
        {
            DESKTOP = 0x0,
            INTERNET = 0x1,
            PROGRAMS = 0x2,
            CONTROLS = 0x3,
            PRINTERS = 0x4,
            PERSONAL = 0x5,
            FAVORITES = 0x6,
            STARTUP = 0x7,
            RECENT = 0x8,
            SENDTO = 0x9,
            BITBUCKET = 0xA,
            STARTMENU = 0xB,
            MYDOCUMENTS = 0xC,
            MYMUSIC = 0xD,
            MYVIDEO = 0xE,
            DESKTOPDIRECTORY = 0x10,
            DRIVES = 0x11,
            NETWORK = 0x12,
            NETHOOD = 0x13,
            FONTS = 0x14,
            TEMPLATES = 0x15,
            COMMON_STARTMENU = 0x16,
            COMMON_PROGRAMS = 0x17,
            COMMON_STARTUP = 0x18,
            COMMON_DESKTOPDIRECTORY = 0x19,
            APPDATA = 0x1A,
            PRINTHOOD = 0x1B,
            LOCAL_APPDATA = 0x1C,
            ALTSTARTUP = 0x1D,
            COMMON_ALTSTARTUP = 0x1E,
            COMMON_FAVORITES = 0x1F,
            INTERNET_CACHE = 0x20,
            COOKIES = 0x21,
            HISTORY = 0x22,
            COMMON_APPDATA = 0x23,
            WINDOWS = 0x24,
            SYSTEM = 0x25,
            PROGRAM_FILES = 0x26,
            MYPICTURES = 0x27,
            PROFILE = 0x28,
            SYSTEMX86 = 0x29,
            PROGRAM_FILESX86 = 0x2A,
            PROGRAM_FILES_COMMON = 0x2B,
            PROGRAM_FILES_COMMONX86 = 0x2C,
            COMMON_TEMPLATES = 0x2D,
            COMMON_DOCUMENTS = 0x2E,
            COMMON_ADMINTOOLS = 0x2F,
            ADMINTOOLS = 0x30,
            CONNECTIONS = 0x31,
            COMMON_MUSIC = 0x35,
            COMMON_PICTURES = 0x36,
            COMMON_VIDEO = 0x37,
            RESOURCES = 0x38,
            RESOURCES_LOCALIZED = 0x39,
            COMMON_OEM_LINKS = 0x3A,
            CDBURN_AREA = 0x3B,
            COMPUTERSNEARME = 0x3D,
            FLAG_PER_USER_INIT = 0x800,
            FLAG_NO_ALIAS = 0x1000,
            FLAG_DONT_VERIFY = 0x4000,
            FLAG_CREATE = 0x8000,
            FLAG_MASK = 0xFF00
        }

        [Flags]
        private enum E_STRRET : int
        {
            WSTR = 0x0,
            OFFSET = 0x1,
            C_STR = 0x2
        }

        [Flags]
        public enum SHCONTF
        {
            EMPTY = 0,
            FOLDERS = 0x20,
            NONFOLDERS = 0x40,
            INCLUDEHIDDEN = 0x80,
            INIT_ON_FIRST_NEXT = 0x100,
            NETPRINTERSRCH = 0x200,
            SHAREABLE = 0x400,
            STORAGE = 0x800
        }

        [Flags]
        public enum SHGDN
        {
            NORMAL = 0,
            INFOLDER = 1,
            FORADDRESSBAR = 16384,
            FORPARSING = 32768
        }

        [Flags]
        public enum ILD
        {
            NORMAL = 0x0,
            TRANSPARENT = 0x1,
            BLEND25 = 0x2,
            SELECTED = 0x4,
            MASK = 0x10,
            IMAGE = 0x20,
            ROP = 0x40,
            PRESERVEALPHA = 0x1000,
            SCALE = 0x2000,
            DPISCALE = 0x4000
        }

        public enum ILS
        {
            NORMAL = 0x0,
            GLOW = 0x1,
            SHADOW = 0x2,
            SATURATE = 0x4,
            ALPHA = 0x8
        }

        [Flags]
        public enum SLR
        {
            NO_UI = 0x1,
            ANY_MATCH = 0x2,
            UPDATE = 0x4,
            NOUPDATE = 0x8,
            NOSEARCH = 0x10,
            NOTRACK = 0x20,
            NOLINKINFO = 0x40,
            INVOKE_MSI = 0x80,
            NO_UI_WITH_MSG_PUMP = 0x101
        }

        [Flags]
        public enum SLGP
        {
            SHORTPATH = 0x1,
            UNCPRIORITY = 0x2,
            RAWPATH = 0x4
        }

        [Flags]
        public enum SHGNLI
        {
            PIDL = 1,
            PREFIXNAME = 2,
            NOUNIQUE = 4,
            NOLNK = 8
        }
        #endregion

        #region Shell GUIDs
        public static Guid IID_IMalloc = new Guid("00000002-0000-0000-C000-000000000046");
        public static Guid IID_IShellFolder = new Guid("000214E6-0000-0000-C000-000000000046");
        public static Guid IID_IFolderFilterSite = new Guid("C0A651F5-B48B-11d2-B5ED-006097C686F6");
        public static Guid IID_IFolderFilter = new Guid("9CC22886-DC8E-11d2-B1D0-00C04F8EEB3E");
        public static Guid DesktopGUID = new Guid("00021400-0000-0000-C000-000000000046");
        public static Guid CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");
        public static Guid CLSID_InternetShortcut = new Guid("FBF23B40-E3F0-101B-8488-00AA003E56F8");
        public static Guid IID_IDropTarget = new Guid("00000122-0000-0000-C000-000000000046");
        public static Guid IID_IDataObject = new Guid("0000010e-0000-0000-C000-000000000046");
        #endregion

        #region Shell Structures

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        public static int cbFileInfo = Marshal.SizeOf(typeof(SHFILEINFO));

        [StructLayout(LayoutKind.Explicit)]
        public struct STRRET
        {
            [FieldOffset(0)]
            public int uType;
            [FieldOffset(4)]
            public int pOleStr;
            [FieldOffset(4)]
            public int uOffset;
            [FieldOffset(4)]
            public int pStr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WIN32_FIND_DATA
        {
            public int dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        #endregion

        #region Dll Declarations

        [DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern int SHGetMalloc(out IMalloc pMalloc);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int SHGetDesktopFolder(out IShellFolder ppshf);

        [DllImport("Shell32")]
        public static extern int SHGetSpecialFolderLocation(int hWndOwner, int csidl, out IntPtr ppidl);

        [DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO sfi,
            int cbsfi,
            int uFlags);

        [DllImport("shell32", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(
            IntPtr ppidl,
            int dwFileAttributes,
            ref SHFILEINFO sfi,
            int cbsfi,
            int uFlags);

        [DllImport("shell32", EntryPoint = "SHGetNewLinkInfoA", CharSet = CharSet.Ansi)]
        public static extern int SHGetNewLinkInfo(
            string pszLinkTo,
            string pszDir,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName,
            ref bool pfMustCopy,
            SHGNLI uFlags);

        [DllImport("shell32", EntryPoint = "SHGetNewLinkInfoA", CharSet = CharSet.Ansi)]
        public static extern int SHGetNewLinkInfo(
            IntPtr pszLinkTo,
            string pszDir,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName,
            ref bool pfMustCopy,
            SHGNLI uFlags);

        [DllImport("shell32", EntryPoint = "#23", CharSet = CharSet.Auto)]
        public static extern bool ILIsParent(IntPtr pidlParent, IntPtr pidlBelow, bool fImmediate);

        [DllImport("shell32", EntryPoint = "#21", CharSet = CharSet.Auto)]
        public static extern bool ILIsEqual(IntPtr pidl1, IntPtr pidl2);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int StrRetToBSTR(
            ref STRRET pstr,
            IntPtr pidl,
            [MarshalAs(UnmanagedType.BStr)] ref string pbstr);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern int StrRetToBuf(
            IntPtr pstr,
            IntPtr pidl,
            StringBuilder pszBuf,
            [MarshalAs(UnmanagedType.U4)] int cchBuf);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern int SendMessage(
            IntPtr hWnd,
            int wMsg,
            int wParam,
            IntPtr lParam);

        [DllImport("comctl32")]
        public static extern int ImageList_GetIconSize(
            IntPtr himl,
            ref int cx,
            ref int cy);

        [DllImport("comctl32", CharSet = CharSet.Auto)]
        public static extern int ImageList_ReplaceIcon(
            IntPtr hImageList,
            int IconIndex,
            IntPtr hIcon);

        [DllImport("comctl32")]
        public static extern IntPtr ImageList_GetIcon(
            IntPtr himl,
            int i,
            int flags);

        [DllImport("comctl32")]
        public static extern int ImageList_Draw(
            IntPtr hIml,
            int indx,
            IntPtr hdcDst,
            int x,
            int y,
            int fStyle);

        [DllImport("user32.dll")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern int RegisterClipboardFormat(string lpszFormat);

        [DllImport("ole32.dll", CharSet = CharSet.Auto)]
        public static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);

        [DllImport("ole32.dll", CharSet = CharSet.Auto)]
        public static extern int RegisterDragDrop(IntPtr hWnd, IDropTarget IdropTgt);

        [DllImport("ole32.dll", CharSet = CharSet.Auto)]
        public static extern int RevokeDragDrop(IntPtr hWnd);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int DragQueryFile(
            IntPtr hDrop,
            int iFile,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFile,
            int cch);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        #endregion

        #region Drag/Drop Enums and Structures

        public enum CF
        {
            TEXT = 1,
            BITMAP = 2,
            METAFILEPICT = 3,
            SYLK = 4,
            DIF = 5,
            TIFF = 6,
            OEMTEXT = 7,
            DIB = 8,
            PALETTE = 9,
            PENDATA = 10,
            RIFF = 11,
            WAVE = 12,
            UNICODETEXT = 13,
            ENHMETAFILE = 14,
            HDROP = 15,
            LOCALE = 16,
            MAX = 17,
            OWNERDISPLAY = 0x80,
            DSPTEXT = 0x81,
            DSPBITMAP = 0x82,
            DSPMETAFILEPICT = 0x83,
            DSPENHMETAFILE = 0x8E,
            PRIVATEFIRST = 0x200,
            PRIVATELAST = 0x2FF,
            GDIOBJFIRST = 0x300,
            GDIOBJLAST = 0x3FF
        }

        [Flags]
        public enum DVASPECT
        {
            CONTENT = 1,
            THUMBNAIL = 2,
            ICON = 4,
            DOCPRINT = 8
        }

        [Flags]
        public enum TYMED
        {
            HGLOBAL = 1,
            FILE = 2,
            ISTREAM = 4,
            ISTORAGE = 8,
            GDI = 16,
            MFPICT = 32,
            ENHMF = 64,
            NULL = 0
        }

        [Flags]
        public enum ADVF
        {
            NODATA = 1,
            PRIMEFIRST = 2,
            ONLYONCE = 4,
            DATAONSTOP = 64,
            CACHE_NOHANDLER = 8,
            CACHE_FORCEBUILTIN = 16,
            CACHE_ONSAVE = 32
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct FORMATETC
        {
            public CF cfFormat;
            public IntPtr ptd;
            public DVASPECT dwAspect;
            public int lindex;
            public TYMED Tymd;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STGMEDIUM
        {
            public int tymed;
            public IntPtr hGlobal;
            public IntPtr pUnkForRelease;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DROPFILES
        {
            public int pFiles;
            public POINT pt;
            public bool fNC;
            public bool fWide;
        }

        #endregion

        #region Shell Interfaces

        [ComVisible(false)]
        [ComImport, Guid("00000000-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IUnknown
        {
            [PreserveSig]
            int QueryInterface(ref Guid riid, ref IntPtr pVoid);
            [PreserveSig]
            int AddRef();
            [PreserveSig]
            int Release();
        }

        [ComImport, Guid("00000002-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc(int cb);
            [PreserveSig]
            IntPtr Realloc(IntPtr pv, int cb);
            [PreserveSig]
            void Free(IntPtr pv);
            [PreserveSig]
            int GetSize(IntPtr pv);
            [PreserveSig]
            short DidAlloc(IntPtr pv);
            [PreserveSig]
            void HeapMinimize();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E6-0000-0000-C000-000000000046")]
        public interface IShellFolder
        {
            [PreserveSig]
            int ParseDisplayName(
                int hwndOwner,
                IntPtr pbcReserved,
                [MarshalAs(UnmanagedType.LPWStr)] string lpszDisplayName,
                ref int pchEaten,
                ref IntPtr ppidl,
                ref int pdwAttributes);

            [PreserveSig]
            int EnumObjects(
                int hwndOwner,
                [MarshalAs(UnmanagedType.U4)] SHCONTF grfFlags,
                out IEnumIDList ppenumIDList);

            [PreserveSig]
            int BindToObject(
                IntPtr pidl,
                IntPtr pbcReserved,
                ref Guid riid,
                out IShellFolder ppvOut);

            [PreserveSig]
            int BindToStorage(
                IntPtr pidl,
                IntPtr pbcReserved,
                ref Guid riid,
                IntPtr ppvObj);

            [PreserveSig]
            int CompareIDs(
                IntPtr lParam,
                IntPtr pidl1,
                IntPtr pidl2);

            [PreserveSig]
            int CreateViewObject(
                IntPtr hwndOwner,
                ref Guid riid,
                out IUnknown ppvOut);

            [PreserveSig]
            int GetAttributesOf(
                int cidl,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl,
                ref SFGAO rgfInOut);

            [PreserveSig]
            int GetUIObjectOf(
                IntPtr hwndOwner,
                int cidl,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] apidl,
                ref Guid riid,
                ref int prgfInOut,
                out IUnknown ppvOut);

            [PreserveSig]
            int GetDisplayNameOf(
                IntPtr pidl,
                [MarshalAs(UnmanagedType.U4)] SHGDN uFlags,
                IntPtr lpName);

            [PreserveSig]
            int SetNameOf(
                int hwndOwner,
                IntPtr pidl,
                [MarshalAs(UnmanagedType.LPWStr)] string lpszName,
                [MarshalAs(UnmanagedType.U4)] SHCONTF uFlags,
                ref IntPtr ppidlOut);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F2-0000-0000-C000-000000000046")]
        public interface IEnumIDList
        {
            [PreserveSig]
            int GetNext(
                int celt,
                ref IntPtr rgelt,
                ref int pceltFetched);

            [PreserveSig]
            int Skip(int celt);

            [PreserveSig]
            int Reset();

            [PreserveSig]
            int Clone(out IEnumIDList ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010B-0000-0000-C000-000000000046")]
        public interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            [PreserveSig]
            int IsDirty();
            int Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
            int Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            int SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            int GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214EE-0000-0000-C000-000000000046")]
        public interface IShellLink
        {
            int GetPath([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxPath, out WIN32_FIND_DATA pfd, SLGP fFlags);
            int GetIDList(out IntPtr ppidl);
            int SetIDList(IntPtr pidl);
            int GetDescription([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, int cchMaxName);
            int SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);
            int GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);
            int SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);
            int GetArguments([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);
            int SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);
            int GetHotkey(out short pwHotkey);
            int SetHotkey(short wHotkey);
            int GetShowCmd(out int piShowCmd);
            int SetShowCmd(int iShowCmd);
            int GetIconLocation([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            int SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);
            int SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, int dwReserved);
            int Resolve(IntPtr hwnd, SLR fFlags);
            int SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000103-0000-0000-C000-000000000046")]
        public interface IEnumFORMATETC
        {
            [PreserveSig]
            int GetNext(int celt, ref FORMATETC rgelt, ref int pceltFetched);
            [PreserveSig]
            int Skip(int celt);
            [PreserveSig]
            int Reset();
            [PreserveSig]
            int Clone(out IEnumFORMATETC ppenum);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010e-0000-0000-C000-000000000046")]
        public interface IDataObject
        {
            [PreserveSig]
            int GetData(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium);
            [PreserveSig]
            int GetDataHere(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium);
            [PreserveSig]
            int QueryGetData(ref FORMATETC pformatetc);
            [PreserveSig]
            int GetCanonicalFormatEtc(FORMATETC pformatetc, ref FORMATETC pformatetcout);
            [PreserveSig]
            int SetData(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium, bool frelease);
            [PreserveSig]
            int EnumFormatEtc(int dwDirection, out IEnumFORMATETC ppenumFormatEtc);
            [PreserveSig]
            int DAdvise(ref FORMATETC pformatetc, ADVF advf, IAdviseSink pAdvSink, ref int pdwConnection);
            [PreserveSig]
            int DUnadvise(int dwConnection);
            [PreserveSig]
            int EnumDAdvise(out object ppenumAdvise);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010f-0000-0000-C000-000000000046")]
        public interface IAdviseSink
        {
            void OnDataChange(ref FORMATETC pformatetcIn, ref STGMEDIUM pmedium);
            void OnViewChange(int dwAspect, int lindex);
            void OnRename(IntPtr pmk);
            void OnSave();
            void OnClose();
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000122-0000-0000-C000-000000000046")]
        public interface IDropTarget
        {
            [PreserveSig]
            int DragEnter(IntPtr pDataObj, int grfKeyState, POINT pt, ref int pdwEffect);
            [PreserveSig]
            int DragOver(int grfKeyState, POINT pt, ref int pdwEffect);
            [PreserveSig]
            int DragLeave();
            [PreserveSig]
            int DragDrop(IntPtr pDataObj, int grfKeyState, POINT pt, ref int pdwEffect);
        }

        #endregion

        #region Public Shared Methods

        public static string GetSpecialFolderPath(IntPtr hWnd, int csidl)
        {
            IntPtr ppidl = GetSpecialFolderLocation(hWnd, csidl);
            SHFILEINFO shfi = new SHFILEINFO();
            int uFlags = (int)(SHGFI.PIDL | SHGFI.DISPLAYNAME | SHGFI.TYPENAME);
            int dwAttr = 0;
            IntPtr res = SHGetFileInfo(ppidl, dwAttr, ref shfi, cbFileInfo, uFlags);
            Marshal.FreeCoTaskMem(ppidl);
            return shfi.szDisplayName + "  (" + shfi.szTypeName + ")";
        }

        public static IntPtr GetSpecialFolderLocation(IntPtr hWnd, int csidl)
        {
            IntPtr rVal;
            int res = SHGetSpecialFolderLocation(0, csidl, out rVal);
            return rVal;
        }

        public static bool IsXpOrAbove()
        {
            bool rVal = false;
            if (Environment.OSVersion.Version.Major > 5)
            {
                rVal = true;
            }
            else if (Environment.OSVersion.Version.Major == 5 &&
                     Environment.OSVersion.Version.Minor >= 1)
            {
                rVal = true;
            }
            return rVal;
        }

        public static bool Is2KOrAbove()
        {
            return Environment.OSVersion.Version.Major >= 5;
        }

        #endregion
    }
}