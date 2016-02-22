SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CustomerLoadLoginData') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerLoadLoginData AS SELECT 1')
GO

ALTER PROCEDURE CustomerLoadLoginData
@UserName NVARCHAR(250),
@OriginID INT
AS
BEGIN
	;WITH u AS (
		SELECT
			UserID = u.UserId,
			IsDisabled = CONVERT(BIT,
				CASE
					WHEN (u.IsDeleted = 1) OR (1 = CONVERT(BIT, CASE LOWER(s.Name) WHEN 'disabled' THEN 1 ELSE 0 END))
						THEN 1
					ELSE
						0
				END
			),
			c.RefNumber,
			u.LoginFailedCount
		FROM
			Customer c
			INNER JOIN Security_User u ON c.Id = u.UserId
			INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
		WHERE
			LOWER(u.UserName) = LOWER(@UserName)
			AND
			u.OriginID = @OriginID
	), o AS (
		SELECT
			Name,
			CustomerCareEmail
		FROM
			CustomerOrigin
		WHERE
			CustomerOriginID = @OriginID
	)
	SELECT
		UserID = ISNULL(u.UserID, 0),
		u.IsDisabled,
		u.RefNumber,
		LoginFailedCount = ISNULL(u.LoginFailedCount, 0),
		o.CustomerCareEmail,
		OriginName = o.Name
	FROM
		u
		FULL OUTER JOIN o ON 1 = 1
END
GO
