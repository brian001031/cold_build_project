        Option Explicit

		Const xlTypePDF = 0
		Const xlQualityStandard = 0
		Const xlLandscape = 2
		Const xlCenter = -4108
		Const xlContinuous = 1
		Const xlThin = 2

		Dim fso, csvPath, outputFolder
		Dim currentDateFolder
		Dim errCode
        Dim errDesc
		Dim xlApp, wb, ws, reportWs
		Dim headerMap
		Dim lastRow
		Dim lastCol
		Dim startRow, endRow, groupNo, groupCount
		Dim pdfPath, modelName
		Dim vashcCol, mohmCol, kCol, voltageCol, modelCol
		Dim firstDataCol, lastDataCol , type_classsCol
		Dim r, c
		Dim vashcMin, vashcMax, vashcAvg, vashcSum, vashcCount
		Dim mohmMin, mohmMax, kMin, kMax, voltageMin, voltageMax , typeclass_char
		Dim val, firstModel, pdfCount
		Dim currentDateTime
		
		typeclass_char = Empty

		Set fso = CreateObject("Scripting.FileSystemObject")

		csvPath = SelectCSVFile()
		
		If csvPath = "" Then WScript.Quit

		outputFolder = fso.GetParentFolderName(csvPath) & "\PDF_PACKAGE"
		
		If Not fso.FolderExists(outputFolder) Then fso.CreateFolder outputFolder
		
		' 建立當前日期資料夾
		currentDateFolder = outputFolder & "\" &  _
					 Year(Date) & "-" & _
					 Right("0" & Month(Date), 2) & "-" & _
					 Right("0" & Day(Date), 2)
					
	    If Not fso.FolderExists(currentDateFolder) Then fso.CreateFolder currentDateFolder

		' 最後輸出資料夾
		outputFolder = currentDateFolder
		
		'先清除內部所有組裝pdf檔案
		outputFolder = ClearPdfAndGetFileName(outputFolder)
		
		'取得當前日期時間
		'currentDateTime = Format(Now, "yyyy/m/d H:mm")
		
		currentDateTime = Year(Now) & "/" & _
                  Month(Now) & "/" & _
                  Day(Now) & " " & _
                  Right("0" & Hour(Now), 2) & ":" & _
                  Right("0" & Minute(Now), 2)

		Set xlApp = CreateObject("Excel.Application")
		xlApp.Visible = False
		xlApp.DisplayAlerts = False
		xlApp.ScreenUpdating = False

		On Error Resume Next
		
		Set wb = xlApp.Workbooks.Open(csvPath, False, True)
		
		errCode = Err.Number
        errDesc = Err.Description

        Err.Clear
			
		If errCode <> 0 Then
			WScript.Echo "CsV OPEN ERROR"
			WScript.Echo errDesc
			xlApp.Quit
			WScript.Quit
		End If 
		

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
		
		

		' 最後一筆資料的 Row
		lastRow = ws.Cells.Find("*", ws.Cells(1, 1),xlFormulas, xlPart , _
				   xlByRows, _SearchDirection:=xlPrevious, False)

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

	    

		Set headerMap = CreateObject("Scripting.Dictionary")
		BuildHeaderMap ws, headerMap, lastCol

		RequireHeader headerMap, "PLCCellID_CE"
		RequireHeader headerMap, "parallel_match"
		RequireHeader headerMap, "3.5-2.8V_mAH"
		RequireHeader headerMap, "acirRP12_CE"
		RequireHeader headerMap, "K_Value"
		RequireHeader headerMap, "acirVP12_CE"
		RequireHeader headerMap, "model_combine_number"
		'32分選號
		RequireHeader headerMap, "PLCCellIDClass_CE"
		

		firstDataCol = headerMap("PLCCellID_CE")
		lastDataCol = headerMap("parallel_match")
		type_classsCol = headerMap("PLCCellIDClass_CE")

		If firstDataCol > lastDataCol Then
			WScript.Echo "錯誤：PLCCellID_CE 必須位於 parallel_match 左側。"
			wb.Close False
			xlApp.Quit
			WScript.Quit
		End If

		vashcCol = headerMap("3.5-2.8V_mAH")
		mohmCol = headerMap("acirRP12_CE")
		kCol = headerMap("K_Value")
		voltageCol = headerMap("acirVP12_CE")
		modelCol = headerMap("model_combine_number")

		startRow = 2
		groupNo = 0
		pdfCount = 0

		Do While startRow <= lastRow

			groupNo = groupNo + 1
			endRow = startRow + 31
			'當前計算到最後行數超出事先計算,直接賦予
			If endRow > lastRow Then endRow = lastRow
								
            ' 取得此 32 筆群組第一筆的 Class Type
            typeclass_char = GetEnglishLetters(ws.Cells(startRow, type_classsCol).Value)
			
			groupCount = endRow - startRow + 1

			vashcMin = Empty : vashcMax = Empty
			vashcSum = 0 : vashcCount = 0
			mohmMin = Empty : mohmMax = Empty
			kMin = Empty : kMax = Empty
			voltageMin = Empty : voltageMax = Empty 
			

			For r = startRow To endRow			
				val = NumericCellValue(ws.Cells(r, vashcCol).Value)
				If val <> "" Then
					If vashcCount = 0 Then
						vashcMin = CDbl(val)
						vashcMax = CDbl(val)
					Else
						If CDbl(val) < CDbl(vashcMin) Then vashcMin = CDbl(val)
						If CDbl(val) > CDbl(vashcMax) Then vashcMax = CDbl(val)
					End If
					vashcSum = vashcSum + CDbl(val)
					vashcCount = vashcCount + 1
				End If

				val = NumericCellValue(ws.Cells(r, mohmCol).Value)
				If val <> "" Then
					If IsEmpty(mohmMin) Then
						mohmMin = CDbl(val) : mohmMax = CDbl(val)
					Else
						If CDbl(val) < CDbl(mohmMin) Then mohmMin = CDbl(val)
						If CDbl(val) > CDbl(mohmMax) Then mohmMax = CDbl(val)
					End If
				End If

				val = NumericCellValue(ws.Cells(r, kCol).Value)
				If val <> "" Then
					If IsEmpty(kMin) Then
						kMin = CDbl(val) : kMax = CDbl(val)
					Else
						If CDbl(val) < CDbl(kMin) Then kMin = CDbl(val)
						If CDbl(val) > CDbl(kMax) Then kMax = CDbl(val)
					End If
				End If

				val = NumericCellValue(ws.Cells(r, voltageCol).Value)
				If val <> "" Then
					If IsEmpty(voltageMin) Then
						voltageMin = CDbl(val) : voltageMax = CDbl(val)
					Else
						If CDbl(val) < CDbl(voltageMin) Then voltageMin = CDbl(val)
						If CDbl(val) > CDbl(voltageMax) Then voltageMax = CDbl(val)
					End If
				End If

			Next

			If vashcCount > 0 Then
				vashcAvg = CDbl(vashcSum / vashcCount) / 1000
			Else
				vashcAvg = Empty
			End If

			firstModel = Trim(CStr(ws.Cells(startRow, modelCol).Value))
			If firstModel = "" Then firstModel = "Model"
			modelName = CleanFileName(firstModel)

			Set reportWs = wb.Worksheets.Add
			reportWs.Name = "PDF_" & Right("000" & CStr(groupNo), 3)

			BuildReport reportWs, ws, startRow, endRow, firstDataCol, lastDataCol, _
						vashcMin, vashcMax, vashcAvg, _
						mohmMin, mohmMax, kMin, kMax, _
						voltageMin, voltageMax, firstModel , _
						currentDateTime , typeclass_char

			pdfPath = outputFolder & "\" & modelName & "_" & Right("000" & CStr(groupNo), 3) & ".pdf"
							
			'pdfPath = GetUniqueFileName(pdfPath)

			reportWs.ExportAsFixedFormat xlTypePDF, pdfPath, xlQualityStandard, True, False

			reportWs.Delete
			Set reportWs = Nothing

			pdfCount = pdfCount + 1
			startRow = endRow + 1

		Loop

		wb.Close False
		xlApp.Quit

		WScript.Echo "PDF 產生完成。" & vbCrLf & _
					 "Group 數量：" & pdfCount & vbCrLf & _
					 "輸出資料夾：" & outputFolder


		Function SelectCSVFile()
			Dim app, fd
			SelectCSVFile = ""
			Set app = CreateObject("Excel.Application")
			app.Visible = False
			On Error Resume Next
			Set fd = app.FileDialog(3)
			If Err.Number = 0 Then
				fd.Title = "請選擇 CSV 檔案"
				fd.AllowMultiSelect = False
				fd.Filters.Clear
				fd.Filters.Add "CSV Files", "*.csv"
				If fd.Show = -1 Then 
				   SelectCSVFile = fd.SelectedItems(1)
				End If
			On Error GoTo 0
			app.Quit
			Set fd = Nothing
			Set app = Nothing
		End Function


		Sub BuildHeaderMap(ByRef targetWs, ByRef dict, ByVal colCount)
			Dim c, h
			For c = 1 To colCount
				h = Trim(CStr(targetWs.Cells(1, c).Value))
				If h <> "" Then
					If Not dict.Exists(h) Then dict.Add h, c
				End If
			Next
		End Sub


		Sub RequireHeader(ByRef dict, ByVal h)
			If Not dict.Exists(h) Then
				WScript.Echo "CSV 缺少必要欄位：" & h
				WScript.Quit
			End If
		End Sub


		Function NumericCellValue(ByVal rawValue)
			Dim s
			NumericCellValue = ""
			If IsError(rawValue) Then Exit Function
			s = Trim(CStr(rawValue))
			If s = "" Then Exit Function
			s = Replace(s, "'", "")
			s = Trim(s)
			If s <> "" And IsNumeric(s) Then NumericCellValue = CDbl(s)
		End Function


		Sub BuildReport(ByRef rpt, ByRef sourceWs, _
						ByVal startRow, ByVal endRow, _
						ByVal firstDataCol, ByVal lastDataCol, _
						ByVal vashcMin, ByVal vashcMax, ByVal vashcAvg, _
						ByVal mohmMin, ByVal mohmMax, _
						ByVal kMin, ByVal kMax, _
						ByVal voltageMin, ByVal voltageMax, _
						ByVal modelName , _
						ByVal Dateformat As String, _
						ByVal cell_class As String
					   )

			Dim headers, values1 , values2
			Dim c, r, bodyRow, bodyCol

			headers = Array("挑選作業","模組號","MIN","Max","mAh","AC-IR(mOhm)","K-max","K-min","voltage","配對員","覆核員")
			values1 = Array("執行中",modelName,FormatStat(vashcMin,1),FormatStat(vashcMax,1),FormatStat(vashcAvg, 3), _
						   "",FormatStat(kMax,5),FormatStat(kMin,5), _
						   FormatRange(voltageMin,voltageMax),"自動","")
						   
		    values2 = Array(Dateformat,"K值群組",cell_class,"","ACIR(max-min)", _
						   FormatRange(mohmMin,mohmMax),"","","","","")

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

			For c = 0 To UBound(headers)
				'rpt.Cells(1,c+1).Value = headers(c)
				'rpt.Cells(2,c+1).Value = values1(c)
				'rpt.Cells(1,c+1).Font.Bold = True
				'rpt.Cells(1,c+1).HorizontalAlignment = xlCenter
				'rpt.Cells(2,c+1).HorizontalAlignment = xlCenter
				'rpt.Cells(1,c+1).Borders.LineStyle = xlContinuous
				'rpt.Cells(2,c+1).Borders.LineStyle = xlContinuous
				
				' Header
				rpt.Cells(1, c + 1).Value = headers(c)
				rpt.Cells(1, c + 1).Font.Bold = True
				rpt.Cells(1, c + 1).HorizontalAlignment = xlCenter
				rpt.Cells(1, c + 1).Borders.LineStyle = xlContinuous
				
				' Values 1
				rpt.Cells(2, c + 1).Value = values1(c)
				rpt.Cells(2, c + 1).HorizontalAlignment = xlCenter
				rpt.Cells(2, c + 1).Borders.LineStyle = xlContinuous

				' Values 2
				rpt.Cells(3, c + 1).Value = values2(c)
				rpt.Cells(3, c + 1).HorizontalAlignment = xlCenter
				rpt.Cells(3, c + 1).Borders.LineStyle = xlContinuous				
				
			Next

			'rpt.Cells(3,1).Value = FormatDateTimeNow()

			'========================================
			' Body Header
			'========================================

			rpt.Cells(5, 1).Value = "No."
			rpt.Cells(5, 2).Value = "掃碼機"

			bodyCol = 3

			For c = firstDataCol To lastDataCol

				rpt.Cells(5, bodyCol).Value = _
					CStr(sourceWs.Cells(1, c).Value)

				bodyCol = bodyCol + 1

			Next

			
			'========================================
			' Body Data
			' startRow ~ endRow
			'========================================

			bodyRow = 6

			For r = startRow To endRow
			
			    ' No.
				rpt.Cells(bodyRow,1).Value = _
				     r - startRow + 1
					 
			    ' 掃碼機 
				rpt.Cells(bodyRow,2).Value = ""
				
				' 實際資料
				bodyCol = 3
				
				For c = firstDataCol To lastDataCol
				
					rpt.Cells(bodyRow,bodyCol).Value = _
					    sourceWs.Cells(r,c).Value
						
					bodyCol = bodyCol + 1
					
				Next

				bodyRow = bodyRow + 1
				
			Next
			
			'========================================
			' Body Border / Alignment
			'========================================

			With rpt.Range( _ 
			        rpt.Cells(5,1), _ 
					rpt.Cells(bodyRow-1,bodyCol-1))
					
				.Borders.LineStyle = xlContinuous
				.Borders.Weight = xlThin
				.VerticalAlignment = xlCenter
				.WrapText = False
				
			End With
			
			'========================================
			' Body Header Style
			'========================================

			With rpt.Range( _ 
			     rpt.Cells(5,1), _ 
				 rpt.Cells(5,bodyCol-1))
				 
				.Font.Bold = True
				.HorizontalAlignment = xlCenter
				.WrapText = True
				
			End With
			
			'========================================
			' Column Width
			'========================================

			rpt.Columns(1).ColumnWidth = 5
			rpt.Columns(2).ColumnWidth = 10

			For c = 3 To bodyCol-1
			
				rpt.Columns(c).ColumnWidth = 15
				
				Select Case CStr(rpt.Cells(5,c).Value)
				
					Case "PLCCellID_CE"
					     rpt.Columns(c).ColumnWidth = 18
						 
					Case "PLCCellIDClass_CE" 
					     rpt.Columns(c).ColumnWidth = 15
						 
					Case "PLCTrayID_CE" 
					     rpt.Columns(c).ColumnWidth = 15
						 
					Case "model_combine_number" 
					     rpt.Columns(c).ColumnWidth = 27
						 
					Case "parallel_match" 
					     rpt.Columns(c).ColumnWidth = 15
						 
				End Select
				
			Next


			'========================================
			' Row Height
			'========================================

			rpt.Rows(1).RowHeight = 28
			rpt.Rows(2).RowHeight = 32
			rpt.Rows(3).RowHeight = 25
			rpt.Rows(5).RowHeight = 35
			
			'========================================
			' Print Area
			'========================================

			rpt.PageSetup.PrintArea = _ 
 			       rpt.Range( _ 
				       rpt.Cells(1,1), _ 
					   rpt.Cells( bodyRow - 1, bodyCol - 1 ) _ 
				   ).Address
				
		End Sub


		Function FormatStat(ByVal v , ByVal num As Integer)
			If IsEmpty(v) Then
				FormatStat = ""
			Else
				FormatStat = FormatNumber(CDbl(v),num,-1,0,False)
			End If
		End Function


		Function FormatRange(ByVal minV, ByVal maxV)
			If IsEmpty(minV) Or IsEmpty(maxV) Then
				FormatRange = ""
			Else
				FormatRange = FormatNumber(CDbl(minV),5,-1,0,False) & " ~ " & FormatNumber(CDbl(maxV),3,-1,0,False)
			End If
		End Function


		Function FormatDateTimeNow()
			Dim d
			d = Now
			FormatDateTimeNow = Year(d) & "-" & Right("0"&Month(d),2) & "-" & Right("0"&Day(d),2) & _
								" " & Right("0"&Hour(d),2) & ":" & Right("0"&Minute(d),2)
		End Function


		Function CleanFileName(ByVal s)
			Dim a, i
			a = Array("\","/",":","*","?","""","<",">","|")
			CleanFileName = Trim(s)
			For i=0 To UBound(a)
				CleanFileName = Replace(CleanFileName,a(i),"_")
			Next
			If CleanFileName="" Then CleanFileName="Model"
		End Function
		
		
		'只取當前字串(包含'a~z' 或 'A~Z')的字元結合
		Function GetEnglishLetters(ByVal inputValue)

			Dim i
			Dim ch
			Dim result

			result = ""

			For i = 1 To Len(CStr(inputValue))

				ch = Mid(CStr(inputValue), i, 1)

				If (ch >= "A" And ch <= "Z") Or _
				   (ch >= "a" And ch <= "z") Then

					result = result & ch

				End If

			Next

			GetEnglishLetters = result

		End Function

        '不復蓋原先PDF檔案
		Function GetUniqueFileName(ByVal fullPath)
			Dim folder, base, ext, n, candidate
			If Not fso.FileExists(fullPath) Then
				GetUniqueFileName = fullPath
				Exit Function
			End If

			folder = fso.GetParentFolderName(fullPath)
			base = fso.GetBaseName(fullPath)
			ext = fso.GetExtensionName(fullPath)
			n = 1

			Do
				candidate = folder & "\" & base & "_" & n & "." & ext
				n = n + 1
			Loop While fso.FileExists(candidate)

			GetUniqueFileName = candidate
		End Function
		
		
		'刪除指定路徑之所有PDF檔案,確保每次都是做最後產出的結果
		Function ClearPdfAndGetFileName(ByVal fullPath)

			Dim folder, file

			folder = fso.GetParentFolderName(fullPath)

			' 清除該資料夾內所有 PDF
			If fso.FolderExists(folder) Then

				For Each file In fso.GetFolder(folder).Files

					If LCase(fso.GetExtensionName(file.Name)) = "pdf" Then
						On Error Resume Next
						file.Delete True
						On Error GoTo 0
					End If

				Next

			End If

			' 清除後直接使用原始檔名
			ClearPdfAndGetFileName = fullPath

		End Function
		
		
