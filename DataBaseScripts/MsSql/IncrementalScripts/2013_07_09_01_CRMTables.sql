IF OBJECT_ID ('dbo.CustomerRelations') IS NOT NULL
	DROP TABLE dbo.CustomerRelations
GO
IF OBJECT_ID ('dbo.CRMActions') IS NOT NULL
	DROP TABLE dbo.CRMActions
GO
IF OBJECT_ID ('dbo.CRMStatuses') IS NOT NULL
	DROP TABLE dbo.CRMStatuses
GO
CREATE TABLE dbo.CRMStatuses
	(
	  Id INT IDENTITY NOT NULL
	, Name NVARCHAR(100)
	, CONSTRAINT PK_CRMStatuses PRIMARY KEY (Id)
	)
GO
CREATE INDEX IX_CRMStatusesName
	ON dbo.CRMStatuses (Name)
GO

CREATE TABLE dbo.CRMActions
	(
	  Id INT IDENTITY NOT NULL
	, Name NVARCHAR(100)
	, CONSTRAINT PK_CRMActions PRIMARY KEY (Id)
	)
GO

CREATE INDEX IX_CRMActions
	ON dbo.CRMActions (Name)
GO

CREATE TABLE dbo.CustomerRelations
	(
	  Id INT IDENTITY NOT NULL
	, CustomerId INT NOT NULL
	, UserName NVARCHAR(100)
	, Incoming Bit -- 1 is incoming, 0 is outgoing
	, ActionId INT
	, StatusId INT
	, Comment VARCHAR(1000)
	, Timestamp DateTime
	, CONSTRAINT PK_CustomerRelations PRIMARY KEY (Id)
	, CONSTRAINT FK_CustomerRelations_CRMActions FOREIGN KEY (ActionId) REFERENCES dbo.CRMActions (Id)
	, CONSTRAINT FK_CustomerRelations_CRMStatuses FOREIGN KEY (StatusId) REFERENCES dbo.CRMStatuses (Id)
	)
GO

INSERT INTO CRMStatuses VALUES ('NoSale')
INSERT INTO CRMStatuses VALUES ('Sale')
INSERT INTO CRMStatuses VALUES ('InProcess')
INSERT INTO CRMActions VALUES ('Call')
INSERT INTO CRMActions VALUES ('Chat')
INSERT INTO CRMActions VALUES ('Mail')
GO


