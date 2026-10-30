VERSION 5.00
Begin {C62A69F0-16DC-11CE-9E98-00AA00574A4F} UserSelect_Form 
   Caption         =   "32分選碼選擇"
   ClientHeight    =   3015
   ClientLeft      =   120
   ClientTop       =   465
   ClientWidth     =   4560
   OleObjectBlob   =   "UserSelect_Form.frx":0000
   StartUpPosition =   1  '所屬視窗中央
End
Attribute VB_Name = "UserSelect_Form"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Public SelectedValue As Boolean

Public IsConfirmed As Boolean



Private Sub optContinue_Click()
 
   ' Option 1 = True
    SelectedValue = True

End Sub

Private Sub optNotContinue_Click()
   
   ' Option 2 = False
    SelectedValue = False

End Sub

Private Sub UserForm_Initialize()

    ' 預設不選
    optContinue.Value = False
    optNotContinue.Value = False

    SelectedValue = False
    IsConfirmed = False
       
End Sub

Private Sub UserForm_QueryClose( _
    Cancel As Integer, _
    CloseMode As Integer)

    ' 使用者按右上角 X
    If CloseMode = vbFormControlMenu Then

        IsConfirmed = False

        Me.Hide

        Cancel = True

    End If

End Sub

Private Sub cmdConfirm_Click()
   
   
   '=================================================
    ' 必須選擇 Option 1 或 Option 2
    '=================================================
    If Not optContinue.Value And Not optNotContinue.Value Then

        MsgBox "請先選擇 連續 或 非連續 其中一狀態再進續執行。", _
               vbExclamation, "提示"

        ' 停留在 UserForm
        Exit Sub

    End If
    
    
    '=================================================
    ' 重新確認最後選擇
    '=================================================

    If optContinue.Value Then

        ' Option 1 = True
        SelectedValue = True

    ElseIf optNotContinue.Value Then

        ' Option 2 = False
        SelectedValue = False

    End If
    
    
    '=================================================
    ' 條件成立，正式確認
    '=================================================
    IsConfirmed = True

    ' 將 UserForm 隱藏
    Me.Hide
  

End Sub

