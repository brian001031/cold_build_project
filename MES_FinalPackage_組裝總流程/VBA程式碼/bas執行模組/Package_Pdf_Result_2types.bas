Option Explicit

Const xlTypePDF = 0
Const xlQualityStandard = 0
Const xlLandscape = 2
Const xlCenter = -4108
Const xlContinuous = 1
Const xlThin = 2

'=========================================================
' Model / Location PDF組裝自動產生工具 '
' 參數註解意義
' num         → 小數位數
' vbTrue      → 使用千分位
' vbFalse     → 不使用括號表示負數
' False       → 不使用系統負數格式
'=========================================================

Sub Generate_Package_PDF()

    Dim fso As Object
    Dim xlsPath As String
    Dim outputFolder As String
    Dim currentDateFolder As String

    Dim wb As Workbook
    Dim ws As Worksheet
    Dim reportWs As Worksheet

    Dim headerMap As Object
        
    Dim lastRow As Long
    Dim lastCol As Long
    Dim startRow As Long
    Dim endRow As Long
    Dim groupNo As Long
    Dim groupCount As Long

    Dim pdfPath As String
    Dim modelName As String

    Dim vashcCol As Long
    Dim mohmCol As Long
    Dim kCol As Long
    Dim voltageCol As Long
    Dim modelCol As Long

    Dim firstDataCol As Long
    Dim lastDataCol As Long
    Dim type_classsCol As Long
	Dim allocate_Col As Long
	Dim machine_workdate_Col As Long

    Dim r As Long
    Dim c As Long

    Dim vashcMin As Variant
    Dim vashcMax As Variant
    Dim vashcAvg As Variant
    Dim vashcSum As Double
    Dim vashcCount As Long

    Dim mohmMin As Variant
    Dim mohmMax As Variant
    Dim kMin As Variant
    Dim kMax As Variant
    Dim voltageMin As Variant
    Dim voltageMax As Variant

    Dim typeclass_char As Variant

    Dim val As Variant	 
	Dim regis_val_temp As Double
    Dim firstModel As String
    Dim pdfCount As Long
	Dim work_datestr As String

    Dim currentDateTime As String
	Dim classify_member As String
	
	Dim totalGroups As Long
	Dim progressPercent As Long
	
	'判斷是否連續或非連續開檔
	Dim isContinue_ClassId As Boolean
	
	Dim g_classID As String
	
	Dim g_StartRow As Long
	Dim g_EndRow As Long
	Dim g_isComplete32 As Boolean
	
	'計算總阻抗
	Dim All_Resist_radio As Double
	
	Dim oldCalculation As XlCalculation
    Dim oldScreenUpdating As Boolean
    Dim oldEnableEvents As Boolean
	
	On Error GoTo ErrorHandler
	
	'=====================================================
    ' 保存 Excel 原始設定
    '=====================================================
    oldCalculation = Application.Calculation
    oldScreenUpdating = Application.ScreenUpdating
    oldEnableEvents = Application.EnableEvents
    Application.ScreenUpdating = False
    Application.EnableEvents = False
    Application.Calculation = xlCalculationManual
	
	

    typeclass_char = Empty

    Set fso = CreateObject("Scripting.FileSystemObject")

    xlsPath = SelectXLSFile(isContinue_ClassId)
    
    If xlsPath = "" Then
	
	    MsgBox "已取消檔案選擇或檔名格式錯誤。", _	   
              vbInformation, _
              "程序結束"
			  
       GoTo SafeExit
	   
    End If

    outputFolder = fso.GetParentFolderName(xlsPath) & "\PDF_PACKAGE"
    
    If Not fso.FolderExists(outputFolder) Then
       fso.CreateFolder outputFolder
    End If
    
    ' 建立當前日期資料夾
    currentDateFolder = outputFolder & "\" & _
                 Year(Date) & "-" & _
                 Right("0" & Month(Date), 2) & "-" & _
                 Right("0" & Day(Date), 2)
                
    If Not fso.FolderExists(currentDateFolder) Then
        fso.CreateFolder currentDateFolder
    End If

    ' 最後輸出資料夾
    outputFolder = currentDateFolder
    
    '先清除內部所有組裝pdf檔案
    outputFolder = ClearPdfAndGetFileName(outputFolder)
    
    '取得當前日期時間
    'currentDateTime = Format(Now, "yyyy/m/d H:mm")

    'Set xlApp = CreateObject("Excel.Application")
    'xlApp.Visible = False
    'xlApp.DisplayAlerts = False
    'xlApp.ScreenUpdating = False
    
    Application.ScreenUpdating = False
    Application.DisplayAlerts = False

    On Error Resume Next
    
    Set wb = Workbooks.Open(xlsPath, False, True)
    
    If Err.Number <> 0 Then
        MsgBox "XLSX 開啟失敗：" & Err.Description, vbCritical
        Err.Clear

        Application.ScreenUpdating = True
        Application.DisplayAlerts = True

        Exit Sub
    End If
    
    On Error GoTo 0

    '=====================================================
    ' 使用第一個工作表
    '=====================================================
    
    '--------------------
    ' -4162 = xlUp
    ' -4152 = xlToLeft
    '-------------------

    Set ws = wb.Worksheets(1)
    
    'lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    'lastCol = ws.Cells(1, ws.Columns.Count).End(xlToLeft).Column
    
    With ws

        ' 最後一筆資料的 Row
        lastRow = .Cells.Find( _
            What:="*", _
            After:=.Cells(1, 1), _
            LookAt:=xlPart, _
            LookIn:=xlFormulas, _
            SearchOrder:=xlByRows, _
            SearchDirection:=xlPrevious, _
            MatchCase:=False _
        ).Row

        ' 最後一筆資料的 Column
        lastCol = .Cells.Find( _
            What:="*", _
            After:=.Cells(1, 1), _
            LookAt:=xlPart, _
            LookIn:=xlFormulas, _
            SearchOrder:=xlByColumns, _
            SearchDirection:=xlPrevious, _
            MatchCase:=False _
        ).Column

    End With

    Set headerMap = CreateObject("Scripting.Dictionary")
    
    BuildHeaderMap ws, headerMap, lastCol

    '分配電芯系統作業日期時間
	RequireHeader headerMap, "machine_workTime"

    RequireHeader headerMap, "PLCCellID_CE"
    RequireHeader headerMap, "parallel_match"
    RequireHeader headerMap, "3.5-2.8V_mAH"
    RequireHeader headerMap, "acirRP12_CE"
    RequireHeader headerMap, "K_Value"
    RequireHeader headerMap, "acirVP12_CE"
    RequireHeader headerMap, "model_combine_number"
    '32分選號
    RequireHeader headerMap, "PLCCellIDClass_CE"
	'阻值分配位置
	RequireHeader headerMap, "last_define_location"

	
	
	 
    firstDataCol = headerMap("PLCCellID_CE")
    lastDataCol = headerMap("parallel_match")
    type_classsCol = headerMap("PLCCellIDClass_CE")
	
	
	allocate_Col = headerMap("last_define_location")
	
	machine_workdate_Col = headerMap("machine_workTime")

    If firstDataCol > lastDataCol Then
    
        MsgBox "錯誤：PLCCellID_CE 必須位於 parallel_match 左側。", vbCritical
        
        wb.Close False
        Application.ScreenUpdating = True
        Application.DisplayAlerts = True
        
        Exit Sub
        
    End If

    vashcCol = headerMap("3.5-2.8V_mAH")
    mohmCol = headerMap("acirRP12_CE")
    kCol = headerMap("K_Value")
    voltageCol = headerMap("acirVP12_CE")
    modelCol = headerMap("model_combine_number")
    
	'=====================================================
    ' 配對員輸入姓名
    '=====================================================
	
	classify_member  = InputBox( _
        "請輸入配對執行者姓名", _
        "Name設定", _
        "自動")
    
    If Trim(classify_member) = "" Then
	
       MsgBox "未輸入配對員姓名! ", vbExclamation
       GoTo SafeExit
	   
    End If
	
	If  IsNumeric(classify_member) Then
	
       MsgBox "配對員姓名不能輸入數字格式!", vbExclamation
       GoTo SafeExit
	   
    End If
	
	
	startRow = 2
    groupNo = 0
    pdfCount = 0
	
	' 總 Group 數
    totalGroups = ((lastRow - startRow + 1) + 31) \ 32
	
	
	'=====================================================
	' 顯示 PDF 進度視窗
	'=====================================================
	frmPDFProgress.Show vbModeless

	DoEvents
	
    
    '=====================================================
    ' 每 32 筆產生一份 PDF
    '=====================================================
	
    Do While startRow <= lastRow
	
	
		'=================================================
		' 預設每次循環 Group 狀態
		'=================================================

		g_StartRow = 0
		g_EndRow = 0

		g_isComplete32 = False
	
	    '=================================================
        ' 取得本次完整 32 顆範圍
        '=================================================

        If isContinue_ClassId Then
		 
		 
			 '=================================================
			' True
			'
			' ClassID 可以接續
			' 找到 Location 16,16 才結束
			'=================================================

			Call FindContinuous32Group( _
					ws, _
					startRow, _
					lastRow, _
					type_classsCol, _
					allocate_Col, _
					g_StartRow, _
					g_EndRow, _
					g_isComplete32)
					
        Else
            
			'=================================================
			' False
			'
			' 每一個 ClassID 自己必須完整 32 顆
			'=================================================

			Call FindSingleClass32Group( _
					ws, _
					startRow, _
					lastRow, _
					type_classsCol, _
					allocate_Col, _					
					g_StartRow, _
					g_EndRow, _
					g_classID, _
					g_isComplete32)

		 
	    End If
	
	
	    '=================================================
		' 沒有更多資料
		'=================================================

		If g_StartRow = 0 Then
		
			Exit Do
			
		End If
		
	
		' 不完整的結構 未滿足符合 32顆 ClassID 同group 
		' 不產生 PDF
        ' 繼續尋找下一個群組
		
		If Not g_isComplete32 Then

			startRow = g_EndRow + 1

			DoEvents

			GoTo ContinuePDFLoop

		End If
	
	
	    '=================================================
		' 確認完整 32 顆
		'=================================================

		startRow = g_StartRow
		endRow = g_EndRow
	

        groupNo = groupNo + 1
		
		'=================================================
		' 取得當前日期時間
		' 每 32 筆重新取得一次
		' 格式：YYYY-MM-DD HH:mm:ss
		'=================================================
		currentDateTime = Format(Now, "yyyy-mm-dd HH:mm:ss")
        
        'endRow = startRow + 31
        
        '當前計算到最後行數超出事先計算,直接賦予
        If endRow > lastRow Then
             endRow = lastRow
        End If
			
		
		' 顯示進度
		'progressPercent = Int((groupNo / totalGroups) * 100)
		'progressPercent = Int( _
		'        ((endRow - 1 ) /  (lastRow - 1)) * 100)
		
	    'CStr(groupNo) & " / " & CStr(totalGroups) & _

		'Application.StatusBar = _
		'	"PDF 產生進度：" & _
		'	 CStr(pdfCount) & " 組" & _		
		'	" (" & CStr(progressPercent) & "%)" & _
		'	"　資料：" & _
		'	CStr(startRow - 1) & " ~ " & _
		'	CStr(endRow - 1)

		'DoEvents
        
              
        groupCount = endRow - startRow + 1


        '=================================================
        ' 初始化統計值
        '=================================================

        regis_val_temp = 0#
        vashcMin = Empty: vashcMax = Empty
        vashcSum = 0: vashcCount = 0
        mohmMin = Empty: mohmMax = Empty
        kMin = Empty: kMax = Empty
        voltageMin = Empty: voltageMax = Empty
		
		' 取得此 32 筆群組第一筆的 Class Type
        typeclass_char = GetEnglishLetters( _
                         ws.Cells(startRow, type_classsCol).Value)

        
		'=================================================
        ' 逐筆統計
        '=================================================

        For r = startRow To endRow
					
            val = NumericCellValue(ws.Cells(r, vashcCol).Value)
            
            If val <> "" Then
            
                If vashcCount = 0 Then
                
                    vashcMin = CDbl(val)
                    vashcMax = CDbl(val)
                    
                Else
                
                    If CDbl(val) < CDbl(vashcMin) Then
                         vashcMin = CDbl(val)
                    End If
                         
                    If CDbl(val) > CDbl(vashcMax) Then
                        vashcMax = CDbl(val)
                    End If
                    
                End If
                
                vashcSum = vashcSum + CDbl(val)
                vashcCount = vashcCount + 1
                
            End If

            val = NumericCellValue(ws.Cells(r, mohmCol).Value)
            
            If val <> "" Then
						              
                If IsEmpty(mohmMin) Then
                
                    mohmMin = CDbl(val): mohmMax = CDbl(val)
                    
                Else
                
                    If CDbl(val) < CDbl(mohmMin) Then
                        mohmMin = CDbl(val)
                    End If
                    
                    If CDbl(val) > CDbl(mohmMax) Then
                       mohmMax = CDbl(val)
                    End If
                    
                End If
                
            End If

            val = NumericCellValue(ws.Cells(r, kCol).Value)
            If val <> "" Then
                If IsEmpty(kMin) Then
                
                    kMin = CDbl(val)
                    kMax = CDbl(val)
                    
                Else
                
                    If CDbl(val) < CDbl(kMin) Then
                        kMin = CDbl(val)
                    End If
                    
                    If CDbl(val) > CDbl(kMax) Then
                         kMax = CDbl(val)
                    End If
                    
                End If
                
            End If

            val = NumericCellValue(ws.Cells(r, voltageCol).Value)
            
            If val <> "" Then
            
                If IsEmpty(voltageMin) Then
                
                    voltageMin = CDbl(val)
                    voltageMax = CDbl(val)
                    
                Else
                    If CDbl(val) < CDbl(voltageMin) Then
                         voltageMin = CDbl(val)
                    End If
                    
                    If CDbl(val) > CDbl(voltageMax) Then
                         voltageMax = CDbl(val)
                    End If
                    
                End If
                
            End If
			
			
			val = NumericCellValue(ws.Cells(r, lastDataCol).Value)

		    '暫存並聯阻抗值累加	
            If val <> "" Then
			
				regis_val_temp = _
  				    regis_val_temp + CDbl(val)
			
			End If
					
        Next r
        
        '=================================================
        ' VASHC 平均
        '=================================================

        If vashcCount > 0 Then
            vashcAvg = CDbl(vashcSum / vashcCount) / 1000
        Else
            vashcAvg = Empty
        End If
        
        '=================================================
        ' Model
        '=================================================

        firstModel = Trim(CStr(ws.Cells(startRow, modelCol).Value))
        
        If firstModel = "" Then
           firstModel = "Model"
        End If
        
        modelName = CleanFileName(firstModel)
		
		'=================================================
        ' 計算總阻抗值 *1000 
        '=================================================
		
		If regis_val_temp > 0 Then
		
            All_Resist_radio =  regis_val_temp * 1000
			
        Else
		
            All_Resist_radio =  0#
			
        End If
		
		
		' 取得作業日期
		If IsDate(ws.Cells(startRow, machine_workdate_Col).Value) Then
		
		    
		   work_datestr = Format( _
				CDate(ws.Cells(startRow, machine_workdate_Col).Value), _
				"yyyy_mm_dd" _
		   )
		
		Else
		
		   work_datestr = ""
		
		End IF
		

        '=================================================
        ' 建立報表 Sheet
        '=================================================
        
        Set reportWs = wb.Worksheets.Add
        
        reportWs.Name = "PDF_" & Right("000" & CStr(groupNo), 3)

        BuildReport reportWs, ws, _
                    startRow, endRow, _
                    firstDataCol, lastDataCol, _
                    vashcMin, vashcMax, vashcAvg, _
                    mohmMin, mohmMax, _
                    kMin, kMax, _
                    voltageMin, voltageMax, _
                    firstModel, _
                    currentDateTime, _
                    typeclass_char, _
					classify_member, _
					All_Resist_radio

        pdfPath = outputFolder & "\" & _
                    modelName & "_" & _
					work_datestr & "_" & _
                    Right("000" & CStr(groupNo), 3) & ".pdf"
                        
        'pdfPath = GetUniqueFileName(pdfPath)

        reportWs.ExportAsFixedFormat _
                  Type:=xlTypePDF, _
                  Filename:=pdfPath, _
                  Quality:=xlQualityStandard, _
                  IncludeDocProperties:=True, _
                  IgnorePrintAreas:=False

        reportWs.Delete
        
        Set reportWs = Nothing

        pdfCount = pdfCount + 1
        
        startRow = endRow + 1
		
	ContinuePDFLoop:

          DoEvents

    Loop

    wb.Close False
    
    '=====================================================
    ' 完成
    '=====================================================
    Unload frmPDFProgress

    Application.StatusBar = False
 

    MsgBox "PDF 產生完成。" & vbCrLf & _
                 "Group 數量：" & pdfCount & vbCrLf & _
                 "輸出資料夾：" & outputFolder, _
                  vbInformation, _
                 "PDF 產生完成"
				 
	SafeExit:

        On Error Resume Next

        Application.DisplayAlerts = True
        Application.Calculation = oldCalculation
        Application.ScreenUpdating = oldScreenUpdating
        Application.EnableEvents = oldEnableEvents

    Exit Sub			 
    
    ErrorHandler:

        On Error Resume Next

		Unload frmPDFProgress

		Application.StatusBar = False

		MsgBox _
			"PDF 產生失敗。" & vbCrLf & _
			"錯誤：" & Err.Description, _
			vbCritical, _
			"PDF 產生錯誤"
	
