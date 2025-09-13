Public Class NativeTreeView : Inherits TreeView

    Public Declare Unicode Function SetWindowTheme Lib "uxtheme.dll" (ByVal hWnd As IntPtr, ByVal pszSubAppName As String, ByVal pszSubIdList As String) As Integer

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub

    Protected Overrides Sub CreateHandle()
        MyBase.CreateHandle()
        SetWindowTheme(Me.Handle, "Explorer", Nothing)
    End Sub

End Class

