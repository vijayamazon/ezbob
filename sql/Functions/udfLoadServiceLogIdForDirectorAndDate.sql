IF OBJECT_ID('dbo.udfLoadServiceLogIdForDirectorAndDate') IS NOT NULL
	DROP FUNCTION dbo.udfLoadServiceLogIdForDirectorAndDate
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION dbo.udfLoadServiceLogIdForDirectorAndDate(@DirectorId INT, @Now DATETIME)
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
		l.DirectorId = @DirectorId
		AND
		l.ServiceType = 'Consumer Request'
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
			Director d
			INNER JOIN CustomerAddress a
				ON a.DirectorId = d.Id
				AND a.addressType = 1
			INNER JOIN MP_ServiceLog l
				ON l.Firstname = d.Name
				AND l.Surname = d.Surname
				AND l.DateOfBirth = d.DateOfBirth
				AND l.Postcode = a.Postcode
				AND l.ServiceType = 'Consumer Request'
				AND (@Now IS NULL OR l.InsertDate < @Now)
			WHERE
				d.Id = @DirectorId
			ORDER BY
				l.InsertDate DESC,
				l.Id DESC
	END

	------------------------------------------------------------------------------

	RETURN @ServiceLogId
END
GO
