IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId =                                  'A660B9CC-8BB1-4A37-9597-507622AEBF9E')
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description]) VALUES ('Magento', 'A660B9CC-8BB1-4A37-9597-507622AEBF9E', 'Magento.com')
GO

IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId =                                     'AE0BC89A-9884-4025-9D96-2755A6CD10EE')
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description]) VALUES ('Prestashop', 'AE0BC89A-9884-4025-9D96-2755A6CD10EE', 'Prestashop.com')
GO

-- instead of CREATE TABLE to avoid collation conflicts
SELECT
	m.Name AS Vendor,
	f.Name AS FuncName,
	v.Name AS DataType,
	f.InternalId AS FuncID
INTO
	#tmp
FROM
	MP_AnalyisisFunction f
	INNER JOIN MP_MarketplaceType m ON 1 = 0
	INNER JOIN MP_ValueType v ON 0 = 1

INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'NumOfOrders',          'Integer', 'B712A57A-816C-46D6-9283-804390D715AC')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'TotalSumOfOrders',     'Double',  'B85A6035-59D4-4155-9EE2-AF925A9893D7')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'AverageSumOfOrders',   'Double',  'BCD30FB2-7C1D-4D50-A0AC-D11FD8477ED0')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'NumOfExpenses',        'Integer', 'C8DF63FC-7DB3-4F75-AD40-97A556225042')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'TotalSumOfExpenses',   'Double',  'C3669B1A-9B21-4C31-B535-C7E542DD8BCC')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Magento', 'AverageSumOfExpenses', 'Double',  'C06CCAA8-8946-4B4A-92C3-55AF0FFC2C35')

INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'NumOfOrders',          'Integer', 'BA962184-7F24-4C05-81F9-B8E1C9067F6B')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'TotalSumOfOrders',     'Double',  'B3ED3FC2-38CF-443B-8EAF-794D7F0FC341')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'AverageSumOfOrders',   'Double',  'BCADB388-8C7A-47F6-B6D4-348A390635E8')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'NumOfExpenses',        'Integer', 'C64761DB-D075-40A1-ABF7-E07FD07ED11C')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'TotalSumOfExpenses',   'Double',  'C2D5B62B-43CF-4FA1-9128-3242007401FB')
INSERT INTO #tmp (Vendor, FuncName, DataType, FuncID) VALUES ('Prestashop', 'AverageSumOfExpenses', 'Double',  'CCA07C01-88F6-4A53-8CAB-361489DD2E14')

DELETE
	#tmp
FROM
	MP_AnalyisisFunction
WHERE
	#tmp.FuncID = MP_AnalyisisFunction.InternalId

INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
SELECT
	m.Id, v.Id, f.FuncName, f.FuncID, NULL
FROM
	#tmp f
	INNER JOIN MP_MarketplaceType m ON f.Vendor = m.Name
	INNER JOIN MP_ValueType v ON f.DataType = v.Name

DROP TABLE #tmp
GO
