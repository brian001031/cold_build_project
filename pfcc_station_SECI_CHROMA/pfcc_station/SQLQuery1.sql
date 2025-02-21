select count(*) AS cell_HT_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 
and replace(convert(nvarchar(100),create_date,120),'.','-') between '2025-02-21 00:00' and '2025-02-21 20:00' 
and BIN_CODE like 'H%' 
and type=4 
and BOX_BATT <> 'NANANANANANA'

select count(*) AS cell_RT_1_period_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 
and replace(convert(nvarchar(100),create_date,120),'.','-') between '2025-02-21 00:00' and '2025-02-21 20:00' 
and BIN_CODE like 'N%' 
and type=4 
and BOX_BATT <> 'NANANANANANA'

 

select count(*) AS cell_RT_2_period_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 
and replace(convert(nvarchar(100),create_date,120),'.','-') between '2025-02-21 00:00' and '2025-02-21 20:00' 
and BIN_CODE like 'N2%' 
and type=4 
and BOX_BATT <> 'NANANANANANA'

SELECT ID
FROM (
    SELECT TOP 1 ID 
    FROM ITFC_MES_UPLOAD_STATUS_TB
    WHERE BIN_CODE LIKE 'N%'
    UNION ALL
    SELECT TOP 1 ID
    FROM ITFC_MES_UPLOAD_STATUS_TB
    WHERE BIN_CODE LIKE 'N2%'
) AS CombinedResults
ORDER BY ID DESC;



SELECT TOP 1 ID
FROM ITFC_MES_UPLOAD_STATUS_TB
WHERE BIN_CODE LIKE 'H%' ORDER BY ID DESC;

SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'H%' ORDER BY ID DESC
SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'N%' ORDER BY ID DESC;
SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'N2%' ORDER BY ID DESC;


