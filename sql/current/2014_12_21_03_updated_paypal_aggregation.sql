SET QUOTED_IDENTIFIER ON
GO

-------------------------------------------------------------------------------

DECLARE @tblid INT = OBJECT_ID('PayPalAggregation')

-------------------------------------------------------------------------------

SELECT
	name
INTO
	#n
FROM
	syscolumns
WHERE
	1 = 0

-------------------------------------------------------------------------------

INSERT INTO #n (name) VALUES
	('AmountPerTransferIn'),
	('AmountPerTransferOut'),
	('GrossProfitMargin'),
	('RevenuePerTrasnaction')

-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = @tblid AND name IN (SELECT name FROM #n))
BEGIN
	ALTER TABLE PayPalAggregation DROP COLUMN TimestampCounter

	DECLARE @ColumnName NVARCHAR(128)
	DECLARE @Query NVARCHAR(1024)

	DECLARE drop_cur CURSOR FOR SELECT name FROM #n

	OPEN drop_cur

	FETCH NEXT FROM drop_cur INTO @ColumnName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Query = 'IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID(''PayPalAggregation'') AND name = ''' + @ColumnName + ''')' + CHAR(13) + CHAR(10) +
			'ALTER TABLE PayPalAggregation DROP COLUMN ' + @ColumnName + CHAR(13) + CHAR(10) +
			'ALTER TABLE PayPalAggregation ADD ' + @ColumnName + 'Numerator NUMERIC(18, 2) NOT NULL' + CHAR(13) + CHAR(10) +
			'ALTER TABLE PayPalAggregation ADD ' + @ColumnName + 'Denominator NUMERIC(18, 2) NOT NULL'

		EXECUTE(@Query)

		FETCH NEXT FROM drop_cur INTO @ColumnName
	END

	CLOSE drop_cur
	DEALLOCATE drop_cur

	ALTER TABLE PayPalAggregation ADD TimestampCounter ROWVERSION
END

-------------------------------------------------------------------------------

DROP TABLE #n
GO
