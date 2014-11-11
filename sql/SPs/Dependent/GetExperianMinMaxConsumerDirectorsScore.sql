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

	EXEC GetExperianConsumerServiceLog @CustomerId, @ServiceLogId OUTPUT

	SELECT
		MIN(d.BureauScore) AS MinExperianScore,
		MAX(d.BureauScore) AS MaxExperianScore
	FROM 
		ExperianConsumerData d		
	WHERE
		d.ServiceLogId = @ServiceLogId
		OR 
		d.ServiceLogId IN ( -- Directors last consumer check
			SELECT
				p1.Id
			FROM
				MP_ServiceLog p1
				LEFT JOIN MP_ServiceLog p2
					ON p1.DirectorId = p2.DirectorId
					AND p1.Id < p2.Id
			WHERE
				p2.Id IS NULL
				AND
				p1.CustomerId = @CustomerId
				AND
				p1.DirectorId IS NOT NULL
				AND
				p1.ServiceType = 'Consumer Request'
	)
END
GO
