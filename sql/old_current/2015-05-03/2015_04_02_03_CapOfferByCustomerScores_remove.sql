SET QUOTED_IDENTIFIER ON
GO

DELETE
	MatrixColumns
FROM
	MatrixColumns c
	INNER JOIN MatrixRowTitles r ON c.MatrixRowID = r.MatrixRowID
	INNER JOIN Matrices m ON r.MatrixID = m.MatrixID
WHERE
	m.MatrixName = 'CapOfferByCustomerScores'
GO

DELETE
	MatrixRowTitles
FROM
	MatrixRowTitles r
	INNER JOIN Matrices m ON r.MatrixID = m.MatrixID
WHERE
	m.MatrixName = 'CapOfferByCustomerScores'
GO

DELETE FROM
	Matrices
WHERE
	MatrixName = 'CapOfferByCustomerScores'
GO
