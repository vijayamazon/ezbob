IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoReRejectMinRepaidPortion')
BEGIN
	INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('AutoReRejectMinRepaidPortion', '0.5', 'Minimal repaid portion of principal for a customer not to be auto re-rejected', NULL)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'AutoReRejectMaxLRDAge')
BEGIN
	INSERT INTO dbo.ConfigurationVariables(Name, Value, Description, IsEncrypted)
	VALUES('AutoReRejectMaxLRDAge', '30', 'Integer. No auto re-reject if after the last manually rejected cash request happened more than this number of days ago.', 0)
END 
GO
