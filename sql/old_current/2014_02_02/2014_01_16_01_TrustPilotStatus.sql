IF OBJECT_ID('TrustPilotStatus') IS NULL
BEGIN
	CREATE TABLE TrustPilotStatus (
		TrustPilotStatusID INT NOT NULL,
		TrustPilotStatus NVARCHAR(32) NOT NULL,
		TrustPilotStatusDescription NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_TrustPilotStatus PRIMARY KEY (TrustPilotStatusID),
		CONSTRAINT CHK_TrustPilotStatus CHECK (TrustPilotStatus != '' AND TrustPilotStatusDescription != ''),
		CONSTRAINT UC_TrustPilotStatus UNIQUE (TrustPilotStatus)
	)

	INSERT INTO TrustPilotStatus(TrustPilotStatusID, TrustPilotStatus, TrustPilotStatusDescription) VALUES
		(0, 'Nether', 'Did not leave TrustPilot review'),
		(1, 'Claims', 'Claims that left TrustPilot review'),
		(2, 'Done', 'Confirmed TrustPilot review')
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'TrustPilotStatusID')
BEGIN
	ALTER TABLE Customer ADD TrustPilotStatusID INT NOT NULL CONSTRAINT DF_Customer_TrustPilotStatus DEFAULT (0)

	ALTER TABLE Customer ADD CONSTRAINT FK_Customer_TrustPilotStatus FOREIGN KEY (TrustPilotStatusID) REFERENCES TrustPilotStatus(TrustPilotStatusID)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'TrustPilotReviewEnabled')
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TrustPilotReviewEnabled', 'true', 'Enables/disables Trust Pilot review request when clicking "Request cash"')
GO
