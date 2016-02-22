SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BrokerLoadDetailsForPasswordUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE BrokerLoadDetailsForPasswordUpdate AS SELECT 1')
GO

ALTER PROCEDURE BrokerLoadDetailsForPasswordUpdate
@ContactEmail NVARCHAR(255),
@OriginID INT,
@BrokerID INT OUTPUT,
@EzPassword NVARCHAR(255) OUTPUT,
@Salt NVARCHAR(255) OUTPUT,
@CycleCount NVARCHAR(255) OUTPUT
AS
BEGIN
	SET @BrokerID = 0
	SET @EzPassword = NULL
	SET @Salt = NULL
	SET @CycleCount = NULL

	------------------------------------------------------------------------------

	SELECT
		@BrokerID = b.BrokerID,
		@EzPassword = u.EzPassword,
		@Salt = u.Salt,
		@CycleCount = u.CycleCount
	FROM
		Security_User u
		INNER JOIN Broker b ON u.UserID = b.BrokerID
	WHERE
		b.ContactEmail = @ContactEmail
		AND
		u.OriginID = @OriginID
END
GO
