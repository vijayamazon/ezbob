IF NOT EXISTS (SELECT * FROM QuickOfferConfiguration)
	INSERT INTO QuickOfferConfiguration (ID) VALUES (1)
GO

UPDATE QuickOfferConfiguration SET Enabled = 0
GO
