
IF OBJECT_ID (N'dbo.CRMRanks') IS NULL
BEGIN
	CREATE TABLE dbo.CRMRanks
	(
	  Id   INT IDENTITY NOT NULL
	, Name NVARCHAR (100)
	, CONSTRAINT PK_CRMRanks PRIMARY KEY (Id)
	)
	
	INSERT INTO CRMRanks (Name) VALUES ('High')
	INSERT INTO CRMRanks (Name) VALUES ('Regular')
	INSERT INTO CRMRanks (Name) VALUES ('No')
	INSERT INTO CRMRanks (Name) VALUES ('No Answer ')
	
	ALTER TABLE CustomerRelations ADD RankId INT
	ALTER TABLE CustomerRelations ADD CONSTRAINT FK_CustomerRelations_CRMRanks FOREIGN KEY (RankId) REFERENCES CRMRanks(Id)
	
END 	
	

