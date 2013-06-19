IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId = 'a386f349-8e41-4ba9-b709-90332466d42d')
BEGIN
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description])
		VALUES ('Shopify', 'a386f349-8e41-4ba9-b709-90332466d42d', 'Shopify.com')
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'B547E0B7-0C29-4172-8D35-C8D0F2966FA9')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'NumOfOrders', 'B547E0B7-0C29-4172-8D35-C8D0F2966FA9', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'a386f349-8e41-4ba9-b709-90332466d42d'
		AND
		v.InternalId = 'A35FA704-C79E-4AA1-AB4C-A47B0005A2DE'
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'BAF770CD-1A53-49BD-A9BF-087B1BB82F4C')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'TotalSumOfOrders', 'BAF770CD-1A53-49BD-A9BF-087B1BB82F4C', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'a386f349-8e41-4ba9-b709-90332466d42d'
		AND
		v.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'B1B0C576-1AAC-4CD1-BFA6-CDC081FCB306')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'AverageSumOfOrders', 'B1B0C576-1AAC-4CD1-BFA6-CDC081FCB306', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'a386f349-8e41-4ba9-b709-90332466d42d'
		AND
		v.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
END
GO