End Sub


Sub BuildHeaderMap(ByRef targetWs As Worksheet, ByRef dict As Object, ByVal colCount As Long)
    
    Dim c As Long
    Dim rawHeader As String
    Dim cleanHeader As String

    ' 不區分大小寫
    dict.CompareMode = vbTextCompare
             
    For c = 1 To colCount
	
		rawHeader = CStr(targetWs.Cells(1, c).Value)
		
		cleanHeader = NormalizeHeader(rawHeader)
		
		If cleanHeader <> "" Then
		
			If Not dict.Exists(cleanHeader) Then
			
				 dict.Add cleanHeader, c
				 
			End If
			
		End If
		
    Next c
    
End Sub

Sub RequireHeader(ByRef dict As Object, ByVal h As String)

		Dim cleanHeader As String
		
		cleanHeader = NormalizeHeader(h)

        If Not dict.Exists(cleanHeader) Then
        
            MsgBox "XLSX 缺少必要欄位：" & h, vbCritical
            
            Err.Raise vbObjectError + 1000, _
                  "Generate_PDF", _
                  "XLSX 缺少必要欄位：" & h
            
        End If
        
End Sub

Sub BuildReport( _
    ByRef rpt As Worksheet, _
    ByRef sourceWs As Worksheet, _
    ByVal startRow As Long, _
    ByVal endRow As Long, _
    ByVal firstDataCol As Long, _
    ByVal lastDataCol As Long, _
    ByVal vashcMin As Variant, _
    ByVal vashcMax As Variant, _
    ByVal vashcAvg As Variant, _
    ByVal mohmMin As Variant, _
    ByVal mohmMax As Variant, _
    ByVal kMin As Variant, _
    ByVal kMax As Variant, _
    ByVal voltageMin As Variant, _
    ByVal voltageMax As Variant, _
    ByVal modelName As String, _
    ByVal Dateformat As String, _
    ByVal cell_class As String, _
	ByVal cell_member As String, _
	ByVal All_Resist_Sum As Double)
	

    Dim headers As Variant
    Dim values1 As Variant
    Dim values2 As Variant

    Dim c As Long
    Dim r As Long
    Dim bodyRow As Long
    Dim bodyCol As Long
	Dim summary_start As Long

    '=====================================================
    ' Summary Data
    '=====================================================

    headers = Array( _
        "挑選作業", _
        "模組號", _
        "MIN", _
        "Max", _
        "mAh", _
        "AC-IR(mOhm)", _
        "K-max", _
        "K-min", _
        "voltage", _
        "配對員", _
        "覆核員" _
    )

    values1 = Array( _
        "執行中", _
        modelName, _
        FormatStat(vashcMin, 1), _
        FormatStat(vashcMax, 1), _
        FormatStat(vashcAvg, 3), _
        FormatStat(All_Resist_Sum, 2), _
        FormatStat(kMax, 5), _
        FormatStat(kMin, 5), _
        FormatRange(voltageMin, voltageMax), _
        Trim(cell_member), _
        "" _
    )

    values2 = Array( _
        Dateformat, _
        "K值群組", _
        cell_class, _
        "", _
        "ACIR(max-min)", _
        FormatRange(mohmMin, mohmMax), _
        "", _
        "", _
        "", _
        "", _
        "" _
    )

    '=====================================================
    ' Page Setup
    '=====================================================

    With rpt.PageSetup

        .Orientation = xlLandscape

        .LeftMargin = 18
        .RightMargin = 18
        .TopMargin = 18
        .BottomMargin = 18

        .Zoom = False

        .FitToPagesWide = 1
        .FitToPagesTall = False

        .PrintGridlines = False

    End With

    '=====================================================
    ' Summary Table
    '=====================================================
    summary_start = Int(3)
	
	rpt.Range( _
		rpt.Cells(1, 3), _
		rpt.Cells(3, 3 + UBound(headers)) _
    ).UnMerge
	
    For c = 0 To UBound(headers)

        '-------------------------------------------------
        ' Header
        '-------------------------------------------------

        rpt.Cells(1, c + summary_start).Value = headers(c)
        rpt.Cells(1, c + summary_start).Font.Bold = True
        rpt.Cells(1, c + summary_start).HorizontalAlignment = xlCenter
        rpt.Cells(1, c + summary_start).Borders.LineStyle = xlContinuous

        '-------------------------------------------------
        ' Values 1
        '-------------------------------------------------

        rpt.Cells(2, c + summary_start).Value = values1(c)
        rpt.Cells(2, c + summary_start).HorizontalAlignment = xlCenter
        rpt.Cells(2, c + summary_start).Borders.LineStyle = xlContinuous

        '-------------------------------------------------
        ' Values 2
        '-------------------------------------------------

        rpt.Cells(3, c + summary_start).Value = values2(c)
        rpt.Cells(3, c + summary_start).HorizontalAlignment = xlCenter
        rpt.Cells(3, c + summary_start).Borders.LineStyle = xlContinuous

    Next c

    '=====================================================
    ' Body Header
    '=====================================================

    rpt.Cells(5, 1).Value = "No."
    rpt.Cells(5, 2).Value = "掃碼機"

    bodyCol = 3

    For c = firstDataCol To lastDataCol

        rpt.Cells(5, bodyCol).Value = _
            CStr(sourceWs.Cells(1, c).Value)

        bodyCol = bodyCol + 1

    Next c

    '=====================================================
    ' Body Data
    ' startRow ~ endRow
    '=====================================================

    bodyRow = 6

    For r = startRow To endRow

        '-------------------------------------------------
        ' No.
        '-------------------------------------------------

        rpt.Cells(bodyRow, 1).Value = _
            r - startRow + 1

        '-------------------------------------------------
        ' 掃碼機
        '-------------------------------------------------

        rpt.Cells(bodyRow, 2).Value = ""

        '-------------------------------------------------
        ' 實際資料
        '-------------------------------------------------

        bodyCol = 3

        For c = firstDataCol To lastDataCol

            rpt.Cells(bodyRow, bodyCol).Value = _
                sourceWs.Cells(r, c).Value

            bodyCol = bodyCol + 1

        Next c

        bodyRow = bodyRow + 1

    Next r

    '=====================================================
    ' Body Border / Alignment
    '=====================================================

    With rpt.Range( _
        rpt.Cells(5, 1), _
        rpt.Cells(bodyRow - 1, bodyCol - 1))

        .Borders.LineStyle = xlContinuous
        .Borders.Weight = xlThin
        .VerticalAlignment = xlCenter
        .WrapText = False

    End With

    '=====================================================
    ' Body Header Style
    '=====================================================

    With rpt.Range( _
        rpt.Cells(5, 3), _
        rpt.Cells(5, bodyCol - 1))

        .Font.Bold = True
        .HorizontalAlignment = xlCenter
        .WrapText = True
		
		' Header 邊框
		.Borders.LineStyle = xlContinuous
		.Borders.Weight = xlThin

    End With

    '=====================================================
    ' Column Width
    '=====================================================
	'序號 掃碼機 寬度設定值
	
    rpt.Columns(1).ColumnWidth = 15
    rpt.Columns(2).ColumnWidth = 25

    For c = 3 To bodyCol - 1

        rpt.Columns(c).ColumnWidth = 15

        Select Case CStr(rpt.Cells(5, c).Value)

            Case "PLCCellID_CE"

                rpt.Columns(c).ColumnWidth = 18

            Case "PLCCellIDClass_CE"

                rpt.Columns(c).ColumnWidth = 26

            Case "PLCTrayID_CE"

                rpt.Columns(c).ColumnWidth = 18

            Case "model_combine_number"

                rpt.Columns(c).ColumnWidth = 27
				
			Case "last_define_location"

                rpt.Columns(c).ColumnWidth = 24

            Case "parallel_match"

                rpt.Columns(c).ColumnWidth = 15

        End Select

    Next c

    '=====================================================
    ' Row Height
    '=====================================================

    rpt.Rows(1).RowHeight = 28
    rpt.Rows(2).RowHeight = 32
    rpt.Rows(3).RowHeight = 25
    rpt.Rows(5).RowHeight = 41
	
	'=====================================================
	' Cell Alignment
    '=====================================================
	With rpt.Range( _
	       rpt.Cells(1,1), _
		   rpt.Cells(bodyRow - 1, bodyCol - 1) _
         )

		'水平置中
		.HorizontalAlignment = xlCenter

		'垂直置中
		.VerticalAlignment = xlCenter

    End With 

    '=====================================================
    ' Print Area
    '=====================================================
	
    rpt.PageSetup.PrintArea = _
        rpt.Range( _
            rpt.Cells(1, 1), _
            rpt.Cells(bodyRow - 1, bodyCol - 1) _
        ).Address

