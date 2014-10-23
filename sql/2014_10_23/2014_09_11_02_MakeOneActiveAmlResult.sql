DECLARE @LookupKey NVARCHAR(500), @MaxId INT

SELECT COUNT(1) AS Num, LookupKey INTO #AmlKeys FROM AmlResults WHERE IsActive=1 GROUP BY LookupKey

DECLARE cur CURSOR FOR 
	SELECT LookupKey FROM #AmlKeys WHERE Num > 1
OPEN cur
FETCH NEXT FROM cur INTO @LookupKey
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT @MaxId = MAX(Id) FROM AmlResults WHERE LookupKey = @LookupKey AND IsActive = 1

	UPDATE AmlResults SET IsActive = 0 WHERE LookupKey = @LookupKey AND Id != @MaxId

	FETCH NEXT FROM cur INTO @LookupKey
END
CLOSE cur
DEALLOCATE cur

DROP TABLE #AmlKeys
GO

