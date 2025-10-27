SELECT
                              
                                COUNT(CASE
                                        WHEN
                                          REPLACE(CONVERT(NVARCHAR(100), create_date, 120), '.', '-')
                                          BETWEEN '2025-10-22 00:00:00' AND '2025-10-22 23:59:59'  
                                          AND (BIN_CODE LIKE 'N%' AND BIN_CODE  LIKE 'N2%')
                                        THEN 1
                                      END) AS todayevening_total_capacity                   
                              FROM ITFC_MES_UPLOAD_STATUS_TB
                              WHERE
                                TYPE = 4
                                AND BOX_BATT <> 'NANANANANANA'



 SELECT 
                        COUNT(CASE WHEN BIN_CODE LIKE 'N%' THEN 1 END) AS 常溫一期
                     /*   COUNT(CASE WHEN BIN_CODE LIKE 'N2%' THEN 1 END) AS 常溫二期, */
                     /* COUNT(CASE WHEN BIN_CODE LIKE 'N%' THEN 1 END) + COUNT(CASE WHEN BIN_CODE LIKE 'N2%' THEN 1 END) AS 常溫當天總產能*/
                      FROM ITFC_MES_UPLOAD_STATUS_TB
                      WHERE 
                        TYPE = 4
                        AND BOX_BATT <> 'NANANANANANA'
                        AND REPLACE(CONVERT(NVARCHAR(100), create_date, 120), '.', '-') BETWEEN '2025-10-22 08:00:00' AND '2025-10-22 23:59:59';


SELECT
                              
                                COUNT(CASE
                                        WHEN
                                          REPLACE(CONVERT(NVARCHAR(100), create_date, 120), '.', '-')
                                          BETWEEN '2025-10-22 00:00:00' AND '2025-10-22 23:59:59'  
                                          AND (BIN_CODE LIKE 'N%' AND BIN_CODE  LIKE 'N2%')
                                        THEN 1
                                      END) AS todayevening_total_capacity                   
                              FROM ITFC_MES_UPLOAD_STATUS_TB
                              WHERE
                                TYPE = 4
                                AND BOX_BATT <> 'NANANANANANA'
