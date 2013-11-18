IF OBJECT_ID('UiControls') IS NULL
BEGIN
	CREATE TABLE UiControls (
		UiControlID INT IDENTITY(1, 1) NOT NULL,
		UiControlName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UiControls PRIMARY KEY (UiControlID),
		CONSTRAINT CHK_UiControls CHECK (UiControlName != '')
	)

	CREATE UNIQUE INDEX IDX_UiControls ON UiControls(UiControlName)

	CREATE TABLE UiActions (
		UiActionID INT IDENTITY(1, 1) NOT NULL,
		UiActionName NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_UiActions PRIMARY KEY (UiActionID),
		CONSTRAINT CHK_UiActions CHECK (UiActionName != '')
	)

	CREATE UNIQUE INDEX IDX_UiActions ON UiActions(UiActionName)

	CREATE TABLE BrowserVersions (
		BrowserVersionID INT IDENTITY(1, 1) NOT NULL,
		BrowserVersion NVARCHAR(1024) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_BrowserVersions PRIMARY KEY (BrowserVersionID),
		CONSTRAINT CHK_BrowserVersions CHECK (BrowserVersion != '')
	)

	CREATE UNIQUE INDEX IDX_BrowserVersions ON BrowserVersions(BrowserVersion)

	CREATE TABLE UiEvents (
		UiEventID BIGINT IDENTITY(1, 1) NOT NULL,
		UiControlID INT NOT NULL,
		UiActionID INT NOT NULL,
		EventTime DATETIME NOT NULL,
		ControlHtmlID NVARCHAR(255) NULL,
		BrowserVersionID INT NOT NULL,
		UserID INT NULL,
		EventRefNum BIGINT NOT NULL,
		EventArguments NTEXT NULL,
		RemoteIP NVARCHAR(64) NOT NULL,
		SessionCookie NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,

		CONSTRAINT PK_UiEvents PRIMARY KEY (UiEventID),
		CONSTRAINT FK_UiEvents_Control FOREIGN KEY (UiControlID) REFERENCES UiControls (UiControlID),
		CONSTRAINT FK_UiEvents_Action FOREIGN KEY (UiActionID) REFERENCES UiActions (UiActionID),
		CONSTRAINT FK_UiEvents_User FOREIGN KEY (UserID) REFERENCES Security_User(UserId),
		CONSTRAINT FK_UiEvents_BrowserVersion FOREIGN KEY (BrowserVersionID) REFERENCES BrowserVersions(BrowserVersionID),
		CONSTRAINT CHK_UiEvents CHECK (RemoteIP != '' AND SessionCookie != '')
	)

	CREATE UNIQUE INDEX IDX_UiEvents ON UiEvents (UserID, EventRefNum, SessionCookie)

	INSERT INTO UiActions (UiActionName) VALUES
		('click'),
		('change'),
		('checked'),
		('linked'),
		('focusin'),
		('focusout')
END
GO
