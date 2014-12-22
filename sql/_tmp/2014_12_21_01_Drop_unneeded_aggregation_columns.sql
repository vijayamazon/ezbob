SELECT
	o.name AS TableName,
	c.name AS ColumnName
INTO
	#cols_to_drop
FROM
	syscolumns c
	INNER JOIN sysobjects o ON 1 = 0

-------------------------------------------------------------------------------

INSERT INTO #cols_to_drop (TableName, ColumnName) VALUES
	('EbayAggregation', 'InventoryTotalItems'),
	('EbayAggregation', 'InventoryTotalValue'),
	('EbayAggregation', 'TopCategories'),
	('HmrcAggregation', 'AverageSumOfExpensesDenominator'),
	('HmrcAggregation', 'AverageSumOfExpensesNumerator'),
	('HmrcAggregation', 'AverageSumOfOrdersDenominator'),
	('HmrcAggregation', 'AverageSumOfOrdersNumerator'),
	('HmrcAggregation', 'NumOfExpenses'),
	('HmrcAggregation', 'NumOfOrders'),
	('HmrcAggregation', 'TotalSumOfExpenses'),
	('HmrcAggregation', 'TotalSumOfOrders')

-------------------------------------------------------------------------------

DECLARE @TableName sysname
DECLARE @ColumnName NVARCHAR(128)
DECLARE @Query NVARCHAR(1024)

DECLARE drop_cur CURSOR FOR SELECT TableName, ColumnName FROM #cols_to_drop

OPEN drop_cur

FETCH NEXT FROM drop_cur INTO @TableName, @ColumnName

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @Query = 'IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID(''' + @TableName + ''') AND name = ''' + @ColumnName + ''')' + CHAR(13) + CHAR(10) +
		'ALTER TABLE ' + @TableName + ' DROP COLUMN ' + @ColumnName
 
	EXECUTE(@Query)

	FETCH NEXT FROM drop_cur INTO @TableName, @ColumnName
END

CLOSE drop_cur
DEALLOCATE drop_cur

DROP TABLE #cols_to_drop
GO

-------------------------------------------------------------------------------

SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

IF OBJECT_ID('EbayAggregationCategories') IS NULL
BEGIN
	CREATE TABLE EbayAggregationCategories (
		EbayAggregationCategoryID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerMarketPlaceUpdatingHistoryID INT NOT NULL,
		IsActive BIT NOT NULL,
		CategoryID INT NOT NULL,
		Listings INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_EbayAggregationCategories PRIMARY KEY (EbayAggregationCategoryID),
		CONSTRAINT FK_EbayAggregationCategories_CustomerMarketplaceUpdatingHistory FOREIGN KEY (CustomerMarketPlaceUpdatingHistoryID) REFERENCES MP_CustomerMarketPlaceUpdatingHistory (Id)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

SELECT
	o.name AS TableName,
	c.name AS ColumnName,
	c.name AS NewName
INTO
	#cols_to_rename
FROM
	syscolumns c
	INNER JOIN sysobjects o ON 1 = 0

-------------------------------------------------------------------------------

INSERT INTO #cols_to_rename (TableName, ColumnName, NewName) VALUES
	('FreeAgentAggregation', 'SumOfAdminExpensesCatery', 'SumOfAdminExpensesCategory'),
	('FreeAgentAggregation', 'SumOfCostOfSalesExpensesCatery', 'SumOfCostOfSalesExpensesCategory'),
	('FreeAgentAggregation', 'SumOfGeneralExpensesCatery', 'SumOfGeneralExpensesCategory')

-------------------------------------------------------------------------------

DECLARE @TableName sysname
DECLARE @ColumnName NVARCHAR(128)
DECLARE @NewName NVARCHAR(128)
DECLARE @Query NVARCHAR(1024)

DECLARE rename_cur CURSOR FOR SELECT TableName, ColumnName, NewName FROM #cols_to_rename

OPEN rename_cur

FETCH NEXT FROM rename_cur INTO @TableName, @ColumnName, @NewName

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @Query = 'IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID(''' + @TableName + ''') AND name = ''' + @ColumnName + ''')' + CHAR(13) + CHAR(10) +
		'EXECUTE sp_rename ''' + @TableName + '.' + @ColumnName + ''', ''' + @NewName + ''''

	EXECUTE(@Query)

	FETCH NEXT FROM rename_cur INTO @TableName, @ColumnName, @NewName
END

CLOSE rename_cur
DEALLOCATE rename_cur

DROP TABLE #cols_to_rename
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Business') and name = 'BelongsToCustomer')
	ALTER TABLE Business ADD BelongsToCustomer BIT NULL
GO
