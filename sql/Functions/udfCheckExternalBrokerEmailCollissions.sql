SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfCheckExternalBrokerEmailCollissions') IS NOT NULL
	DROP FUNCTION dbo.udfCheckExternalBrokerEmailCollissions
GO

CREATE FUNCTION dbo.udfCheckExternalBrokerEmailCollissions(@ContactEmail NVARCHAR(255))
RETURNS NVARCHAR(255)
AS
BEGIN
	IF EXISTS (SELECT Id FROM Customer WHERE Name = @ContactEmail)
		RETURN 'email is already being used'

	IF EXISTS (SELECT BrokerLeadID FROM BrokerLeads WHERE Email = @ContactEmail)
		RETURN 'email is already being used'

	RETURN ''
END
GO
