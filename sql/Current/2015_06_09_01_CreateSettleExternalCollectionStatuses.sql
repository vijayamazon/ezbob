IF OBJECT_ID('ExternalCollectionStatuses') IS NULL
BEGIN
	CREATE TABLE [ExternalCollectionStatuses] (
		[ExternalCollectionStatusID] INT UNIQUE NOT NULL,
		[Name] NVARCHAR(50) NULL,
		CONSTRAINT PK_ExternalCollectionStatuses PRIMARY KEY ([ExternalCollectionStatusID])
	)
END
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = '1st place DCA1')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(0, '1st place DCA1')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = '1st place DCA2')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(1, '1st place DCA2')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = '2st place DCA1')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(2, '2st place DCA1')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = '2st place DCA2')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(3, '2st place DCA2')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = 'T&C DCA')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(4, 'T&C DCA')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = 'Insolvency Practitioner')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(5, 'Insolvency Practitioner')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = 'SOLICITORS')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(6, 'SOLICITORS')
END	
GO

IF NOT EXISTS (SELECT 1 FROM ExternalCollectionStatuses WHERE Name = 'Debt Sale')
BEGIN
	INSERT INTO dbo.ExternalCollectionStatuses([ExternalCollectionStatusID], Name) VALUES(7, 'Debt Sale')
END	
GO

