IF OBJECT_ID('GetExperianMinMaxConsumerDirectorsScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianMinMaxConsumerDirectorsScore AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianMinMaxConsumerDirectorsScore
@CustomerId INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		MinScore AS MinExperianScore,
		MaxScore AS MaxExperianScore
	FROM
		dbo.udfGetCustomerScoreAnalytics(@CustomerId, @Now)
END
GO