End Sub


Private Sub GeneratePDF( _
    ByVal ws As Worksheet, _
    ByVal startRow As Long, _
    ByVal endRow As Long, _
    ByVal classID As String, _
    ByVal pdfNo As Long)

    Dim pdfPath As String
    Dim pdfFileName As String
    Dim exportRange As Range

    '=====================================================
    ' PDF 儲存位置
    '=====================================================

    pdfPath = ThisWorkbook.Path & "\PDF_NoCoutinue\"

    ' 如果資料夾不存在，建立資料夾
    If Dir(pdfPath, vbDirectory) = "" Then
        MkDir pdfPath
    End If
	
	
	 '=====================================================
    ' PDF 檔名
    '=====================================================

    pdfFileName = _
        pdfPath & _
        classID & "_" & _
        "PDF_" & _
        Format(pdfNo, "00") & _
        ".pdf"
  
    '=====================================================
    ' 指定要輸出的資料範圍
    '
    ' 假設：
    ' A欄 到 最後一欄
    '
    ' 如果你的資料不是 A:Z，
    ' 這裡要依你的實際欄位修改
    '=====================================================

    Set exportRange = ws.Range( _
        ws.Cells(startRow, 1), _
        ws.Cells(endRow, ws.UsedRange.Columns.Count))


    '=====================================================
    ' 設定列印範圍
    '=====================================================

    With ws.PageSetup

        .PrintArea = exportRange.Address

        .Orientation = xlPortrait

        .Zoom = False

        .FitToPagesWide = 1

        .FitToPagesTall = False

    End With
	
	 '=====================================================
    ' 產生 PDF
    '=====================================================

    ws.ExportAsFixedFormat _
        Type:=xlTypePDF, _
        Filename:=pdfFileName, _
        Quality:=xlQualityStandard, _
        IncludeDocProperties:=True, _
        IgnorePrintAreas:=False, _
        OpenAfterPublish:=False


    Debug.Print "PDF 已產生："
    Debug.Print pdfFileName
	
