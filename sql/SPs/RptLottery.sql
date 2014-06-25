IF OBJECT_ID('RptLottery') IS NULL
	EXECUTE('CREATE PROCEDURE RptLottery AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLottery
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #data (
		Name NVARCHAR(32),
		Value INT,
		Delta INT,
		Css NVARCHAR(64)
	)

	DECLARE @Num INT
	DECLARE @MinDelta INT

	SELECT
		@Num = COUNT(*)
	FROM
		Broker
	WHERE
		'June 25 2014' <= AgreedToTermsDate AND AgreedToTermsDate < 'June 28 2014'
		AND
		IsTest = 0

	INSERT INTO #data(Name, Value) VALUES
		('Nimrod', 55),
		('Shiri', 47),
		('Ros', 50),
		('Nir', 33),
		('Eilay', 24),
		('Oran', 36),
		('Emma', 58),
		('Stas', 40),
		('Alex', 44),
		('-- Actual number --', @Num)

	UPDATE #data SET
		Delta = ABS(@Num - Value)

	SELECT
		@MinDelta = MIN(Delta)
	FROM
		#data
	WHERE
		Name NOT LIKE '--%'

	UPDATE #data SET
		Css = 'total'
	WHERE
		Name LIKE '--%'

	UPDATE #data SET
		Css = 'total2'
	WHERE
		Delta = @MinDelta

	SELECT
		Name,
		Value,
		Css
	FROM
		#data
	ORDER BY
		Value

	DROP TABLE #data
END
GO
