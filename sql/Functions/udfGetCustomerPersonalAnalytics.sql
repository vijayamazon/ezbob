SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('dbo.udfGetCustomerPersonalAnalytics') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerPersonalAnalytics
GO

CREATE FUNCTION dbo.udfGetCustomerPersonalAnalytics(@CustomerID INT, @Now DATETIME)
RETURNS @output TABLE (
	ExperianConsumerDataID BIGINT NULL,
	Score INT NULL,
	IndebtednessIndex INT NULL,
	ThinFile BIT NULL,
	NumOfAccounts INT NULL,
	NumOfDefaults INT NULL
)
AS
BEGIN
	DECLARE @id BIGINT = dbo.udfLoadExperianConsumerIdForCustomerAndDate(@CustomerID, @Now)

	IF ISNULL(@id, 0) < 1
		RETURN

	INSERT INTO @output (ExperianConsumerDataID, Score, IndebtednessIndex)
	SELECT
		Id,
		BureauScore,
		CII
	FROM
		ExperianConsumerData
	WHERE
		Id = @id

	DECLARE @outputCount INT = ISNULL((SELECT COUNT(*) FROM @output), 0)

	IF @outputCount = 0
		RETURN

	IF @outputCount > 1
	BEGIN
		DELETE FROM @output
		RETURN
	END

	UPDATE @output SET
		NumOfAccounts = (SELECT COUNT(*) FROM ExperianConsumerDataCais WHERE ExperianConsumerDataID = @id),
		NumOfDefaults = (SELECT COUNT(*) FROM ExperianConsumerDataCais WHERE ExperianConsumerDataID = @id AND AccountStatus = 'F')

	UPDATE @output SET
		ThinFile = CASE NumOfAccounts WHEN 0 THEN 1 ELSE 0 END

	RETURN
END
GO
