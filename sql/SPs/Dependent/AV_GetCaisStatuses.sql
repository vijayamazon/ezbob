IF OBJECT_ID('AV_GetCaisStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetCaisStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE AV_GetCaisStatuses
	@CustomerId INT 
AS
BEGIN

	DECLARE @ConsumerServiceLogId BIGINT 
	EXEC GetExperianConsumerServiceLog @CustomerId, @ConsumerServiceLogId OUTPUT
	
	SELECT cais.LastUpdatedDate, cais.AccountStatusCodes FROM ExperianConsumerData e INNER JOIN ExperianConsumerDataCais cais ON cais.ExperianConsumerDataId = e.Id
	WHERE e.ServiceLogId=@ConsumerServiceLogId AND cais.MatchTo=1 AND cais.LastUpdatedDate IS NOT NULL

END
GO