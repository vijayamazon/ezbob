SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetCaisStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCaisStatuses AS SELECT 1')
GO

ALTER PROCEDURE AV_GetCaisStatuses
@CustomerId INT 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ConsumerServiceLogId BIGINT 
	EXECUTE GetExperianConsumerServiceLog @CustomerId, @ConsumerServiceLogId OUTPUT

	SELECT
		cais.LastUpdatedDate,
		cais.AccountStatusCodes,
		cais.Balance,
		cais.CurrentDefBalance
	FROM
		ExperianConsumerData e
		INNER JOIN ExperianConsumerDataCais cais ON cais.ExperianConsumerDataId = e.Id
	WHERE
		e.ServiceLogId = @ConsumerServiceLogId
		AND
		cais.MatchTo = 1
		AND
		cais.LastUpdatedDate IS NOT NULL
END
GO
