IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EzServiceUpdateConfiguration')
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'EzServiceUpdateConfiguration', '30', 'In minutes. How ofter re-read configuration variables from DB in EzService.')
GO
