IF OBJECT_ID('EzServiceActionStatus') IS NULL
BEGIN
	CREATE TABLE EzServiceActionStatus (
		ActionStatusID INT NOT NULL,
		ActionStatusName NVARCHAR(32) NOT NULL,
		ActionStatusDescription NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceActionStatus PRIMARY KEY (ActionStatusID),
		CONSTRAINT UC_EzServiceActionStatus UNIQUE (ActionStatusName),
		CONSTRAINT CHK_EzServiceActionStatus CHECK (ActionStatusName != '' AND ActionStatusDescription != '')
	)

	INSERT INTO EzServiceActionStatus (ActionStatusID, ActionStatusName, ActionStatusDescription) VALUES
		(1, 'In progress', 'Action started and is executed'),
		(2, 'Done', 'Action has completed successfully'),
		(3, 'Finished', 'Action has completed but its result is failure'),
		(4, 'Failed', 'Action has failed to complete'),
		(5, 'Terminated', 'Action has been terminated by other action'),
		(6, 'Unknown', 'Action status is unknown')
END
GO

IF OBJECT_ID('EzServiceActionHistory') IS NULL
BEGIN
	CREATE TABLE EzServiceActionHistory (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		EntryTime DATETIME NOT NULL CONSTRAINT DF_EzServiceActionHistoer_Time DEFAULT (GETDATE()),
		ServiceInstanceName NVARCHAR(32) NOT NULL,
		ActionID UNIQUEIDENTIFIER NOT NULL,
		IsSync BIT NOT NULL,
		ActionStatusID INT NOT NULL,
		CurrentThreadID INT NOT NULL,
		UnderlyingThreadID INT NOT NULL,
		Comment NTEXT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EzServiceActionHistory PRIMARY KEY (EntryID),
		CONSTRAINT FK_EzServiceActionHistory FOREIGN KEY (ActionStatusID) REFERENCES EzServiceActionStatus (ActionStatusID),
	)
END
GO

