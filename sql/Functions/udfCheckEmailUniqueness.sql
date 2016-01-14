IF OBJECT_ID('dbo.udfCheckEmailUniqueness') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfCheckEmailUniqueness() RETURNS NVARCHAR(255) BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfCheckEmailUniqueness(
@ContactEmail NVARCHAR(255),
@OriginID INT,
@CheckUsers BIT = 1,
@CheckBrokers BIT = 1,
@CheckBrokerLeads BIT = 1
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @EmailToCheck NVARCHAR(255) = LOWER(ISNULL(@ContactEmail, ''))

	IF @EmailToCheck = ''
		RETURN 'email is already being used'

	IF @CheckUsers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Security_User WHERE LOWER(Email) = @EmailToCheck AND OriginID = @OriginID)
			RETURN 'email is already being used'
	END

	IF @CheckBrokers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Broker WHERE LOWER(ContactEmail) = @EmailToCheck AND OriginID = @OriginID)
			RETURN 'email is already being used'
	END

	IF @CheckBrokerLeads = 1
	BEGIN
		IF EXISTS (
			SELECT *
			FROM BrokerLeads l
			INNER JOIN Broker b ON l.BrokerID = b.BrokerID AND b.OriginID = @OriginID
			WHERE LOWER(l.Email) = @EmailToCheck
			AND l.BrokerLeadDeletedReasonID IS NULL
			AND l.DateDeleted IS NULL
		)
			RETURN 'There is already a lead with such email: ' + @ContactEmail
	END

	RETURN ''
END
GO
