SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerMortgagesData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerMortgagesData AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerMortgagesData
@CustomerId BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ServiceLogId INT = (SELECT TOP 1 Id 
								FROM MP_ServiceLog 
								WHERE ServiceType='Consumer Request' 
								AND CustomerId = @CustomerId 
								AND DirectorId IS NULL 
								ORDER BY InsertDate DESC
							   )
							   
	SELECT count(*) AS NumMortgages, isnull(sum(Balance),0) AS MortageBalance 
	FROM ExperianConsumerDataCais c INNER JOIN ExperianConsumerData d ON d.Id = c.ExperianConsumerDataId 
	WHERE d.ServiceLogId=@ServiceLogId 
	AND AccountType IN ('03','16','25','30','31','32','33','34','35','69') 
	AND MatchTo=1 
	AND AccountStatus <> 'S'
END 
GO
