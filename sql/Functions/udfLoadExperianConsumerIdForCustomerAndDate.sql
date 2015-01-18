IF OBJECT_ID('dbo.udfLoadExperianConsumerIdForCustomerAndDate') IS NOT NULL
	DROP FUNCTION dbo.udfLoadExperianConsumerIdForCustomerAndDate
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfLoadExperianConsumerIdForCustomerAndDate(@CustomerId INT, @Now DATETIME)
RETURNS BIGINT
AS
BEGIN
	DECLARE @ServiceLogID BIGINT
	DECLARE @ExperianConsumerId BIGINT

	------------------------------------------------------------------------------

	SELECT TOP 1
		@ServiceLogID = Id
	FROM
		MP_ServiceLog l
	WHERE
		l.CustomerId = @CustomerId
		AND
		l.ServiceType = 'Consumer Request'
		AND
		l.DirectorId IS NULL
		AND
		l.InsertDate < @Now
	ORDER BY
		l.InsertDate DESC,
		l.Id DESC

	------------------------------------------------------------------------------

	IF @ServiceLogId IS NULL
	BEGIN
		SELECT TOP 1
			@ServiceLogID = l.Id
		FROM
			Customer c 
			INNER JOIN CustomerAddress a
				ON a.CustomerId = c.Id
				AND a.addressType = 1
			INNER JOIN MP_ServiceLog l
				ON l.Firstname = c.FirstName
				AND l.Surname = c.Surname
				AND l.DateOfBirth = c.DateOfBirth
				AND l.Postcode = a.Postcode
				AND l.ServiceType = 'Consumer Request'
				AND l.InsertDate < @Now
			WHERE
				c.Id = @CustomerId
			ORDER BY
				l.InsertDate DESC,
				l.Id DESC
	END

	------------------------------------------------------------------------------

	SELECT
		@ExperianConsumerId = Id
	FROM
		ExperianConsumerData
	WHERE
		ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	RETURN ISNULL(@ExperianConsumerId, 0)
END
GO
