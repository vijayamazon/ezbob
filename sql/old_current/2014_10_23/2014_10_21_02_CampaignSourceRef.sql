SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CampaignSourceRef') IS NULL
BEGIN
	CREATE TABLE CampaignSourceRef (
		Id INT NOT NULL IDENTITY(1,1),
		CustomerId INT NOT NULL,
		FUrl NVARCHAR(255) NULL,
		FSource NVARCHAR(255) NULL,
		FMedium NVARCHAR(255) NULL,
		FTerm NVARCHAR(255) NULL,
		FContent NVARCHAR(255) NULL,
		FName NVARCHAR(255) NULL,
		FDate DATETIME NULL,
		RUrl NVARCHAR(255) NULL,
		RSource NVARCHAR(255) NULL,
		RMedium NVARCHAR(255) NULL,
		RTerm NVARCHAR(255) NULL,
		RContent NVARCHAR(255) NULL,
		RName NVARCHAR(255) NULL,
		RDate DATETIME NULL,
		CONSTRAINT PK_CampaignSourceRef PRIMARY KEY (Id),
		CONSTRAINT FK_CampaignSourceRef_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customer(Id)
	)
END
GO
 