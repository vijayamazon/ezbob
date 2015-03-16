IF OBJECT_ID('Broker') IS NULL
	CREATE TABLE Broker (
		BrokerID INT IDENTITY(1, 1) NOT NULL,
		FirmName NVARCHAR(255) NOT NULL,
		FirmRegNum NVARCHAR(255) NULL,
		ContactName NVARCHAR(255) NOT NULL,
		ContactEmail NVARCHAR(255) NOT NULL,
		ContactMobile NVARCHAR(255) NOT NULL,
		ContactOtherPhone NVARCHAR(255) NULL,
		SourceRef NVARCHAR(255) NOT NULL,
		EstimatedMonthlyClientAmount DECIMAL(18, 4) NOT NULL,
		Password NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Broker PRIMARY KEY (BrokerID),
		CONSTRAINT UC_Broker_Firm UNIQUE (FirmName),
		CONSTRAINT UC_Broker_Email UNIQUE (ContactEmail),
		CONSTRAINT UC_Broker_Phone UNIQUE (ContactMobile),
		CONSTRAINT UC_Broker_SrcRef UNIQUE (SourceRef),
		CONSTRAINT CHK_Broker CHECK (
			LTRIM(RTRIM(FirmName)) != ''
			AND
			LTRIM(RTRIM(ContactName)) != ''
			AND
			LTRIM(RTRIM(ContactEmail)) != ''
			AND
			LTRIM(RTRIM(ContactMobile)) != ''
			AND
			LTRIM(RTRIM(Password)) != ''
		)
	)
GO
