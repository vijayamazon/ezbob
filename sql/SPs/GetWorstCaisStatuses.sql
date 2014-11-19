IF OBJECT_ID('GetWorstCaisStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE GetWorstCaisStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetWorstCaisStatuses
@CustomerId INT
AS
BEGIN
	DECLARE @ExperianConsumerDataID BIGINT

	------------------------------------------------------------------------------

	SELECT TOP 1
		@ExperianConsumerDataID = e.Id
	FROM
		MP_ServiceLog l
		INNER JOIN ExperianConsumerData e ON l.Id = e.ServiceLogId
	WHERE
		l.CustomerId = @CustomerId
	ORDER BY
		l.InsertDate DESC

	------------------------------------------------------------------------------

	SELECT DISTINCT
		WorstStatus = RTRIM(LTRIM(ec.WorstStatus))
	FROM
		ExperianConsumerDataCais ec
	WHERE
		ec.ExperianConsumerDataId = @ExperianConsumerDataID
END
GO
