SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('WhatsNewExcludeCustomerOrigin') IS NULL
BEGIN
	CREATE TABLE WhatsNewExcludeCustomerOrigin (
		EntryID BIGINT IDENTITY NOT NULL,
		WhatsNewId INT NOT NULL,
		CustomerOriginID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_WhatsNewExcludeCustomerOrigin PRIMARY KEY (EntryID),
		CONSTRAINT FK_WhatsNewExcludeCustomerOrigin_WhatsNew FOREIGN KEY (WhatsNewId) REFERENCES WhatsNew (Id),
		CONSTRAINT FK_WhatsNewExcludeCustomerOrigin_Origin FOREIGN KEY (CustomerOriginID) REFERENCES CustomerOrigin (CustomerOriginID),
		CONSTRAINT UC_WhatsNewExcludeCustomerOrigin UNIQUE (WhatsNewId, CustomerOriginID)
	)
END
GO
