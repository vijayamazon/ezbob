IF OBJECT_ID('GetExperianConsumerScore') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianConsumerScore AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianConsumerScore
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianScore INT
	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerId, @ServiceLogId OUTPUT

	SELECT
		@ExperianScore = d.BureauScore
	FROM
		ExperianConsumerData d
	WHERE
		d.ServiceLogId = @ServiceLogId

	IF @ExperianScore IS NULL
		SELECT @ExperianScore = 0

	SELECT @ExperianScore AS ExperianScore
END
GO
