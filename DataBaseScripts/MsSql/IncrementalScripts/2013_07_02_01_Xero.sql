IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C')
BEGIN
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description])
		VALUES ('Xero', 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C', 'Xero.com')
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'B1676D81-160E-4AEA-BCDC-B5CD20812E3B')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'NumOfOrders', 'B1676D81-160E-4AEA-BCDC-B5CD20812E3B', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C'
		AND
		v.InternalId = 'A35FA704-C79E-4AA1-AB4C-A47B0005A2DE'
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'BE5D278C-5337-4C25-8EBA-537E67EFD133')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'TotalSumOfOrders', 'BE5D278C-5337-4C25-8EBA-537E67EFD133', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C'
		AND
		v.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
END
GO

IF NOT EXISTS (SELECT * FROM MP_AnalyisisFunction WHERE InternalId = 'B4EEAA5F-00CB-4AE9-B1C8-2123BA3A3FEC')
BEGIN
	INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
	SELECT
		m.Id, v.Id, 'AverageSumOfOrders', 'B4EEAA5F-00CB-4AE9-B1C8-2123BA3A3FEC', NULL
	FROM
		MP_MarketplaceType m,
		MP_ValueType v
	WHERE
		m.InternalId = 'AAFEBF1F-C4BD-4AFA-80ED-037AACFA392C'
		AND
		v.InternalId = '97594E98-6B09-46AB-83ED-618678B327BE'
END
GO

