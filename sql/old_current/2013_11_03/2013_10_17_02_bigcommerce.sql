SELECT
	m.Name AS Vendor, f.Name AS FuncName, v.Name AS DataType, f.InternalId AS FuncID
INTO
	#tmp_func_list
FROM
	MP_MarketplaceType m, MP_AnalyisisFunction f, MP_ValueType v
WHERE
	m.Id = -1 AND f.Id = -1 AND v.Id = -1

-------------------------------------------------------------------------------
--
-- Configure marketplace here:
--
-------------------------------------------------------------------------------

DECLARE @MPID UNIQUEIDENTIFIER = 'A5FC4B43-EBB7-4C6B-BC23-3C162CB61996'
DECLARE @MPNAME NVARCHAR(50) = 'Bigcommerce'
DECLARE @Description NVARCHAR(50) = 'Bigcommerce.com'
DECLARE @IsActive BIT = 1
DECLARE @IsOffline BIT = 0

INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('B60B03E0-AB11-4952-97BA-D6F751EF795D', 'NumOfOrders',          @MPNAME, 'Integer')
INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('BDF566A9-62D9-4D56-B219-C3A09B3D8DD7', 'TotalSumOfOrders',     @MPNAME, 'Double')
INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('B68A64A4-40DA-45D1-9F70-B9010A97C3E7', 'AverageSumOfOrders',   @MPNAME, 'Double')
INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('C94C7DED-4B3B-4598-8D6F-4C6D75D93BFA', 'NumOfExpenses',        @MPNAME, 'Integer')
INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('C960B241-D67F-453D-BF04-647A0B9C752B', 'TotalSumOfExpenses',   @MPNAME, 'Double')
INSERT INTO #tmp_func_list (FuncID, FuncName, Vendor, DataType) VALUES ('C54B4BFD-B5E0-4D0A-8C4E-365EAEB3E584', 'AverageSumOfExpenses', @MPNAME, 'Double')

-------------------------------------------------------------------------------
--
-- End of marketplace configuration area.
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM MP_MarketplaceType WHERE InternalId = @MPID)
BEGIN
	INSERT INTO [dbo].[MP_MarketplaceType] ([Name], [InternalId], [Description]) -- , Active, IsOffline)
		VALUES (@MPNAME,  @MPID, @Description) -- , @IsActive, @IsOffline)
END

DELETE
	#tmp_func_list
FROM
	MP_AnalyisisFunction
WHERE
	#tmp_func_list.FuncID = MP_AnalyisisFunction.InternalId

INSERT INTO MP_AnalyisisFunction (MarketPlaceId, ValueTypeId, Name, InternalId, Description)
SELECT
	m.Id, v.Id, f.FuncName, f.FuncID, NULL
FROM
	#tmp_func_list f
	INNER JOIN MP_MarketplaceType m ON f.Vendor = m.Name
	INNER JOIN MP_ValueType v ON f.DataType = v.Name

DROP TABLE #tmp_func_list
go
