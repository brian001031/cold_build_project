VERSION 5.00
Begin {C62A69F0-16DC-11CE-9E98-00AA00574A4F} frmPDFProgress 
   Caption         =   "UserForm1"
   ClientHeight    =   4740
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   6645
   OleObjectBlob   =   "frmPDFProgress.frx":0000
   StartUpPosition =   1  '所屬視窗中央
End
Attribute VB_Name = "frmPDFProgress"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

'=====================================================
' UserForm 初始化
'=====================================================
Private Sub UserForm_Initialize()

    lblTitle.Caption = "PDF 產生進度"

    lblStatus.Caption = "準備開始..."

    lblPercent.Caption = "0%"

    lblProgress.Width = 0

End Sub


Private Sub UserForm_QueryClose( _
    Cancel As Integer, _
    CloseMode As Integer)

    '=====================================================
    ' PDF 執行期間禁止使用者關閉 UserForm
    '=====================================================
    If CloseMode = vbFormControlMenu Then

        Cancel = True
       

    End If

End Sub
