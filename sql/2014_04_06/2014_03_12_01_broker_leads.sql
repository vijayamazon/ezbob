-------------------------------------------------------------------------------
--
-- Create BrokerLeadDeletedReasons
--
-------------------------------------------------------------------------------

IF OBJECT_ID('BrokerLeadDeletedReasons') IS NULL
BEGIN
	CREATE TABLE BrokerLeadDeletedReasons (
		BrokerLeadDeletedReasonID INT IDENTITY(1, 1) NOT NULL,
		BrokerLeadDeletedReasonCode NVARCHAR(10) NOT NULL,
		BrokerLeadDeletedReason NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrokerLeadDeletedReasons PRIMARY KEY (BrokerLeadDeletedReasonID),
		CONSTRAINT UC_BrokerLeadDeletedReasons UNIQUE (BrokerLeadDeletedReasonCode),
		CONSTRAINT CHK_BrokerLeadDeletedReasons CHECK (LTRIM(RTRIM(BrokerLeadDeletedReasonCode)) != '' AND LTRIM(RTRIM(BrokerLeadDeletedReason)) != '')
	)
END
GO

-------------------------------------------------------------------------------
--
-- Fill BrokerLeadDeletedReasons
--
-------------------------------------------------------------------------------

SELECT
	BrokerLeadDeletedReasonCode,
	BrokerLeadDeletedReason
INTO
	#t
FROM
	BrokerLeadDeletedReasons

INSERT INTO #t (BrokerLeadDeletedReasonCode, BrokerLeadDeletedReason)
	VALUES ('SIGNEDUP', 'Customer signup complete')

INSERT INTO #t (BrokerLeadDeletedReasonCode, BrokerLeadDeletedReason)
	VALUES ('MANUAL', 'Broker decided to remove the lead')

INSERT INTO BrokerLeadDeletedReasons (BrokerLeadDeletedReasonCode, BrokerLeadDeletedReason)
SELECT
	#t.BrokerLeadDeletedReasonCode, #t.BrokerLeadDeletedReason
FROM
	#t
	LEFT JOIN BrokerLeadDeletedReasons r ON #t.BrokerLeadDeletedReasonCode = r.BrokerLeadDeletedReasonCode
WHERE
	r.BrokerLeadDeletedReasonCode IS NULL

DROP TABLE #t
GO

-------------------------------------------------------------------------------
--
-- Create BrokerLeadAddModes
--
-------------------------------------------------------------------------------

IF OBJECT_ID('BrokerLeadAddModes') IS NULL
BEGIN
	CREATE TABLE BrokerLeadAddModes (
		BrokerLeadAddModeID INT IDENTITY(1, 1) NOT NULL,
		BrokerLeadAddModeCode NVARCHAR(10) NOT NULL,
		BrokerLeadAddMode NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrokerLeadAddMode PRIMARY KEY (BrokerLeadAddModeID),
		CONSTRAINT UC_BrokerLeadAddMode UNIQUE (BrokerLeadAddModeCode),
		CONSTRAINT CHK_BrokerLeadAddMode CHECK (LTRIM(RTRIM(BrokerLeadAddModeCode)) != '' AND LTRIM(RTRIM(BrokerLeadAddMode)) != '')
	)
END
GO

-------------------------------------------------------------------------------
--
-- Fill BrokerLeadAddModes
--
-------------------------------------------------------------------------------

SELECT
	BrokerLeadAddModeCode,
	BrokerLeadAddMode
INTO
	#t
FROM
	BrokerLeadAddModes

INSERT INTO #t (BrokerLeadAddModeCode, BrokerLeadAddMode)
	VALUES ('INVITATION', 'Customer received invitation to sign up')

INSERT INTO #t (BrokerLeadAddModeCode, BrokerLeadAddMode)
	VALUES ('FILL', 'Broker filled all the customer details')

INSERT INTO BrokerLeadAddModes (BrokerLeadAddModeCode, BrokerLeadAddMode)
SELECT
	#t.BrokerLeadAddModeCode, #t.BrokerLeadAddMode
FROM
	#t
	LEFT JOIN BrokerLeadAddModes r ON #t.BrokerLeadAddModeCode = r.BrokerLeadAddModeCode
WHERE
	r.BrokerLeadAddModeCode IS NULL

DROP TABLE #t
GO

-------------------------------------------------------------------------------
--
-- Create BrokerLeads
--
-------------------------------------------------------------------------------

IF OBJECT_ID('BrokerLeads') IS NULL
BEGIN
	CREATE TABLE BrokerLeads (
		BrokerLeadID INT IDENTITY(1, 1) NOT NULL,
		BrokerID INT NOT NULL,
		CustomerID INT NULL,
		FirstName NVARCHAR(250) NOT NULL,
		LastName NVARCHAR(250) NOT NULL,
		Email NVARCHAR(128) NOT NULL,
		DateCreated DATETIME NOT NULL,
		BrokerLeadDeletedReasonID INT NULL,
		DateDeleted DATETIME NULL,
		BrokerLeadAddModeID INT NOT NULL,
		DateLastInvitationSent DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrokerLeads PRIMARY KEY (BrokerLeadID),
		CONSTRAINT FK_BrokerLead_Broker FOREIGN KEY (BrokerID) REFERENCES Broker(BrokerID),
		CONSTRAINT FK_BrokerLead_Error FOREIGN KEY (BrokerLeadDeletedReasonID) REFERENCES BrokerLeadDeletedReasons(BrokerLeadDeletedReasonID),
		CONSTRAINT FK_BrokerLead_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id),
		CONSTRAINT FK_BrokerLead_AddMode FOREIGN KEY (BrokerLeadAddModeID) REFERENCES BrokerLeadAddModes (BrokerLeadAddModeID),
		CONSTRAINT CHK_BrokerLead CHECK (LTRIM(RTRIM(FirstName)) != '' AND LTRIM(RTRIM(LastName)) != '' AND LTRIM(RTRIM(Email)) != '')
	)
END
GO
