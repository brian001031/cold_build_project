Option Explicit

Dim excelApp
Dim wb
Dim xlsmPath

xlsmPath = "C:\MES\ModelPackageTool\test_tune.xlsm"

Set excelApp = CreateObject("Excel.Application")

excelApp.Visible = True
excelApp.DisplayAlerts = False

Set wb = excelApp.Workbooks.Open(xlsmPath)

excelApp.Run "Generate_Model_Combine_Final"

Set wb = Nothing
Set excelApp = Nothing