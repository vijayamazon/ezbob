IF OBJECT_ID('CustomerAnalyticsUpdateDirector') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdateDirector AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CustomerAnalyticsUpdateDirector
@CustomerID INT,
@Score INT,
@AnalyticsDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	DECLARE @MinScore INT
	DECLARE @MaxScore INT

	SELECT
		@MinScore = MinScore,
		@MaxScore = MaxScore
	FROM
		CustomerAnalyticsDirector
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1

	IF @MinScore IS NOT NULL
	BEGIN
		UPDATE CustomerAnalyticsDirector SET
			IsActive = 0
		WHERE
			CustomerID = @CustomerID
			AND
			ISActive = 1
	END

	INSERT INTO CustomerAnalyticsDirector (CustomerID, AnalyticsDate, IsActive, MinScore, MaxScore)
		VALUES (@CustomerID, @AnalyticsDate, 1, dbo.udfMinInt(@MinScore, @Score), dbo.udfMaxInt(@MaxScore, @Score))

	COMMIT TRANSACTION
END
GO
