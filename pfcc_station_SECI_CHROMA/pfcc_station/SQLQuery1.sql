SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'H-%' ORDER BY ID DESC; 
select count(*) AS cell_HT_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' and BIN_CODE like 'H%' and type=4 and BOX_BATT <> 'NANANANANANA' and TEST_STATUS =0;



SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'N%' ORDER BY ID DESC;SELECT TOP 1 * FROM ITFC_MES_UPLOAD_STATUS_TB WHERE BIN_CODE LIKE 'N2%' ORDER BY ID DESC; 
                           select count(*) AS cell_RT_1_period_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' 
                           and BIN_CODE like 'N-%' and type=4 and BOX_BATT <> 'NANANANANANA'   and TEST_STATUS =0 ; 
                           select count(*) AS cell_RT_2_period_product_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1  and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' 
                           and BIN_CODE like 'N2-%' and type=4 and BOX_BATT <> 'NANANANANANA' and TEST_STATUS =0 ;





select count(*) AS cell_HT_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' and BIN_CODE like 'H-%' and type=4 and BOX_BATT <> 'NANANANANANA' 
        select count(*) AS cell_RT_1_period_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1 and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' 
        and BIN_CODE like 'N-%'  AND BIN_CODE NOT LIKE 'N2-%' and type=4 and BOX_BATT <> 'NANANANANANA'; 
        select count(*) AS cell_RT_2_period_num from ITFC_MES_UPLOAD_STATUS_TB where 1=1  and replace(convert(nvarchar(100),create_date,120),'.','-') between '2026-05-18 00:00:00' AND '2026-05-18 23:59:59' 
        and BIN_CODE like 'N2-%' and type=4 and BOX_BATT <> 'NANANANANANA'