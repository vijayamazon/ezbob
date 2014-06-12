IF(Object_ID(N'CRMStatusGroup') IS NULL)
BEGIN 
	CREATE TABLE CRMStatusGroup
	(
		  Id         INT IDENTITY NOT NULL
		, Name       NVARCHAR (50)
		, Priority   INT NOT NULL
		, CONSTRAINT PK_CRMStatusGroup PRIMARY KEY (Id)
	)
END 
GO

IF NOT EXISTS (SELECT * FROM CRMStatusGroup)
BEGIN
	INSERT INTO CRMStatusGroup (Name, Priority) VALUES ('Sales', 1)
	INSERT INTO CRMStatusGroup (Name, Priority) VALUES ('Collection', 2)
	INSERT INTO CRMStatusGroup (Name, Priority) VALUES ('Other', 3)
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'GroupId' and Object_ID = Object_ID(N'CRMStatuses'))
BEGIN 
 ALTER TABLE CRMStatuses ADD [GroupId] INT
 ALTER TABLE CRMStatuses ADD CONSTRAINT FK_CRMStatuses_CRMStatusGroup FOREIGN KEY (GroupId) REFERENCES CRMStatusGroup (Id)
END
GO

UPDATE CRMStatuses SET [GroupId]=(SELECT Id FROM CRMStatusGroup WHERE Name='Sales') WHERE [GroupId] IS NULL AND Name IN ('InProcess','NoSale','Sale','Pending')
UPDATE CRMStatuses SET [GroupId]=(SELECT Id FROM CRMStatusGroup WHERE Name='Collection') WHERE [GroupId] IS NULL AND Name IN ('Arrears letter sent','Default letter sent','Termination letter sent','Legal', 'Collection')
UPDATE CRMStatuses SET [GroupId]=(SELECT Id FROM CRMStatusGroup WHERE Name='Other') WHERE [GroupId] IS NULL AND Name IN ('Support request','N Feedback','P Feedback', 'Need additional info')
GO
