CREATE TABLE #new_column_created (A INT NULL)
GO

-------------------------------------------------------------------------------
--
-- Create OriginID in Security_User.
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'OriginID' AND id = OBJECT_ID('Security_User'))
BEGIN
	INSERT INTO #new_column_created (A) VALUES (1)

	ALTER TABLE Security_User DROP COLUMN TimestampCounter

	ALTER TABLE Security_User ADD OriginID INT NULL

	ALTER TABLE Security_User ADD CONSTRAINT FK_Security_User_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin (CustomerOriginID)

	ALTER TABLE Security_User ADD Salt VARCHAR(255) NULL

	ALTER TABLE Security_User ADD CycleCount VARCHAR(255) NULL

	ALTER TABLE Security_User ADD TimestampCounter ROWVERSION
END
GO

-------------------------------------------------------------------------------
--
-- Update Security_User's unique constraint.
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM #new_column_created)
BEGIN
	ALTER TABLE Security_User DROP CONSTRAINT IX_SECURITY_USER

	--------------------------------------------------------------------------

	ALTER TABLE Security_User ADD CONSTRAINT UC_Security_User_Origin_Delete UNIQUE (
		UserName,
		DeleteId,
		OriginID
	)

END
GO

-------------------------------------------------------------------------------
--
-- Create new unique constraints.
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM #new_column_created)
BEGIN
	ALTER TABLE Security_User ADD CONSTRAINT UC_Security_User_Origin UNIQUE (
		UserName,
		OriginID
	)

	--------------------------------------------------------------------------

	ALTER TABLE Customer ADD CONSTRAINT UC_Customer_Origin UNIQUE (
		Name,
		OriginID
	)

	--------------------------------------------------------------------------

	ALTER TABLE Broker DROP CONSTRAINT UC_Broker_Email

	ALTER TABLE Broker ADD CONSTRAINT UC_Broker_Email UNIQUE (ContactEmail, OriginID)
END
GO

-------------------------------------------------------------------------------
--
-- Populate Security_User.OriginID field for customers and brokers.
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM #new_column_created)
BEGIN
	UPDATE Security_User SET
		OriginID = a.OriginID
	FROM
		Security_User u
		INNER JOIN Customer a ON u.UserID = a.Id

	--------------------------------------------------------------------------

	UPDATE Security_User SET
		OriginID = a.OriginID
	FROM
		Security_User u
		INNER JOIN Broker a ON u.UserID = a.BrokerID
END
GO

-------------------------------------------------------------------------------
--
-- Clean up.
--
-------------------------------------------------------------------------------

DROP TABLE #new_column_created
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'PasswordHashCycleCount')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'PasswordHashCycleCount',
		'Ug5/VvsG75w2SAVFjzP42A==',
		'Integer. How many times the password should be hashed before saving it to DB.',
		1
	)
END
GO
