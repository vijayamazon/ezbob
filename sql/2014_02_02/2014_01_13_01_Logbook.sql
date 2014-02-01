IF OBJECT_ID('LogbookEntryTypes') IS NULL
BEGIN
	CREATE TABLE LogbookEntryTypes (
		LogbookEntryTypeID INT NOT NULL,
		LogbookEntryType NVARCHAR(32) NOT NULL,
		LogbookEntryTypeDescription NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LogbookEntryType PRIMARY KEY (LogbookEntryTypeID),
		CONSTRAINT CHK_LogbookEntryType CHECK (LogbookEntryType != ''),
		CONSTRAINT UC_LogbookEntryType UNIQUE (LogbookEntryType)
	)

	INSERT INTO LogbookEntryTypes (LogbookEntryTypeID, LogbookEntryType, LogbookEntryTypeDescription) VALUES
		(0, 'Other', 'Something else'),
		(1, 'Config', 'Configuration update'),
		(2, 'Release', 'Major release'),
		(3, 'Patch', 'Minor release')
END
GO

IF OBJECT_ID('Logbook') IS NULL
BEGIN
	CREATE TABLE Logbook (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		LogbookEntryTypeID INT NOT NULL,
		EntryTime DATETIME NOT NULL CONSTRAINT DF_Logbook_Date DEFAULT (GETDATE()),
		UserID INT NOT NULL,
		EntryContent NTEXT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_Logbook PRIMARY KEY (EntryID),
		CONSTRAINT FK_Logbook_Entry FOREIGN KEY (LogbookEntryTypeID) REFERENCES LogbookEntryTypes (LogbookEntryTypeID),
		CONSTRAINT FK_Logbook_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId)
	)
END
GO

IF OBJECT_ID('UwGridLogbook') IS NULL
	EXECUTE('CREATE PROCEDURE UwGridLogbook AS SELECT 1')
GO

ALTER PROCEDURE UwGridLogbook
@WithTest BIT
AS
	SELECT
		l.EntryID,
		l.LogbookEntryTypeID,
		lt.LogbookEntryType,
		lt.LogbookEntryTypeDescription,
		l.EntryTime,
		l.UserID,
		u.UserName,
		u.FullName,
		l.EntryContent
	FROM
		Logbook l
		INNER JOIN LogbookEntryTypes lt ON l.LogbookEntryTypeID = lt.LogbookEntryTypeID
		INNER JOIN Security_User u ON l.UserID = u.UserId
	ORDER BY
		l.EntryTime DESC
GO

IF OBJECT_ID('LogbookAdd') IS NULL
	EXECUTE('CREATE PROCEDURE LogbookAdd AS SELECT 1')
GO

ALTER PROCEDURE LogbookAdd
@LogbookEntryTypeID INT,
@UserID INT,
@EntryContent NTEXT
AS
BEGIN
	INSERT INTO Logbook (LogbookEntryTypeID, UserID, EntryContent)
		VALUES (@LogbookEntryTypeID, @UserID, @EntryContent)

	RETURN SCOPE_IDENTITY()
END
GO

IF OBJECT_ID('LogbookEntryTypeList') IS NULL
	EXECUTE('CREATE PROCEDURE LogbookEntryTypeList AS SELECT 1')
GO

ALTER PROCEDURE LogbookEntryTypeList
AS
	SELECT
		lt.LogbookEntryTypeID,
		lt.LogbookEntryType,
		lt.LogbookEntryTypeDescription
	FROM
		LogbookEntryTypes lt
	ORDER BY
		CASE lt.LogbookEntryTypeID WHEN 0 THEN 1 ELSE 0 END,
		lt.LogbookEntryTypeDescription
GO
