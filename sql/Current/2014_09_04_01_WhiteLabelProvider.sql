IF object_id('WhiteLabelProvider') IS NULL
BEGIN
CREATE TABLE WhiteLabelProvider(
	 Id INT NOT NULL IDENTITY(1,1)
	,Name NVARCHAR(50) NULL
	,Email NVARCHAR(300) NULL
	,Phone NVARCHAR(20) NULL
	,Logo NVARCHAR(MAX) NULL
	,LogoImageType NVARCHAR(30) NULL
	,LogoWidthPx INT NULL
	,LogoHeightPx INT NULL
	,LeadingColor NVARCHAR(30) NULL
	,SecondoryColor NVARCHAR(30) NULL
	,FinishWizardText NVARCHAR(1000) NULL
	,MobilePhoneTextMessage NVARCHAR(160) NULL
	,FooterText NVARCHAR(1000) NULL
	,ConnectorsToEnable NVARCHAR(1000) NULL
	,CONSTRAINT PK_WhiteLabelProvider PRIMARY KEY (Id)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'WhiteLabelId' and Object_ID = Object_ID(N'Customer'))    
BEGIN
	ALTER TABLE Customer ADD WhiteLabelId INT NULL
	ALTER TABLE Customer ADD CONSTRAINT FK_Customer_WhiteLabelProvider FOREIGN KEY (WhiteLabelId) REFERENCES WhiteLabelProvider(Id)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'WhiteLabelId' and Object_ID = Object_ID(N'Broker'))    
BEGIN
	ALTER TABLE Broker ADD WhiteLabelId INT NULL
	ALTER TABLE Broker ADD CONSTRAINT FK_Broker_WhiteLabelProvider FOREIGN KEY (WhiteLabelId) REFERENCES WhiteLabelProvider(Id)
END 
GO
