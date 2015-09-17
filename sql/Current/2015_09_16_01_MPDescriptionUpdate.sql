
UPDATE dbo.MP_MarketplaceType
SET Description = 'Financial documents'
WHERE InternalId = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908'
GO

UPDATE dbo.MP_MarketplaceType
SET Description = 'Bank account'
WHERE InternalId = '107DE9EB-3E57-4C5B-A0B5-FFF445C4F2DF'
GO

UPDATE dbo.MP_MarketplaceGroup
SET Description = 'Please link your VAT account or upload the last 5 quarters of your VAT reports.  If your accountant takes care of your VAT, we can contact them on your behalf once you’ve submitted your application.'
WHERE Name = 'VAT'
GO

UPDATE dbo.MP_MarketplaceType
SET Description = 'Rakuten.co.uk'
WHERE InternalId = 'A5E96D38-FD2E-4E54-9E0C-276493C950A6'
GO

UPDATE dbo.MP_MarketplaceGroup
SET DisplayName = 'Business bank statements'
WHERE Name = 'Bank'
GO