IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CampaignType]') AND type in (N'U'))
BEGIN
	CREATE TABLE CampaignType (
		Id INT IDENTITY(1,1) NOT NULL,
		Type NVARCHAR(300) NOT NULL, 
		Description NVARCHAR(300),
		CONSTRAINT PK_CampaignType PRIMARY KEY (Id)
	)
	
	INSERT INTO CampaignType (Type, Description) VALUES ( 'newsletter', NULL)
	INSERT INTO CampaignType (Type, Description) VALUES ( 'email', NULL)
	INSERT INTO CampaignType (Type, Description) VALUES ( 'email & phone', NULL)
	INSERT INTO CampaignType (Type, Description) VALUES ( 'other', NULL)
	INSERT INTO CampaignType (Type, Description) VALUES ( 'new customers', NULL)
	INSERT INTO CampaignType (Type, Description) VALUES ( 'first loans', NULL)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Campaign]') AND type in (N'U'))
BEGIN
	CREATE TABLE Campaign (
		Id INT IDENTITY(1,1) NOT NULL,
		TypeId INT NOT NULL,
		Name NVARCHAR(300),
		StartDate DATETIME,
		EndDate DATETIME,
		Description NVARCHAR(300),
		CONSTRAINT PK_Campaign PRIMARY KEY (Id),
		CONSTRAINT FK_Campaign_CampaignType FOREIGN KEY (TypeId) REFERENCES CampaignType (Id)
	)
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CampaignClients]') AND type in (N'U'))
BEGIN
	CREATE TABLE CampaignClients (
		Id INT IDENTITY(1,1) NOT NULL,
		CampaignId INT NOT NULL,
		CustomerId  INT NOT NULL,
		CONSTRAINT PK_CampaignClients PRIMARY KEY (Id),
		CONSTRAINT FK_CampaignClients_Campaign FOREIGN KEY (CampaignId) REFERENCES Campaign (Id),
		CONSTRAINT FK_CampaignClients_Customer FOREIGN KEY (CustomerId) REFERENCES Customer (Id)
	)
END
GO
