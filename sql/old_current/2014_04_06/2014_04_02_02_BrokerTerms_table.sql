IF OBJECT_ID('BrokerTerms') IS NULL
BEGIN
	CREATE TABLE BrokerTerms (
		BrokerTermsID INT NOT NULL,
		DateAdded DATETIME NOT NULL,
		BrokerTerms NTEXT NOT NULL,
		CONSTRAINT PK_BrokerTerms PRIMARY KEY(BrokerTermsID)
	)
END
GO

IF OBJECT_ID('TR_BrokerTerms') IS NOT NULL
	DROP TRIGGER TR_BrokerTerms
GO

CREATE TRIGGER TR_BrokerTerms ON BrokerTerms
INSTEAD OF UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	RAISERROR('Updating table BrokerTerms is disabled.', 11, 1)
END
GO
