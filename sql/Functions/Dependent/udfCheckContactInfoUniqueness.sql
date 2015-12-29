IF OBJECT_ID('dbo.udfCheckContactInfoUniqueness') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfCheckContactInfoUniqueness() RETURNS NVARCHAR(255) BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfCheckContactInfoUniqueness(
@ContactEmail NVARCHAR(255),
@OriginID INT,
@ContactMobile NVARCHAR(255),
@CheckUsers BIT = 1,
@CheckBrokers BIT = 1,
@CheckBrokerLeads BIT = 1,
@RespectSkipCodeNumber BIT = 1
)
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @ErrMsg NVARCHAR(255) = ''

	SET @ErrMsg = dbo.udfCheckEmailUniqueness(@ContactEmail, @OriginID, @CheckUsers, @CheckBrokers, @CheckBrokerLeads)

	IF @ErrMsg != ''
		RETURN @ErrMsg

	RETURN dbo.udfCheckPhoneNumUniqueness(@ContactMobile, @OriginID, @CheckUsers, @CheckBrokers, @RespectSkipCodeNumber)
END
GO
