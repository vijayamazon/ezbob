DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment <> 'Prod' OR @Environment IS NULL 
BEGIN
	UPDATE ConfigurationVariables SET Value='yaron789' WHERE Name='SalesForcePassword'
	UPDATE ConfigurationVariables SET Value='8ZVLKiPhbXlUcyhLwdViSAxZ' WHERE Name='SalesForceToken'
END
GO
