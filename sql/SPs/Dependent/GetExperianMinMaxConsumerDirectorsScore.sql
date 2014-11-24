IF OBJECT_ID('GetExperianMinMaxConsumerDirectorsScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianMinMaxConsumerDirectorsScore AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianMinMaxConsumerDirectorsScore
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianScore INT
	DECLARE @ServiceLogId BIGINT

	SELECT
		MIN(x.ExperianConsumerScore) AS MinExperianScore,
		MAX(x.ExperianConsumerScore) AS MaxExperianScore
	FROM	(
		SELECT ExperianConsumerScore
		FROM Customer
		WHERE Id = @CustomerID
		AND ExperianConsumerScore IS NOT NULL
		
		UNION
		
		SELECT ExperianConsumerScore
		FROM Director
		WHERE CustomerId = @CustomerID
		AND ExperianConsumerScore IS NOT NULL
	) x
END
GO
