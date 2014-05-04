IF OBJECT_ID('EzTvIsChanged') IS NULL
	EXECUTE('CREATE PROCEDURE EzTvIsChanged AS SELECT 1')
GO

ALTER PROCEDURE IsChanged
@Date DATETIME
AS
BEGIN
	
	IF (SELECT TOP 1 Date FROM Loan ORDER BY 1 DESC) > @Date
		SELECT CAST (1 AS BIT) AS IsChanged
	ELSE 
		SELECT CAST (0 AS BIT) AS IsChanged

END

GO