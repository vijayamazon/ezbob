SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('EsignatureHistory') IS NULL
BEGIN
	CREATE TABLE EsignatureHistory (
		EventID BIGINT IDENTITY(1, 1) NOT NULL,
		EsignatureID BIGINT NOT NULL,
		EventTime            DATETIME NOT NULL,
		Description          NTEXT NULL,
		VersionKey           NVARCHAR(255) NULL,
		EventTypeID          INT NOT NULL,
		ActingUserEmail      NVARCHAR(255) NULL,
		ActingUserIp         NVARCHAR(255) NULL,
		ParticipantEmail     NVARCHAR(255) NULL,
		Comment              NTEXT NULL,
		Latitude             REAL NULL,
		Longitude            REAL NULL,
		SynchronizationKey   NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EsignatureHistory PRIMARY KEY (EventID),
		CONSTRAINT FK_EsignatureHistory_Signature FOREIGN KEY (EsignatureID) REFERENCES Esignatures(EsignatureID),
		CONSTRAINT FK_EsignatureHistory_EventType FOREIGN KEY (EventTypeID) REFERENCES EsignAgreementEventType(EventTypeID)
	)
END
GO
