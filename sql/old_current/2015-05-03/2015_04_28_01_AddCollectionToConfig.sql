DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Prod'
BEGIN 
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'CollectionToAddress')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('CollectionToAddress', 'emma@ezbob.com', 'Collection mail reciever')
	END 
END 

IF @Environment != 'Prod' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name = 'CollectionToAddress')
	BEGIN
		INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES ('CollectionToAddress', '', 'Collection mail reciever')
	END 
END 

GO