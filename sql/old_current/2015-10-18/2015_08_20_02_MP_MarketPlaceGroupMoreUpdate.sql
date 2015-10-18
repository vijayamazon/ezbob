IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('MP_MarketplaceGroup') AND name = 'Description')
BEGIN
	ALTER TABLE MP_MarketplaceGroup ADD Description NVARCHAR(2000)
END
GO

UPDATE dbo.MP_MarketplaceGroup
SET Description = 'If you are VAT registered, please link your VAT account or upload the last 5 quarters of your VAT reports.'
WHERE Name = 'VAT'
GO

UPDATE dbo.MP_MarketplaceGroup
SET Description = 'If you are not VAT registered or have additional income not included in your VAT submissions, either securely link your business bank account or upload the last 12 months of bank statements.'
WHERE Name = 'Bank'
GO

UPDATE dbo.MP_MarketplaceGroup
SET Description = 'Please upload your most recent accounts including profit & loss and balance sheet.'
WHERE Name = 'Documents'
GO
