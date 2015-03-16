IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Broker') AND name = 'UserID')
	ALTER TABLE Broker ADD UserID INT NULL
GO

IF EXISTS (SELECT * FROM Broker WHERE UserID IS NULL)
BEGIN
	INSERT INTO Security_User (UserName, FullName, Email, BranchId)
	SELECT
		ContactEmail, FirmName, ContactEmail, 0
	FROM
		Broker
	WHERE
		UserID IS NULL


	UPDATE Broker SET
		UserID = u.UserId
	FROM
		Broker b
		INNER JOIN Security_User u ON b.ContactEmail = u.Email
	WHERE
		b.UserID IS NULL
END
GO

ALTER TABLE Broker ALTER COLUMN UserID INT NOT NULL
GO

IF OBJECT_ID('FK_Broker_User') IS NULL
	ALTER TABLE Broker ADD CONSTRAINT FK_Broker_User FOREIGN KEY (UserID) REFERENCES Security_User(UserId)
GO
