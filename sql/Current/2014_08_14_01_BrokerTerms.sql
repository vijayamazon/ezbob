IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'TermsTextID' AND id = OBJECT_ID('BrokerTerms'))
BEGIN
	ALTER TABLE BrokerTerms ADD TermsTextID INT NULL

	ALTER TABLE BrokerTerms ADD CONSTRAINT FK_BrokerTerms_Text FOREIGN KEY (TermsTextID) REFERENCES BrokerTerms (BrokerTermsID)
END
GO

IF OBJECT_ID('TR_BrokerTerms') IS NOT NULL
	DROP TRIGGER TR_BrokerTerms
GO

UPDATE BrokerTerms SET
	TermsTextID = 1
GO

ALTER TABLE BrokerTerms ALTER COLUMN TermsTextID INT NOT NULL
GO

CREATE TRIGGER TR_BrokerTerms ON BrokerTerms
INSTEAD OF UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	RAISERROR('Updating table BrokerTerms is disabled.', 11, 1)
END
GO
