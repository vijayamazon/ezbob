IF OBJECT_ID('GetDirectorsScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetDirectorsScore AS SELECT 1')
GO

ALTER PROCEDURE GetDirectorsScore
@CustomerId INT
AS
BEGIN
	SELECT 
		Id,
		Name,
		Surname,
		0 AS Score
	INTO #GetDirectorsScoreTmp
	FROM 
		Director
	WHERE 
		Director.CustomerId = @CustomerId
		
	UPDATE #GetDirectorsScoreTmp SET Score = ExperianScore FROM MP_ExperianDataCache WHERE MP_ExperianDataCache.DirectorId = #GetDirectorsScoreTmp.Id
	
	SELECT Name, Surname, Score FROM #GetDirectorsScoreTmp
	
	DROP TABLE #GetDirectorsScoreTmp
END
GO
