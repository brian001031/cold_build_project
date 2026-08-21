Option Explicit

Dim excelApp
Dim wb
Dim xlsmPath

xlsmPath = "C:\MES\ModelPackageTool\pdf_gener_result.xlsm"

Set excelApp = CreateObject("Excel.Application")

excelApp.Visible = True
excelApp.DisplayAlerts = False

Set wb = excelApp.Workbooks.Open(xlsmPath)

excelApp.Run "Generate_Package_PDF"

Set wb = Nothing
Set excelApp = Nothing