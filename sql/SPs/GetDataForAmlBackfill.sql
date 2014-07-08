IF OBJECT_ID('GetDataForAmlBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForAmlBackfill AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetDataForAmlBackfill
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE 
		@Xml NVARCHAR(MAX),
		@CustomerId INT,
		@ExistingAmlResult NVARCHAR(100)
		
	CREATE TABLE #DataForAmlBackfill
	(
		CustomerId INT,
		AmlResult NVARCHAR(100),
		Xml NVARCHAR(MAX)
	)
	
	DECLARE cur CURSOR FOR 
		SELECT DISTINCT CustomerId FROM MP_ServiceLog WHERE ServiceType = 'AML A check' AND CustomerId IS NOT NULL AND Id IN (SELECT DISTINCT ServiceLogId FROM MP_ExperianBankCache)
	OPEN cur
	FETCH NEXT FROM cur INTO @CustomerId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT TOP 1 @Xml = ResponseData FROM MP_ServiceLog WHERE ServiceType = 'AML A check' AND CustomerId = @CustomerId AND Id IN (SELECT DISTINCT ServiceLogId FROM MP_ExperianBankCache) ORDER BY Id DESC
		SELECT @ExistingAmlResult = AMLResult FROM Customer WHERE Id = @CustomerId
		
		INSERT INTO #DataForAmlBackfill (CustomerId, AmlResult, Xml) VALUES (@CustomerId, @ExistingAmlResult, @Xml)
	
		FETCH NEXT FROM cur INTO @CustomerId
	END
	CLOSE cur
	DEALLOCATE cur
	
	SELECT CustomerId, AmlResult, Xml FROM #DataForAmlBackfill
	
	DROP TABLE #DataForAmlBackfill	
END
GO
