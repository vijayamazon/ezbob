INSERT INTO MP_WhiteList (Name, MarketPlaceTypeGuid) VALUES ('orange06', (SELECT MP_MarketplaceType.InternalId FROM MP_MarketplaceType WHERE Name LIKE 'PayPoint'))
GO
