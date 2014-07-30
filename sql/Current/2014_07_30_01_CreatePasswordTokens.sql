SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CreatePasswordTokens') IS NULL
BEGIN
	CREATE TABLE CreatePasswordTokens (
		TokenID UNIQUEIDENTIFIER NOT NULL,
		CustomerID INT NOT NULL,
		DateCreated DATETIME NOT NULL,
		DateAccessed DATETIME NULL,
		DateDeleted DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CreatePasswordTokens PRIMARY KEY (TokenID),
		CONSTRAINT FK_CreatePasswordTokens_Customer FOREIGN KEY (CustomerID) REFERENCES Security_User (UserId),
		CONSTRAINT UC_CreatePasswordTokens UNIQUE (CustomerID)
	)
END
GO
