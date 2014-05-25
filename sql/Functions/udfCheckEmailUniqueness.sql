IF OBJECT_ID('dbo.udfCheckEmailUniqueness') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfCheckEmailUniqueness() RETURNS NVARCHAR(255) BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfCheckEmailUniqueness(
@ContactEmail NVARCHAR(255),
@CheckUsers BIT = 1,
@CheckBrokers BIT = 1,
@CheckBrokerLeads BIT = 1
)
RETURNS NVARCHAR(255)
AS
BEGIN
	IF @CheckUsers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Security_User WHERE Email = @ContactEmail)
			RETURN 'email is already being used'
	END

	IF @CheckBrokers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Broker WHERE ContactEmail = @ContactEmail)
			RETURN 'email is already being used'
	END

	IF @CheckBrokerLeads = 1
	BEGIN
		IF EXISTS (SELECT * FROM BrokerLeads WHERE Email = @ContactEmail AND BrokerLeadDeletedReasonID IS NULL AND DateDeleted IS NULL)
			RETURN 'There is already a lead with such email: ' + @ContactEmail
	END

	RETURN ''
END
GO
