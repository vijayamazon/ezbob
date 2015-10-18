IF OBJECT_ID('CustomerAnalyticsUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CustomerAnalyticsUpdate
@CustomerID INT,
@Score INT,
@IndebtednessIndex INT,
@NumOfAccounts INT,
@NumOfDefaults INT,
@NumOfLastDefaults INT,
@AnalyticsDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	UPDATE CustomerAnalyticsPersonal SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		ISActive = 1

	INSERT INTO CustomerAnalyticsPersonal (CustomerID, AnalyticsDate, IsActive, Score, IndebtednessIndex, NumOfAccounts, NumOfDefaults, NumOfLastDefaults)
		VALUES (@CustomerID, @AnalyticsDate, 1, @Score, @IndebtednessIndex, @NumOfAccounts, @NumOfDefaults, @NumOfLastDefaults)

	COMMIT TRANSACTION
END
GO
