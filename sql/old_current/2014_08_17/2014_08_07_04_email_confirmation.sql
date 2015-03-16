IF OBJECT_ID('EmailConfirmationStates') IS NULL
BEGIN
	CREATE TABLE EmailConfirmationStates (
		EmailStateID INT NOT NULL,
		EmailState NVARCHAR(32) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EmailConfirmationStates PRIMARY KEY (EmailStateID),
		CONSTRAINT CHK_EmailConfirmationStates CHECK (LTRIM(RTRIM(EmailState)) != ''),
		CONSTRAINT UC_EmailConfirmationStates UNIQUE (EmailState)
	)

	INSERT INTO EmailConfirmationStates (EmailStateID, EmailState) VALUES
		(0, 'Unknown'),
		(1, 'Pending'),
		(2, 'Confirmed'),
		(3, 'Canceled'),
		(4, 'ManuallyConfirmed'),
		(5, 'ImplicitlyConfirmed')
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsFinal' AND id = OBJECT_ID('EmailConfirmationStates'))
	ALTER TABLE EmailConfirmationStates ADD IsFinal BIT NOT NULL CONSTRAINT DF_EmailConfirmationStates_IsFinal DEFAULT (0)
GO

IF OBJECT_ID('DF_EmailConfirmationStates_IsFinal') IS NOT NULL
BEGIN
	UPDATE EmailConfirmationStates SET IsFinal = 1 WHERE EmailStateID > 1

	ALTER TABLE EmailConfirmationStates DROP CONSTRAINT DF_EmailConfirmationStates_IsFinal
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'EmailStateID' AND id = OBJECT_ID('Security_User'))
BEGIN
	ALTER TABLE Security_User ADD EmailStateID INT NULL

	ALTER TABLE Security_User ADD CONSTRAINT FK_Security_User_EmailState FOREIGN KEY (EmailStateID) REFERENCES EmailConfirmationStates (EmailStateID)
END
GO

UPDATE Security_User SET
	EmailStateID = 0
WHERE
	EmailStateID IS NULL
GO

UPDATE Security_User SET
	EmailStateID = s.EmailStateID
FROM
	Security_User u
	INNER JOIN Customer c ON u.UserId = c.Id
	INNER JOIN EmailConfirmationStates s ON c.EmailState = s.EmailState
GO