End Sub

'=====================================================
' 更新 PDF UserForm 進度
'=====================================================
Public Sub UpdatePDFProgress( _
    ByVal progressPercent As Long, _
    ByVal statusText As String)

    '=================================================
    ' 限制百分比範圍
    '=================================================
    If progressPercent < 0 Then
        progressPercent = 0
    End If

    If progressPercent > 100 Then
        progressPercent = 100
    End If


    '=================================================
    ' 更新 UserForm 狀態
    '=================================================
    frmPDFProgress.lblStatus.Caption = statusText

    frmPDFProgress.lblPercent.Caption = _
        CStr(progressPercent) & "%"


    '=================================================
    ' 更新進度條
    '=================================================
    frmPDFProgress.lblProgress.Width = _
        frmPDFProgress.fraProgress.InsideWidth * _
        progressPercent / 100


    '=================================================
    ' 同步更新 Excel StatusBar
    '=================================================
    Application.StatusBar = _
        statusText & _
        " (" & CStr(progressPercent) & "%)"


    '=================================================
    ' 強制刷新 UserForm
    '=================================================
    DoEvents

End Sub

    Private Function SelectXLSFile( _
	        ByRef isContinue_ClassId As Boolean) As String
			
            Dim fd As FileDialog
            Dim selectedFile As String
			Dim fileName As String
			            
            isContinue_ClassId = False
			
            'Set fd = Application.FileDialog(3)
            Set fd = Application.FileDialog( _
			             msoFileDialogFilePicker)

            With fd
                 .Title = "請選擇 xlsx 檔案"
                 .AllowMultiSelect = False

                 .Filters.Clear
                 .Filters.Add "XLSX Files", "*.xlsx"
            
			    '=================================================
                ' 使用者取消
                '=================================================
			
                If .Show <> -1 Then
				
				   Set fd = Nothing				   
                   
				   Exit Function			   
				   
                End If
				
				'=================================================
                ' 取得檔案完整路徑
                '=================================================
                
				selectedFile = .SelectedItems(1)
				
            End With

            Set fd = Nothing
			
			fileName = Dir(selectedFile)
			
			'=====================================================
			' 判斷 ClassID 模式
			'
			' 注意：
			' 「非連續」包含「連續」
			' 所以一定要先判斷「非連續」
			'=====================================================

			If InStr(1, fileName, "非連續", vbTextCompare) > 0 Then

				'=================================================
				' 非連續
				'
				' 每個 ClassID 重新從 Location 1 開始
				'=================================================

				isContinue_ClassId = False


			ElseIf InStr(1, fileName, "連續", vbTextCompare) > 0 Then

				'=================================================
				' 連續
				'
				' ClassID 切換後 Location 接續
				'=================================================

				isContinue_ClassId = True

			Else
				
              '=================================================
			  ' 檔名沒有指定模式
			  '=================================================

			   MsgBox _
					"檔案名稱無法判斷 ClassID 模式。" & vbCrLf & vbCrLf & _
					"檔名必須包含：" & vbCrLf & _
					"【連續】 或 【非連續】" & vbCrLf & vbCrLf & _
					"目前檔案：" & fileName, _
					vbExclamation, _
					"檔名格式錯誤"

			   SelectXLSFile = ""

			   Exit Function

            End If
			
	   '=====================================================
       ' 回傳檔案路徑
       '=====================================================

       SelectXLSFile = selectedFile
		
    End Function
	

    Private Function NumericCellValue(ByVal rawValue As Variant) As Double
            Dim s As String

            If IsError(rawValue) Then Exit Function
            
            s = Trim(CStr(rawValue))
            
            If s = "" Then Exit Function
            
            s = Replace(s, "'", "")
            s = Trim(s)
            
            If s <> "" And IsNumeric(s) Then
                NumericCellValue = CDbl(s)
            End If
                
    End Function


    Private Function FormatStat(ByVal v As Variant, ByVal num As Integer) As String
            
			If IsError(v) Or IsEmpty(v) Or IsNull(v) Then
            
                FormatStat = ""
				Exit Function
				
            End If
				
                
            If IsNumeric(v) Then

                FormatStat = FormatNumber( _
                                CDbl(v), _
                                num, _
                                vbTrue, _
                                vbFalse, _
                                False)
            
            Else
            
                FormatStat = ""
                
            End If
            
    End Function


    Private Function FormatRange( _
        ByVal minV As Variant, _
        ByVal maxV As Variant) As String
        
        If IsEmpty(minV) Or IsEmpty(maxV) Then
        
            FormatRange = ""
            
        ElseIf minV = "" Or maxV = "" Then

            FormatRange = ""

        ElseIf Not IsNumeric(minV) Or Not IsNumeric(maxV) Then

            FormatRange = ""
            
        Else
        
            FormatRange = _
                 FormatNumber(CDbl(minV), 5, vbTrue, vbFalse, False) & _
                 " ~ " & _
                 FormatNumber(CDbl(maxV), 5, vbTrue, vbFalse, False)
        
        End If
        
    End Function


    Private Function FormatDateTimeNow() As String

            Dim d As Date

            d = Now
            
            FormatDateTimeNow = _
                     Year(d) & "-" & _
                     Right("0" & Month(d), 2) & "-" & _
                     Right("0" & Day(d), 2) & " " & _
                     Right("0" & Hour(d), 2) & ":" & _
                     Right("0" & Minute(d), 2)

    End Function


    Private Function CleanFileName(ByVal s As String) As String

            Dim a As Variant
            Dim i As Long
            
            a = Array("\", "/", ":", "*", "?", """", "<", ">", "|")
            
            CleanFileName = Trim(s)
            
            For i = 0 To UBound(a)
            
                CleanFileName = _
                       Replace(CleanFileName, a(i), "_")
                
            Next i
            
            If CleanFileName = "" Then
                   CleanFileName = "Model"
            End If
            
    End Function
        
        
    '只取當前字串(包含'a~z' 或 'A~Z')的字元結合
    Private Function GetEnglishLetters(ByVal inputValue As Variant) As String

        Dim i As Long
        Dim ch As String
        Dim result As String

        result = ""

        For i = 1 To Len(CStr(inputValue))

            ch = Mid(CStr(inputValue), i, 1)

            If (ch >= "A" And ch <= "Z") Or _
               (ch >= "a" And ch <= "z") Then

                result = result & ch

            End If

        Next i

        GetEnglishLetters = result

    End Function

    '不復蓋原先PDF檔案
     Private Function GetUniqueFileName(ByVal fullPath As String) As String

        Dim fso As Object
        Dim folder As String
        Dim base As String
        Dim ext As String
        Dim n As Long
        Dim candidate As String
        
        Set fso = CreateObject("Scripting.FileSystemObject")
        
        '-----------------------------------------------------
        ' 原始檔案不存在，直接使用原始路徑
        '-----------------------------------------------------
        
        If Not fso.FileExists(fullPath) Then
        
            GetUniqueFileName = fullPath
            
            Set fso = Nothing

            Exit Function
            
        End If

        '-----------------------------------------------------
        ' 取得資料夾/檔名/副檔名
        '-----------------------------------------------------
        folder = fso.GetParentFolderName(fullPath)
            
        base = fso.GetBaseName(fullPath)
        
        ext = fso.GetExtensionName(fullPath)
        
        n = 1


        '-----------------------------------------------------
        ' 持續尋找沒有使用過的檔名
        '-----------------------------------------------------
        
        Do
            candidate = _
                folder & "\" & _
                base & "_" & _
                CStr(n) & "." & _
                ext

            n = n + 1
            
        Loop While fso.FileExists(candidate)

        '-----------------------------------------------------
        ' 回傳唯一檔名
        '-----------------------------------------------------
        
        GetUniqueFileName = candidate
        
        Set fso = Nothing
        
    End Function
        
        
    '刪除指定路徑之所有PDF檔案,確保每次都是做最後產出的結果
    Private Function ClearPdfAndGetFileName(ByVal fullPath As Variant) As String
             
             Dim fsoLocal As Object
             Dim folder As Object
             Dim file As Object
             
             '=====================================================
             ' 預設回傳原始資料夾路徑
             '=====================================================
             ClearPdfAndGetFileName = fullPath
             
             '=====================================================
             ' 建立 FileSystemObject
             '=====================================================
             Set fsoLocal = CreateObject("Scripting.FileSystemObject")


             '=====================================================
             ' 確認資料夾存在
             '=====================================================
             If Not fsoLocal.FolderExists(fullPath) Then
                Set fsoLocal = Nothing
                Exit Function
             End If
             
             
             '=====================================================
             ' 取得指定資料夾
             '=====================================================
             Set folder = fsoLocal.GetFolder(fullPath)
 
             ' 清除該資料夾內所有 PDF
             For Each file In folder.Files

                If LCase(fsoLocal.GetExtensionName(file.Name)) = "pdf" Then
                
                    On Error Resume Next
                    
                    file.Delete True
                    
                    On Error GoTo 0
                    
                End If

             Next file

            
            
            '=====================================================
            ' 釋放 Object
            '=====================================================
            Set file = Nothing
            Set folder = Nothing
            Set fsoLocal = Nothing
            ' 清除後直接使用原始檔名
            'ClearPdfAndGetFileName = fullPath

    End Function
	
	Private Function NormalizeHeader( ByVal value As Variant) As String

		Dim s As String

		If IsError(value) Then
			NormalizeHeader = ""
			Exit Function
		End If

		s = CStr(value)

		' BOM
		s = Replace(s, ChrW(&HFEFF), "")

		' 不換行空白
		s = Replace(s, ChrW(160), "")

		' 全形空白
		s = Replace(s, ChrW(12288), "")

		' Tab
		s = Replace(s, vbTab, "")

		' CR / LF
		s = Replace(s, vbCr, "")
		s = Replace(s, vbLf, "")

		' 一般前後空白
		s = Trim(s)

		NormalizeHeader = s

    End Function
	
	
	'連續classID group 分配	
	Private Function FindContinuous32Group( _
		ByVal ws As Worksheet, _
		ByVal searchStartRow As Long, _
		ByVal lastRow As Long, _
		ByVal classCol As Long, _
		ByVal locationCol As Long, _
		ByRef groupStartRow As Long, _
		ByRef groupEndRow As Long, _
		ByRef isComplete32 As Boolean) As Boolean
    
	
	    Dim r As Long

        Dim locationValue As Variant
        Dim locationCount As Long

        Dim firstRow As Long
        Dim lastLocationRow As Long
		
		Dim progressPercent As Long
	
	
	    '初始化
		FindContinuous32Group = False
		
		
	    groupStartRow = 0
        groupEndRow = 0
        isComplete32 = False
		
		
	    r = searchStartRow
		
		Do While r <= lastRow		
		
		    
		    If r Mod 50 = 0 Then
			    
			    progressPercent = Int( _
                       ((r - 1) / (lastRow - 1)) * 100)
			
			Else
			   
			    progressPercent = 0
				
				Call UpdatePDFProgress( _
						progressPercent, _
						"尋找連續 ClassID Group..." & _
						vbCrLf & _
						"掃描：" & _
						CStr(r - 1) & " / " & _
						CStr(lastRow - 1))
							
			End If
		
			If Trim(CStr(ws.Cells(r, classCol).Value)) <> "" Then
				Exit Do
			End If

			r = r + 1

        Loop

		'搜尋長度超過最後一筆
		If r > lastRow Then
		
			Exit Function
			
		End If
	
	    firstRow = r
		
		'=====================================================
        ' 開始尋找 Location 16,16
        '=====================================================
        Do While r <= lastRow
		
		   locationValue = NumericCellValue( _
                            ws.Cells(r, locationCol).Value)
							
		   '-----------------------------------------
           ' 確認目前這筆是不是連續兩筆 Location 16
           '-----------------------------------------
		     
		   If IsNumeric(locationValue) Then

             If CDbl(locationValue) >= 1 And _
                CDbl(locationValue) <= 16 Then

                locationCount = locationCount + 1

             End If
								
           End If
		   
		   If IsNumeric(locationValue) Then

              If CDbl(locationValue) = 16 Then
		   
		        '-----------------------------------------
                ' 確認目前這筆是不是連續兩筆 Location 16, 需要鑑定r 當下為 最後一筆
                '-----------------------------------------
		          If r > firstRow Then
				      
					 '倒數第2筆
				     Dim previousLocation As Variant
					 
					 previousLocation = NumericCellValue( _
                        ws.Cells(r - 1, locationCol).Value)
					 
				     If IsNumeric(previousLocation) Then

                        If CDbl(previousLocation) = 16 Then
						
						    lastLocationRow = r

                            '---------------------------------
                            ' 找到 16,16
                            '---------------------------------

							If lastRow > 1 Then

								progressPercent = Int( _
									((r - 1) / (lastRow - 1)) * 100)

							Else

								progressPercent = 0

							End If

                            
							
							Call UpdatePDFProgress( _
								progressPercent, _
								"找到 Location 16,16" & _
								vbCrLf & _
								"資料：" & _
								CStr(firstRow - 1) & " ~ " & _
								CStr(r - 1))
							

                            Exit Do

                        End If

                     End If
		
		        End If

             End If

           End If
		   
		   r = r + 1
				
		 Loop
		
		 
		 '=====================================================
		 ' 找不到完整的 16,16
		 '=====================================================

		 If lastLocationRow = 0 Then

			groupStartRow = firstRow
			groupEndRow = lastRow

			isComplete32 = False

			FindContinuous32Group = True

			Exit Function

		 End If
		 
		 
		 '=====================================================
		 ' 設定 Group 範圍 ,透過上述走訪確認(31:  16 ,  32: 16)
         '=====================================================

		 groupStartRow = firstRow
		 groupEndRow = lastLocationRow
		 
		 
		 '=====================================================
         ' 必須剛好 32 顆 , 第1筆 Location = 1, 最後 Location = 16		 
         '=====================================================

		If groupEndRow - groupStartRow + 1 = 32 Then
		
		    Dim firstLocation As Variant

            '確認頭為 1 開始
            firstLocation = NumericCellValue( _
                        ws.Cells(groupStartRow, locationCol).Value)
						
						

			If IsNumeric(firstLocation) Then

				If CDbl(firstLocation) = 1 Then

					isComplete32 = True

				End If

			End If

		Else

			isComplete32 = False

		End If


		FindContinuous32Group = True

	End Function
        
		
	
	'每個 ClassID 自己必須是完整 32 顆 ,反之則忽略不執行PDF產生
	Private Function FindSingleClass32Group( _
		ByVal ws As Worksheet, _
		ByVal searchStartRow As Long, _
		ByVal lastRow As Long, _
		ByVal classCol As Long, _
		ByVal locationCol As Long, _
		ByRef groupStartRow As Long, _
		ByRef groupEndRow As Long, _
		ByRef classID As String, _
		ByRef isComplete32 As Boolean) As Boolean
		
		
        Dim r As Long
		
		Dim firstRow As Long
		Dim lastClassRow As Long

		Dim currentClass As String
		Dim firstClass As String
		
		Dim locationValue As Variant
        Dim expectedLocation As Long
		
		Dim classCount As Long
		
		Dim remainingCount As Long
		Dim currentStartRow As Long		  
        Dim currentEndRow As Long		    
        Dim currentCount As Long
		
		Dim pdfCount As Long
		Dim icase As Long
		Dim remainCount As Long
		
		Dim location31 As Variant
		Dim location32 As Variant

		Dim progressPercent As Long

        '=====================================================
		' 初始化
		'=====================================================

		FindSingleClass32Group = False

		groupStartRow = 0
		groupEndRow = 0
		
		classID = ""
		isComplete32 = False
		
		
		
		'=====================================================
		' 找下一個有效 ClassID
		'=====================================================
		
		r = searchStartRow

        Do While r <= lastRow
		
		    '-------------------------------------------------
			' 更新搜尋進度
			' 每 50 筆更新一次
			'-------------------------------------------------
			If r Mod 50 = 0 Then

				If lastRow > 1 Then

					progressPercent = Int( _
						((r - 1) / (lastRow - 1)) * 100)

					If progressPercent > 100 Then
						progressPercent = 100
					End If

				Else

					progressPercent = 0

				End If


				Call UpdatePDFProgress( _
					progressPercent, _
					"尋找下一個 ClassID..." & _
					vbCrLf & _
					"掃描：" & _
					CStr(r - 1) & " / " & _
					CStr(lastRow - 1))

			End If
		
		    currentClass = Trim(CStr( _
		         ws.Cells(r, classCol).Value))
		   
		    If currentClass <> "" Then
		   
               Exit Do
			   
            End If

           r = r + 1
		
		Loop
		
		
		'=====================================================
        ' 搜尋超過最後一筆
        '=====================================================
		
        If r > lastRow Then
		
			Exit Function
			
		End If
		
		
		'=====================================================
		' 記錄 Class 起始列
		'=====================================================

		firstRow = r
		
		firstClass = Trim(CStr( _
            ws.Cells(r, classCol).Value))
			
		classID = currentClass
		
		currentStartRow = firstRow
		
		
		'=====================================================
		' 尋找這個 ClassID 的最後一列 ,不包含其它ClassID
		'=====================================================

		Do While r <= lastRow
		
		    '-------------------------------------------------
			' 更新目前 ClassID 掃描進度
			' 每 50 筆更新一次
			'-------------------------------------------------
			If r Mod 50 = 0 Then

				If lastRow > 1 Then

					progressPercent = Int( _
						((r - 1) / (lastRow - 1)) * 100)

					If progressPercent > 100 Then
						progressPercent = 100
					End If

				Else

					progressPercent = 0

				End If


				Call UpdatePDFProgress( _
					progressPercent, _
					"分析 ClassID：" & classID & _
					vbCrLf & _
					"尋找 ClassID 最後一列..." & _
					vbCrLf & _
					"掃描：" & _
					CStr(r - 1) & " / " & _
					CStr(lastRow - 1))

			End If

			currentClass = Trim(CStr( _
			    ws.Cells(r, classCol).Value))

			If currentClass <> firstClass Then
			
				Exit Do
				
			End If

			r = r + 1

		Loop

		'取最後此group 的列位置
		lastClassRow = r - 1
		
		'groupStartRow = firstRow
       ' groupEndRow = lastClassRow
		
		
		'=====================================================
        ' 計算這個 Class 有幾顆
        '=====================================================

        classCount = _
		    lastClassRow - firstRow + 1			       

	    '=====================================================
		' 計算可以產生幾份32顆 PDF
		'=====================================================

		pdfCount = classCount \ 32

		remainCount = classCount Mod 32

		Debug.Print "=========================================="
        Debug.Print "ClassID    = [" & classID & "]"
        Debug.Print "ClassStart = " & firstRow
        Debug.Print "ClassEnd   = " & lastClassRow
        Debug.Print "ClassCount = " & classCount
        Debug.Print "=========================================="
	
		'=====================================================
		' 每32顆產生一份 PDF
		'=====================================================
	    
	'	For icase = 0 To pdfCount - 1
	'	      currentStartRow = _
	'		        groupStartRow + ( icase * 32)
	'			 
	'		  currentEndRow = _
	'				currentStartRow + 31
	'				
	'		  currentCount = _
    '                currentEndRow - currentStartRow + 1
	'				
	'		  Debug.Print "------------------------------"
	'		  Debug.Print "PDF No.      = " & (i + 1)
	'		  Debug.Print "PDF StartRow = " & currentStartRow
	'		  Debug.Print "PDF EndRow   = " & currentEndRow
	'		  Debug.Print "CurrentCount = " & currentCount
	'
    '          '=================================================
	'		  ' 確認這一批一定是32顆
	'		  '=================================================
	'
	'		   If currentCount = 32 Then
	'
	'				Call GeneratePDF( _
	'					currentStartRow, _
	'					currentEndRow)
	'
	'		   Else
	'
	'				Debug.Print _
	'					"錯誤：目前PDF區段不是32顆，不產生PDF"
	'
	'		   End If
	'			
	'	 Next icase
	     
		If classCount < 32 Then

			Debug.Print _
				"ClassID [" & classID & _
				"] 只有 " & classCount & _
				" 顆，不足32顆，忽略"

			groupStartRow = firstRow
			groupEndRow = lastClassRow

			isComplete32 = False
			FindSingleClass32Group = True
			
			
			Call UpdatePDFProgress( _
				progressPercent, _
				"ClassID：" & classID & _
				" 不足 32 顆，忽略" & _
				vbCrLf & _
				"數量：" & CStr(classCount))

			Exit Function

		End If
	
	    
		 
		 
		'=====================================================
		' 剩餘不足32顆的資料
		' 不產生PDF
		'=====================================================

		'If remainCount > 0 Then

		'	Debug.Print _
		'		"ClassID = [" & classID & _
		'		"] 剩餘 " & remainCount & _
		'		" 顆，不足32顆，不產生PDF"

		'End If
		
		
		'currentStartRow = searchStartRow
		
		'=====================================================
		' 確認 currentStartRow 仍然屬於這個 ClassID
		'=====================================================

		If Trim(CStr(ws.Cells( _
			currentStartRow, classCol).Value)) <> firstClass Then

			Debug.Print _
				"searchStartRow 已不屬於目前 ClassID"

			Exit Function

		End If
		
		
		'=====================================================
		' 計算從目前位置開始還剩幾顆
		'=====================================================

		remainingCount = _
			lastClassRow - currentStartRow + 1


		Debug.Print _
			"CurrentStartRow = " & currentStartRow

		Debug.Print _
			"RemainingCount = " & remainingCount
		
		
		 If remainingCount < 32 Then

			Debug.Print _
				"ClassID [" & classID & _
				"] 剩餘 " & remainingCount & _
				" 顆，不足32顆，不產生PDF"

			groupStartRow = currentStartRow
			groupEndRow = lastClassRow

			isComplete32 = False
			FindSingleClass32Group = True
			
			Call UpdatePDFProgress( _
				progressPercent, _
				"ClassID：" & classID & _
				" 剩餘不足 32 顆" & _
				vbCrLf & _
				"剩餘：" & CStr(remainingCount) & " 顆")

			Exit Function

		End If
		
		
		'=====================================================
		' 本次只取32顆
		'=====================================================

		currentEndRow = _
			currentStartRow + 31


		'=====================================================
		' 防止超過此 ClassID 的最後一列
		'=====================================================

		If currentEndRow > lastClassRow Then

			currentEndRow = lastClassRow

		End If
		
		
		'=====================================================
		' 計算本次實際數量
		'=====================================================

		currentCount = _
			currentEndRow - currentStartRow + 1


		Debug.Print "------------------------------"
		Debug.Print "Current ClassID = [" & classID & "]"
		Debug.Print "Group StartRow  = " & currentStartRow
		Debug.Print "Group EndRow    = " & currentEndRow
		Debug.Print "CurrentCount    = " & currentCount
		
		
		
		Call UpdatePDFProgress( _
			progressPercent, _
			"驗證 ClassID：" & classID & _
			vbCrLf & _
			"目前區段：" & _
			CStr(currentStartRow) & _
			" ~ " & _
			CStr(currentEndRow))
		
		
		'=====================================================
		' 必須完整32顆
		'=====================================================

		If currentCount <> 32 Then

			Debug.Print _
				"錯誤：目前區段不是32顆"

			groupStartRow = currentStartRow
			groupEndRow = currentEndRow

			isComplete32 = False
			FindSingleClass32Group = True

			Exit Function

		End If
		
		'=====================================================
		' 驗證 Location
		'
		' 必須：
		'
		' 1,1
		' 2,2
		' ...
		' 16,16
		'
		' 每個 Location 兩顆
		'=====================================================

		For r = currentStartRow To currentEndRow
				
			'UpdatePDFProgress 裡面有 DoEvents ,常態性更新會有元件耗損機率,故不需要每筆都跑狀態			
			'-------------------------------------------------
			' 更新 32 顆內部驗證進度
			'-------------------------------------------------
		    If ((r - currentStartRow) Mod 6 = 0) Or _
                       r = currentEndRow Then

				progressPercent = Int( _
					((r - currentStartRow + 1) / currentCount) * 100)


				Call UpdatePDFProgress( _
					progressPercent, _
					"驗證 ClassID：" & classID & _
					vbCrLf & _
					"Location：" & _
					CStr(r - currentStartRow + 1) & _
					" / 32")
            
			End If


			locationValue = NumericCellValue( _
				ws.Cells(r, locationCol).Value)


            '-----------------------------------------------
            ' 每32顆重新從 Location 1 開始
            '-----------------------------------------------
			
			expectedLocation = _
				Int(((r - currentStartRow) + 2) / 2)


			Debug.Print _
				"Row=" & r & _
				" Class=[" & _
				Trim(CStr(ws.Cells(r, classCol).Value)) & _
				"] Location=[" & _
				CStr(locationValue) & _
				"] Expected=[" & _
				expectedLocation & "]"


			'-----------------------------------------------
			' Location 必須是數字
			'-----------------------------------------------

			If Not IsNumeric(locationValue) Then

				Debug.Print _
					"錯誤：Location 不是數字，Row=" & r
					
				groupStartRow = currentStartRow
                groupEndRow = currentEndRow

				isComplete32 = False

				FindSingleClass32Group = True

				Exit Function

			End If


			'-----------------------------------------------
			' Location 必須符合預期
			'-----------------------------------------------

			If CDbl(locationValue) <> expectedLocation Then

				Debug.Print _
					"錯誤：Location 不符合預期，Row=" & r
					
					
				groupStartRow = currentStartRow
                groupEndRow = currentEndRow

				isComplete32 = False

				FindSingleClass32Group = True

				Exit Function

			End If


		Next r
		
		
	    '=====================================================
		' 檢查最後兩顆 Location
		'
		' 第31顆 = 16
		' 第32顆 = 16
		'=====================================================

		location31 = NumericCellValue( _
						ws.Cells(currentEndRow - 1, locationCol).Value)

		location32 = NumericCellValue( _
						ws.Cells(currentEndRow, locationCol).Value)


		Debug.Print _
			"Last-1 Location = [" & _
			CStr(location31) & "]"

		Debug.Print _
			"Last Location   = [" & _
			CStr(location32) & "]"

		If IsNumeric(location31) And _
		   IsNumeric(location32) Then

			If CDbl(location31) = 16 And _
			   CDbl(location32) = 16 Then
			   
			   '=============================================
               ' 完整 32 顆
               '=============================================
			    groupStartRow = currentStartRow
                groupEndRow = currentEndRow 
			   

				isComplete32 = True
				
				FindSingleClass32Group = True
				
				Call UpdatePDFProgress( _
					100, _
					"ClassID：" & classID & _
					" 驗證完成" & _
					vbCrLf & _
					"完整 32 顆")
				
				Debug.Print "=========================================="
				Debug.Print "成功找到完整32顆"
				Debug.Print "ClassID   = [" & classID & "]"
				Debug.Print "StartRow  = " & groupStartRow
				Debug.Print "EndRow    = " & groupEndRow
				Debug.Print "=========================================="

			Else

				Debug.Print _
					"錯誤：最後兩顆不是 16,16"
					
				groupStartRow = currentStartRow
                groupEndRow = currentEndRow

				isComplete32 = False
				
				FindSingleClass32Group = True

            End If

		End If


		 
				    
    End Function
	
	
	
