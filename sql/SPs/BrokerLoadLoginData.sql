SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLoadLoginData') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadLoginData AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadLoginData
@Email NVARCHAR(255),
@UiOriginID INT
AS
BEGIN
	SELECT
		BrokerID,
		EzPassword,
		Salt,
		CycleCount
	FROM
		Broker b
		INNER JOIN Security_User u ON b.BrokerID = u.UserId
	WHERE
		LOWER(u.UserName) = @Email
		AND
		u.OriginID = @UiOriginID
END
GO
