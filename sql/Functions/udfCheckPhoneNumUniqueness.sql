IF OBJECT_ID('dbo.udfCheckPhoneNumUniqueness') IS NULL
	EXECUTE('CREATE FUNCTION dbo.udfCheckPhoneNumUniqueness() RETURNS NVARCHAR(255) BEGIN RETURN '''' END')
GO

ALTER FUNCTION dbo.udfCheckPhoneNumUniqueness(
@ContactMobile NVARCHAR(255),
@OriginID INT,
@CheckUsers BIT = 1,
@CheckBrokers BIT = 1,
@RespectSkipCodeNumber BIT = 1
)
RETURNS NVARCHAR(255)
AS
BEGIN
	IF @ContactMobile IS NULL
		RETURN ''

	SET @ContactMobile = LTRIM(RTRIM(@ContactMobile))

	IF @ContactMobile = ''
		RETURN ''

	IF @RespectSkipCodeNumber = 1
	BEGIN
		DECLARE @SkipNum NVARCHAR(255)

		SELECT
			@SkipNum = Value
		FROM
			ConfigurationVariables
		WHERE
			Name = 'SkipCodeGenerationNumber'

		IF @SkipNum = @ContactMobile
			RETURN ''
	END

	IF @CheckUsers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Customer WHERE (DaytimePhone = @ContactMobile OR MobilePhone = @ContactMobile))
			RETURN 'There is already a customer with such phone number: ' + @ContactMobile
	END

	IF @CheckBrokers = 1
	BEGIN
		IF EXISTS (SELECT * FROM Broker WHERE ContactMobile = @ContactMobile AND OriginID = @OriginID)
			RETURN 'There is already a broker with such mobile phone: ' + @ContactMobile
	END

	RETURN ''
END
GO
