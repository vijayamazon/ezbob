UPDATE dbo.MP_MarketplaceGroup
SET DisplayName = 'e-Commerce accounts',
	Description='Link your e-Commerce services if you use one or some of these.'
WHERE Name='Common'
GO


UPDATE dbo.MP_MarketplaceGroup
SET DisplayName = 'Web stores',
	PriorityOnline = 6,
    PriorityOffline = 6,
	Description='Link any of these additional services if you use them.'
WHERE Name='Webstores'
GO


UPDATE dbo.MP_MarketplaceGroup
SET DisplayName = 'Accounting services',
	PriorityOnline = 5,
    PriorityOffline = 5,
    Description='Link your accounting service if you use one of these.'
WHERE Name='Accounting'
GO
