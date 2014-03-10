IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptIndexSize]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptIndexSize]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptIndexSize] 
	(@DateStart DATETIME, @DateEnd DATETIME)
AS
BEGIN
SELECT TOP 30 i1.object_name AS TableName, i1.index_name AS IndexName, i1.in_row_reserved_page_count AS CurrentReservedPageSize, i2.in_row_reserved_page_count AS PreviousReserevedPageSize, (i1.in_row_reserved_page_count-i2.in_row_reserved_page_count) AS SizeChange 
FROM LoadStat.[dbo].db_indexes_stats i1,LoadStat.[dbo].db_indexes_stats i2 
WHERE i1.index_id= i2.index_id AND i1.object_id=i2.object_id
AND CONVERT(DATE,i1.measure_date)=@DateEnd 
AND CONVERT(DATE,i2.measure_date)=@DateStart 
ORDER BY (i1.in_row_reserved_page_count-i2.in_row_reserved_page_count) DESC
END
GO