IF OBJECT_ID('dbo.udfLoadServiceLogIdForCustomerAndDate') IS NOT NULL
	DROP FUNCTION dbo.udfLoadServiceLogIdForCustomerAndDate
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfLoadServiceLogIdForCustomerAndDate(@CustomerId INT, @Now DATETIME)
RETURNS BIGINT
AS
BEGIN
	DECLARE @ServiceLogID BIGINT

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
		AND (
			@Now IS NULL OR l.InsertDate < @Now
		)
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
				AND (@Now IS NULL OR l.InsertDate < @Now)
			WHERE
				c.Id = @CustomerId
			ORDER BY
				l.InsertDate DESC,
				l.Id DESC
	END

	------------------------------------------------------------------------------

	RETURN @ServiceLogId
END
GO
