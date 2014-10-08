SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------
--
-- Repetition types - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCronjobRepetitionTypes') IS NULL
BEGIN
	CREATE TABLE EzServiceCronjobRepetitionTypes (
		RepetitionTypeID INT NOT NULL,
		RepetitionType NVARCHAR(32) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCronjobRepetitionTypes PRIMARY KEY (RepetitionTypeID),
		CONSTRAINT UC_EzServiceCronjobRepetitionTypes UNIQUE (RepetitionType),
		CONSTRAINT CHK_EzServiceCronjobRepetitionTypes CHECK (LTRIM(RTRIM(RepetitionType)) != '')
	)
END
GO

-------------------------------------------------------------------------------
--
-- Repetition types - data
--
-------------------------------------------------------------------------------

CREATE TABLE #rt (ID INT, Name NVARCHAR(32))

INSERT INTO #rt (ID, Name) VALUES
	(1, 'Monthly'),
	(2, 'Daily'),
	(3, 'Every X minutes')

INSERT INTO EzServiceCronjobRepetitionTypes (RepetitionTypeID, RepetitionType)
SELECT
	ID, Name
FROM
	#rt
WHERE
	Name NOT IN (SELECT RepetitionType FROM EzServiceCronjobRepetitionTypes)

DROP TABLE #rt
GO

-------------------------------------------------------------------------------
--
-- Scheduled actions - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCrontab') IS NULL
BEGIN
	CREATE TABLE EzServiceCrontab (
		JobID BIGINT IDENTITY(1, 1) NOT NULL,
		ActionNameID INT NOT NULL,
		IsEnabled BIT NOT NULL,
		RepetitionTypeID INT NOT NULL,
		RepetitionTime DATETIME NOT NULL,
		LastStartTime DATETIME NULL,
		LastEndTime DATETIME NULL,
		LastActionStatusID INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCrontab PRIMARY KEY (JobID),
		CONSTRAINT FK_EzServiceCrontab_Name FOREIGN KEY (ActionNameID) REFERENCES EzServiceActionName (ActionNameID),
		CONSTRAINT FK_EzServiceCrontab_Repetition FOREIGN KEY (RepetitionTypeID) REFERENCES EzServiceCronjobRepetitionTypes (RepetitionTypeID),
		CONSTRAINT FK_EzServiceCrontab_Status FOREIGN KEY (LastActionStatusID) REFERENCES EzServiceActionStatus (ActionStatusID)
	)
END
GO

-------------------------------------------------------------------------------
--
-- Scheduled action argument types - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCronjobArgumentTypes') IS NULL
BEGIN
	CREATE TABLE EzServiceCronjobArgumentTypes (
		TypeID INT NOT NULL,
		TypeName NVARCHAR(32) NOT NULL,
		IsNullable BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCronjobArgumentTypes PRIMARY KEY (TypeID),
		CONSTRAINT UC_EzServiceCronjobArgumentTypes UNIQUE (TypeName, IsNullable),
		CONSTRAINT CHK_EzServiceCronjobArgumentTypes CHECK (LTRIM(RTRIM(TypeName)) != '')
	)
END
GO

-------------------------------------------------------------------------------
--
-- Scheduled action argument types - data
--
-------------------------------------------------------------------------------

CREATE TABLE #at (
	ID INT,
	Name NVARCHAR(32),
	IsNullable BIT
)

INSERT INTO #at (ID, Name, IsNullable) VALUES
	( 1, 'int', 0),
	( 2, 'int', 1),
	( 3, 'long', 0),
	( 4, 'long', 1),
	( 5, 'double', 0),
	( 6, 'double', 1),
	( 7, 'decimal', 0),
	( 8, 'decimal', 1),
	( 9, 'bool', 0),
	(10, 'bool', 1),
	(11, 'DateTime', 0),
	(12, 'DateTime', 1),
	(13, 'enum', 0),
	(14, 'enum', 1),
	(15, 'string', 1)

INSERT INTO EzServiceCronjobArgumentTypes (TypeID, TypeName, IsNullable)
SELECT
	a.ID, a.Name, a.IsNullable
FROM
	#at a
	LEFT JOIN EzServiceCronjobArgumentTypes t
		ON a.Name = t.TypeName
		AND a.IsNullable = t.IsNullable
WHERE
	t.TypeID IS NULL


DROP TABLE #at
GO

-------------------------------------------------------------------------------
--
-- Scheduled action arguments - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCronjobArguments') IS NULL
BEGIN
	CREATE TABLE EzServiceCronjobArguments (
		ArgumentID BIGINT IDENTITY(1, 1) NOT NULL,
		JobID BIGINT NOT NULL,
		SerialNo INT NOT NULL,
		ArgumentTypeID INT NOT NULL,
		TypeHint NVARCHAR(255) NULL,
		Value NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCronjobArguments PRIMARY KEY (ArgumentID),
		CONSTRAINT FK_EzServiceCronjobArguments_Task FOREIGN KEY (JobID) REFERENCES EzServiceCrontab (JobID),
		CONSTRAINT FK_EzServiceCronjobArguments_Type FOREIGN KEY (ArgumentTypeID) REFERENCES EzServiceCronjobArgumentTypes (TypeID),
		CONSTRAINT UC_EzServiceCronjobArguments UNIQUE (JobID, SerialNo)
	)
END
GO

-------------------------------------------------------------------------------
--
-- Scheduled action log - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCronjobLog') IS NULL
BEGIN
	CREATE TABLE EzServiceCronjobLog (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		JobID BIGINT NOT NULL,
		ActionNameID INT NOT NULL,
		EntryTime DATETIME NOT NULL,
		ActionStatusID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCronjobLog PRIMARY KEY (EntryID),
		CONSTRAINT PK_EzServiceCronjobLog_Schedule FOREIGN KEY (JobID) REFERENCES EzServiceCrontab (JobID),
		CONSTRAINT FK_EzServiceCronjobLog_Name FOREIGN KEY (ActionNameID) REFERENCES EzServiceActionName (ActionNameID),
		CONSTRAINT FK_EzServiceCronjobLog_Status FOREIGN KEY (ActionStatusID) REFERENCES EzServiceActionStatus (ActionStatusID)
	)
END
GO

-------------------------------------------------------------------------------
--
-- Scheduled action arguments log - definition
--
-------------------------------------------------------------------------------

IF OBJECT_ID('EzServiceCronjobLogArguments') IS NULL
BEGIN
	CREATE TABLE EzServiceCronjobLogArguments (
		ArgumentID BIGINT IDENTITY(1, 1) NOT NULL,
		EntryID BIGINT NOT NULL,
		SerialNo INT NOT NULL,
		ArgumentTypeID INT NOT NULL,
		TypeHint NVARCHAR(255) NULL,
		Value NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceCronjobLogArguments PRIMARY KEY (ArgumentID),
		CONSTRAINT FK_EzServiceCronjobLogArguments_Entry FOREIGN KEY (EntryID) REFERENCES EzServiceCronjobLog (EntryID),
		CONSTRAINT FK_EzServiceCronjobLogArguments_Type FOREIGN KEY (ArgumentTypeID) REFERENCES EzServiceCronjobArgumentTypes (TypeID),
		CONSTRAINT UC_EzServiceCronjobLogArguments UNIQUE (EntryID, SerialNo)
	)
END
GO
