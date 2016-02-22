SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('UserEmailHistory') IS NULL
BEGIN
	CREATE TABLE UserEmailHistory (
		EventID BIGINT IDENTITY(1, 1) NOT NULL,
		EventTime DATETIME NOT NULL,
		ChangedByUserID INT NOT NULL,
		UserID INT NOT NULL,
		OldEmail NVARCHAR(250),
		NewEmail NVARCHAR(250),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UserEmailHistory PRIMARY KEY (EventID),
		CONSTRAINT FK_UserEmailHistory_Changer FOREIGN KEY (ChangedByUserID) REFERENCES Security_User(UserId),
		CONSTRAINT FK_UserEmailHistory_Changee FOREIGN KEY (UserID) REFERENCES Security_User(UserId)
	)
END
GO
