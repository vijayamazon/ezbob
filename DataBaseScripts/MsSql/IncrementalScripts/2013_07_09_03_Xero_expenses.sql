-- tempdb is not used to avoid collation problems

CREATE TABLE temp_func_2013_07_09 (
	Vendor NVARCHAR(255) NOT NULL,
	FuncName NVARCHAR(256) NOT NULL,
	DataType NVARCHAR(50) NOT NULL,
	FuncID UNIQUEIDENTIFIER NOT NULL
)

INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Xero',     'NumOfExpenses',        'Integer', 'CBC5655B-6CC2-4D86-ABB7-E37057A5F892')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Xero',     'TotalSumOfExpenses',   'Double',  'CBA06EBD-E58E-4312-AF61-4AFD07A2C828')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Xero',     'AverageSumOfExpenses', 'Double',  'CA17192B-8B1D-4C15-9157-78CBDD318897')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Volusion', 'NumOfExpenses',        'Integer', 'C93A575D-F536-456E-8F8F-4D4C745FD868')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Volusion', 'TotalSumOfExpenses',   'Double',  'C45298CD-CAFF-49B0-BEE6-6E8E3281F63C')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Volusion', 'AverageSumOfExpenses', 'Double',  'CD2F52DA-8CDC-4302-B76F-562A44F3103A')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Play',     'NumOfExpenses',        'Integer', 'C58BC831-5BDB-414A-9C1E-A1EBB31DAC27')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Play',     'TotalSumOfExpenses',   'Double',  'C1CFA9FD-AEC9-443E-B04D-3C69F416BA7C')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Play',     'AverageSumOfExpenses', 'Double',  'C6FDF05A-022D-4F1C-A0E8-F598A220B7C0')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Shopify',  'NumOfExpenses',        'Integer', 'C2764AC3-B368-454F-872E-FF2B97A61092')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Shopify',  'TotalSumOfExpenses',   'Double',  'C3DBE341-1A0F-4076-B3F9-6C7460D82D37')
INSERT INTO temp_func_2013_07_09 (Vendor, FuncName, DataType, FuncID) VALUES ('Shopify',  'AverageSumOfExpenses', 'Double',  'CD649144-1638-423D-868A-0D7063EF8A7C')

DELETE
	temp_func_2013_07_09
FROM
	MP_AnalyisisFunction
WHERE
	temp_func_2013_07_09.FuncID = MP_AnalyisisFunction.InternalId

INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
SELECT
	m.Id, v.Id, f.FuncName, f.FuncID, NULL
FROM
	temp_func_2013_07_09 f
	INNER JOIN MP_MarketplaceType m ON f.Vendor = m.Name
	INNER JOIN MP_ValueType v ON f.DataType = v.Name

DROP TABLE temp_func_2013_07_09
GO
