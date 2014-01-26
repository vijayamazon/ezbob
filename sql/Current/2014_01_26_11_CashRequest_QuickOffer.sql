IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'QuickOfferID')
BEGIN
	ALTER TABLE CashRequests ADD QuickOfferID INT NULL
	ALTER TABLE CashRequests ADD CONSTRAINT FK_CashRequests_QuickOffer FOREIGN KEY (QuickOfferID) REFERENCES QuickOffer(QuickOfferID)
END
GO
