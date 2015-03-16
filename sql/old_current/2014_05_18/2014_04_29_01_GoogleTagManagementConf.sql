DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment <> 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='GoogleTagManagementProd')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('GoogleTagManagementProd', 'false', 'true - to use production tag management container, false - to use test one')
	END
END 

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='GoogleTagManagementProd')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('GoogleTagManagementProd', 'true', 'true - to use production tag management container, false - to use test one')
	END
END 

GO
