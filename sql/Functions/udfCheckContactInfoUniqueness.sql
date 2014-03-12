IF OBJECT_ID('dbo.udfCheckContactInfoUniqueness') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfCheckContactInfoUniqueness() RETURNS NVARCHAR(255) BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfCheckContactInfoUniqueness(
@ContactEmail NVARCHAR(255),
@ContactMobile NVARCHAR(255),
@CheckUsers BIT = 1,
@CheckBrokers BIT = 1,
@CheckBrokerLeads BIT = 1
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @ErrMsg NVARCHAR(255) = ''

	IF @CheckUsers = 1 AND @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM Security_User WHERE Email = @ContactEmail)
			SET @ErrMsg = 'There is already a customer with such email: ' + @ContactEmail
	END

	IF @CheckUsers = 1 AND @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM Customer WHERE DaytimePhone = @ContactMobile OR MobilePhone = @ContactMobile)
			SET @ErrMsg = 'There is already a customer with such phone number: ' + @ContactMobile
	END

	IF @CheckBrokers = 1 AND @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM Broker WHERE ContactEmail = @ContactEmail OR ContactMobile = @ContactMobile)
			SET @ErrMsg = 'There is already a broker with such email: ' + @ContactEmail + ' or such mobile phone: ' + @ContactMobile
	END

	IF @CheckBrokerLeads = 1 AND @ErrMsg = ''
	BEGIN
		IF EXISTS (SELECT * FROM BrokerLeads WHERE Email = @ContactEmail AND BrokerLeadDeletedReasonID IS NULL)
			SET @ErrMsg = 'There is already a lead with such email: ' + @ContactEmail
	END

	RETURN @ErrMsg
END
GO
