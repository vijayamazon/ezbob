IF OBJECT_ID('LoadExperianConsumerForCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerForCustomer AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadExperianConsumerForCustomer
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ServiceLogID BIGINT
	DECLARE @InsertDate DATETIME
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id, @InsertDate = InsertDate
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.ServiceType = 'Consumer Request'
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC
	
	------------------------------------------------------------------------------

	EXECUTE LoadFullExperianConsumer @ServiceLogID, @InsertDate
END
GO
