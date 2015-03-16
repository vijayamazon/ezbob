SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerStatuses') AND name = 'IsAutomaticStatus')
BEGIN
	ALTER TABLE CustomerStatuses ADD IsAutomaticStatus BIT NOT NULL DEFAULT(0)
END
GO


IF NOT EXISTS (SELECT * FROM CustomerStatuses WHERE Name = '1 - 14 days missed')
BEGIN
	UPDATE CustomerStatuses SET IsAutomaticStatus = 1 WHERE Name='Enabled'
	
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(10, '1 - 14 days missed', 1, 1, 0, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(11, '15 - 30 days missed', 1, 1, 0, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(12, '31 - 45 days missed', 1, 1, 0, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(13, '46 - 60 days missed', 1, 1, 0, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(14, '60 - 90 days missed', 1, 1, 0, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(15, '90+ days missed', 1, 1, 1, 1)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(16, 'Legal – claim process', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(17, 'Legal - apply for judgment', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(18, 'Legal: CCJ', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(19, 'Legal: bailiff', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(20, 'Legal: charging order', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(21, 'Collection: Tracing', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(22, 'Collection: Site Visit', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(23, 'IVA-CVA', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(24, 'Liquidation', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(25, 'Insolvency', 1, 1, 1, 0)
	INSERT INTO dbo.CustomerStatuses(Id, Name, IsEnabled, IsWarning, IsDefault, IsAutomaticStatus) VALUES(26, 'Bankruptcy', 1, 1, 1, 0)

END
	
GO


IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CustomerStatusHistory') AND name = 'Amount')
BEGIN
	ALTER TABLE CustomerStatusHistory ADD Description NVARCHAR(300) NULL
	ALTER TABLE CustomerStatusHistory ADD Amount DECIMAL(18,4) NULL
	ALTER TABLE CustomerStatusHistory ADD ApplyForJudgmentDate DATETIME NULL
	ALTER TABLE CustomerStatusHistory ADD Type NVARCHAR(20) NULL
	ALTER TABLE CustomerStatusHistory ADD Feedback NVARCHAR(300) NULL
	
END

GO
