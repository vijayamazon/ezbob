SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetExperianConsumerServiceLog') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianConsumerServiceLog AS SELECT 1')
GO

ALTER PROCEDURE GetExperianConsumerServiceLog
@CustomerId INT,
@ServiceLogId BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		@ServiceLogID = Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.DirectorId IS NULL
		AND
		l.ServiceType = 'Consumer Request'
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
END
GO
