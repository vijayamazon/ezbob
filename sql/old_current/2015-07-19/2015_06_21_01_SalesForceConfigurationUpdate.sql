DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment <> 'Prod' OR @Environment IS NULL 
BEGIN
	UPDATE ConfigurationVariables SET Value='Ezca$h123' WHERE Name='SalesForcePassword'
	UPDATE ConfigurationVariables SET Value='H3pfFEE09tKxp0vTCoK0mfiS' WHERE Name='SalesForceToken'
END
GO
