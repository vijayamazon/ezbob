IF OBJECT_ID('CustomerAnalyticsUpdateCompany') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdateCompany AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CustomerAnalyticsUpdateCompany
@CustomerID BIGINT,
@Score INT,
@SuggestedAmount DECIMAL(18, 6),
@IncorporationDate DATETIME,
@AnalyticsDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

		SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	UPDATE CustomerAnalyticsCompany SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1

	INSERT INTO CustomerAnalyticsCompany (CustomerID, AnalyticsDate, IsActive, Score, SuggestedAmount, IncorporationDate)
		VALUES (@CustomerID, @AnalyticsDate, 1, @Score, @SuggestedAmount, @IncorporationDate)
	
	COMMIT TRANSACTION
END
GO
