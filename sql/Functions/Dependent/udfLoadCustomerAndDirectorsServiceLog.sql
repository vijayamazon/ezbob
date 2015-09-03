SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfLoadCustomerAndDirectorsServiceLog') IS NOT NULL
	DROP FUNCTION dbo.udfLoadCustomerAndDirectorsServiceLog
GO

-- Returned type should be CustomerAndDirectorsServiceLogIDs.
-- However SQL Server does not allows to specify custom type as return type.
-- Therefore explicit table structure is specified and it must be
-- compatible with CustomerAndDirectorsServiceLogIDs (result of this function
-- should be insertable into CustomerAndDirectorsServiceLogIDs).

CREATE FUNCTION dbo.udfLoadCustomerAndDirectorsServiceLog(@CustomerID INT, @Now DATETIME)
RETURNS @output TABLE (
	CustomerID INT NULL,
	DirectorID INT NULL,
	ServiceLogID BIGINT NULL,
	ExperianConsumerDataID BIGINT NULL
)
AS
BEGIN
	IF @CustomerID IS NULL
		RETURN

	INSERT INTO @output (
		CustomerID,
		ServiceLogID
	) VALUES (
		@CustomerID,
		dbo.udfLoadServiceLogIdForCustomerAndDate(@CustomerID, @Now)
	)

	------------------------------------------------------------------------------

	INSERT INTO @output (
		CustomerID,
		DirectorID,
		ServiceLogID
	) SELECT
		CustomerID,
		Id,
		dbo.udfLoadServiceLogIdForDirectorAndDate(Id, @Now)
	FROM
		Director
	WHERE
		CustomerId = @CustomerID

	------------------------------------------------------------------------------

	UPDATE @output SET
		ExperianConsumerDataID = e.Id
	FROM
		@output o
		INNER JOIN ExperianConsumerData e ON o.ServiceLogID = e.ServiceLogID

	------------------------------------------------------------------------------

	RETURN
END
GO
