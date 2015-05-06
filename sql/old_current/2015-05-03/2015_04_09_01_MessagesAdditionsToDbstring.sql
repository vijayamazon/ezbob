IF NOT EXISTS (SELECT 1 FROM DbString WHERE [Key]='EmailAddressAlreadyRegisteredInOtherOrigin')
	BEGIN
		INSERT INTO dbo.DbString([Key], Value)
		VALUES('EmailAddressAlreadyRegisteredInOtherOrigin', 'This email is already registered. For more information contact support: ')
	END

IF NOT EXISTS (SELECT 1 FROM DbString WHERE [Key]='MaximumAnswerLengthExceeded')
	BEGIN
		INSERT INTO dbo.DbString([Key], Value)
		VALUES('MaximumAnswerLengthExceeded', 'Maximum answer length is 199 characters')
	END

IF NOT EXISTS (SELECT 1 FROM DbString WHERE [Key]='InvalidMobileCode')
	BEGIN
		INSERT INTO dbo.DbString([Key], Value)
		VALUES('InvalidMobileCode', 'Invalid code')
	END

IF NOT EXISTS (SELECT 1 FROM DbString WHERE [Key]='UserCreationFailed')
	BEGIN
		INSERT INTO dbo.DbString([Key], Value)
		VALUES('UserCreationFailed', 'Failed to create user')
	END

IF NOT EXISTS (SELECT 1 FROM DbString WHERE [Key]='CustomeIdNotProvided')
	BEGIN
		INSERT INTO dbo.DbString([Key], Value)
		VALUES('CustomeIdNotProvided', 'Customer id is not provided')
	END
GO
