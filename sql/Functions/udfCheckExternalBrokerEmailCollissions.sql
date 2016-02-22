SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfCheckExternalBrokerEmailCollissions') IS NOT NULL
	DROP FUNCTION dbo.udfCheckExternalBrokerEmailCollissions
GO

CREATE FUNCTION dbo.udfCheckExternalBrokerEmailCollissions(@ContactEmail NVARCHAR(255))
RETURNS NVARCHAR(255)
AS
BEGIN
	DECLARE @EmailToCheck NVARCHAR(255) = LOWER(ISNULL(@ContactEmail, ''))

	IF EXISTS (SELECT Id FROM Customer WHERE LOWER(Name) = @EmailToCheck)
		RETURN 'email is already being used'

	IF EXISTS (SELECT BrokerLeadID FROM BrokerLeads WHERE LOWER(Email) = @EmailToCheck)
		RETURN 'email is already being used'

	RETURN ''
END
GO
