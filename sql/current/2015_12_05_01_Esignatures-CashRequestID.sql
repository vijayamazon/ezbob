SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CashRequestID' AND id = OBJECT_ID('Esignatures'))
BEGIN
	ALTER TABLE Esignatures DROP COLUMN TimestampCounter

	ALTER TABLE Esignatures ADD CashRequestID BIGINT NULL
	
	ALTER TABLE Esignatures ADD CONSTRAINT FK_Esignatures_CashRequestID FOREIGN KEY (CashRequestID) REFERENCES CashRequests (Id)

	ALTER TABLE Esignatures ADD TimestampCounter ROWVERSION
END
GO
