SET QUOTED_IDENTIFIER ON
GO

DECLARE @CapOfferByCustomerScores NVARCHAR(64) = 'CapOfferByCustomerScores'

DECLARE @MatrixID BIGINT
DECLARE @RowID BIGINT

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM Matrices WHERE MatrixName = @CapOfferByCustomerScores)
BEGIN
	INSERT INTO Matrices(MatrixName, Description, MinRowTitleValue, MinColumnTitleValue) VALUES (
		@CapOfferByCustomerScores,
		'When medal calculation is complete cap offered amount by customer business score (row title) and customer personal score (column title). ' +
		'Table values are treated as percentage (i.e. are expected to be in range [0..1]. Value 1.2 means 120%.',
		NULL,
		NULL
	)

	SET @MatrixID = SCOPE_IDENTITY()

	--------------------------------------------------------------------------
	--
	-- Row
	--
	--------------------------------------------------------------------------

	INSERT INTO MatrixRowTitles (MatrixID, TitleValue) VALUES (@MatrixID, 0)
	SET @RowID = SCOPE_IDENTITY()

	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,    0,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  500,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  600,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  700,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID, NULL,    0)

	--------------------------------------------------------------------------
	--
	-- Row
	--
	--------------------------------------------------------------------------

	INSERT INTO MatrixRowTitles (MatrixID, TitleValue) VALUES (@MatrixID, 30)
	SET @RowID = SCOPE_IDENTITY()

	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,    0,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  500, 0.45)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  600, 0.50)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  700, 0.60)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID, NULL, 0.75)

	--------------------------------------------------------------------------
	--
	-- Row
	--
	--------------------------------------------------------------------------

	INSERT INTO MatrixRowTitles (MatrixID, TitleValue) VALUES (@MatrixID, 60)
	SET @RowID = SCOPE_IDENTITY()

	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,    0,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  500, 0.55)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  600, 0.65)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  700, 0.80)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID, NULL, 0.90)

	--------------------------------------------------------------------------
	--
	-- Row
	--
	--------------------------------------------------------------------------

	INSERT INTO MatrixRowTitles (MatrixID, TitleValue) VALUES (@MatrixID, NULL)
	SET @RowID = SCOPE_IDENTITY()

	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,    0,    0)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  500, 0.70)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  600, 0.85)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID,  700, 0.95)
	INSERT INTO MatrixColumns (MatrixRowID, TitleValue, CellValue) VALUES (@RowID, NULL, 1.00)

	--------------------------------------------------------------------------
END
GO
