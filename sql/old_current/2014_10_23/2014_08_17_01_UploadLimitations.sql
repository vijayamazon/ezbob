SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UploadLimitations') IS NULL
BEGIN
	CREATE TABLE UploadLimitations (
		UploadLimitationID INT IDENTITY(1, 1) NOT NULL,
		ControllerName NVARCHAR(64) NULL,
		ActionName NVARCHAR(64) NULL,
		FileSizeLimit INT NULL, -- size in bytes
		AcceptedFiles NVARCHAR(1024) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UploadLimitations PRIMARY KEY (UploadLimitationID),
		CONSTRAINT UC_UploadLimitations UNIQUE (ControllerName, ActionName),
		CONSTRAINT CHK_UploadLimitations CHECK (
			(
				FileSizeLimit IS NULL OR FileSizeLimit > 0
			)
			AND (
				(ControllerName IS NULL AND ActionName IS NULL)
				OR
				(ControllerName IS NOT NULL AND ActionName IS NOT NULL)
			)
		)
	)

	INSERT INTO UploadLimitations (ControllerName, ActionName, FileSizeLimit, AcceptedFiles) VALUES
		(NULL, NULL, 30000000, 'image/*,application/pdf,.doc,.docx,.odt,.ppt,.pptx,.odp,.xls,.xlsx,.ods,.txt,.csv'),
		('BrokerHome', 'HandleUploadFile', 1000000, NULL),
		('CompanyFilesMarketPlaces', 'UploadedFiles', 1500000, NULL),
		('HmrcController', 'SaveFile', 600000, NULL),
		('UploadHmrcController', 'SaveFile', 700000, NULL),
		('AlertDocsController', 'UploadDoc', 2000000, NULL)
END
GO
