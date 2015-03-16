IF OBJECT_ID('BrokerMarketingFile') IS NULL
BEGIN
	CREATE TABLE BrokerMarketingFile (
		BrokerMarketingFileID INT IDENTITY(1, 1) NOT NULL,
		FileID NVARCHAR(64) NOT NULL,
		FileName NVARCHAR(255) NOT NULL,
		DisplayName NVARCHAR(255) NOT NULL,
		MimeType NVARCHAR(255) NOT NULL,
		IsActive BIT NOT NULL,
		SortPosition INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrokerMarketingFile PRIMARY KEY (BrokerMarketingFileID),
		CONSTRAINT UC_BrokerMarketingFile_ID UNIQUE (FileID),
		CONSTRAINT UC_BrokerMarketingFile_Name UNIQUE (FileName)
	)
END
GO

IF NOT EXISTS (SELECT * FROM BrokerMarketingFile WHERE FileID = 'customer-consent')
	INSERT INTO BrokerMarketingFile (FileID, FileName, DisplayName, MimeType, IsActive, SortPosition)
		VALUES ('customer-consent', 'credit.file.release.and.consent.doc', 'Credit file release and consent', 'application/msword', 1, 1)
GO

IF NOT EXISTS (SELECT * FROM BrokerMarketingFile WHERE FileID = 'brokers-presentation')
	INSERT INTO BrokerMarketingFile (FileID, FileName, DisplayName, MimeType, IsActive, SortPosition)
		VALUES ('brokers-presentation', 'brokers.presentation.pdf', 'Brokers presentation', 'application/pdf', 1, 2)
GO
