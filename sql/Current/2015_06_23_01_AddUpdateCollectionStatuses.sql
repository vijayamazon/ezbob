IF NOT EXISTS (SELECT * FROM CustomerStatuses WHERE Name='CUST INSOLVENT PROCEED PG')
BEGIN
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus)
	VALUES(27, 'CUST INSOLVENT PROCEED PG', 0, 1, 0, 0)
END
GO

IF NOT EXISTS (SELECT * FROM CustomerStatuses WHERE Name='PG INSOLVENT PROCEED CUST')
BEGIN
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus)
	VALUES(28, 'PG INSOLVENT PROCEED CUST', 0, 1, 0, 0)
END
GO

UPDATE CustomerStatuses SET IsEnabled=0 WHERE Id IN (1,2,3,4,5,8,9,18,20,21,27,28)
GO


IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('CustomerStatuses') AND name='IsVisible')
BEGIN
	ALTER TABLE CustomerStatuses DROP COLUMN TimestampCounter
	ALTER TABLE CustomerStatuses ADD IsVisible BIT NOT NULL DEFAULT(0)
	ALTER TABLE CustomerStatuses ADD TimestampCounter ROWVERSION
END 
GO

UPDATE CustomerStatuses SET IsVisible=1 WHERE Id NOT IN (16,17,19,23,24,25,26)
GO