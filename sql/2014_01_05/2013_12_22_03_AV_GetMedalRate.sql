IF OBJECT_ID('AV_GetMedalRate') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetMedalRate AS SELECT 1')
GO

ALTER PROCEDURE AV_GetMedalRate
@CustomerId INT
AS
BEGIN 
	SELECT TOP 1 ScoreResult FROM CustomerScoringResult WHERE CustomerId=@CustomerId ORDER BY ScoreDate DESC
END 
