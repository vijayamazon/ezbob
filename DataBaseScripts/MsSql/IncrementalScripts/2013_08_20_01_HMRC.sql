-- tempdb is not used to avoid collation problems

IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId =                                'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA')
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description]) VALUES ('HMRC', 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA', 'HMRC')
GO

CREATE TABLE temp_func_2013_08_20 (
	Vendor NVARCHAR(255) NOT NULL,
	FuncName NVARCHAR(256) NOT NULL,
	DataType NVARCHAR(50) NOT NULL,
	FuncID UNIQUEIDENTIFIER NOT NULL
)

INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'NumOfOrders',          'Integer', 'BC9E71F7-55C3-4F92-86A9-18EF000890F0')
INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'TotalSumOfOrders',     'Double',  'B81B7727-BBCF-4731-9975-CE402F94B8B3')
INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'AverageSumOfOrders',   'Double',  'BCA489F8-847A-46EC-BE2D-35ABFEDD0D76')
INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'NumOfExpenses',        'Integer', 'C6847E6E-D4DF-4FD9-B0B4-0BC0235DB428')
INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'TotalSumOfExpenses',   'Double',  'CCF67CC1-18F5-4547-86A7-5B159C2462A0')
INSERT INTO temp_func_2013_08_20 (Vendor, FuncName, DataType, FuncID) VALUES ('HMRC', 'AverageSumOfExpenses', 'Double',  'C206B422-CDB3-4361-89EC-673B5753FA06')

DELETE
	temp_func_2013_08_20
FROM
	MP_AnalyisisFunction
WHERE
	temp_func_2013_08_20.FuncID = MP_AnalyisisFunction.InternalId

INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
SELECT
	m.Id, v.Id, f.FuncName, f.FuncID, NULL
FROM
	temp_func_2013_08_20 f
	INNER JOIN MP_MarketplaceType m ON f.Vendor = m.Name
	INNER JOIN MP_ValueType v ON f.DataType = v.Name

DROP TABLE temp_func_2013_08_20
GO
