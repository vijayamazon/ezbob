SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM CustomerStatuses WHERE Name = 'Cust insolvent proceed PG')
BEGIN
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(27, 'Cust insolvent proceed PG', 1, 0, 1, 0)	
END	
GO

IF NOT EXISTS (SELECT * FROM CustomerStatuses WHERE Name = 'PG insolvent proceed Cust')
BEGIN	
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(28, 'PG insolvent proceed Cust', 1, 0, 1, 0)
END	
GO