IF OBJECT_ID('GetNoLtdIncorporationDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetNoLtdIncorporationDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetNoLtdIncorporationDate
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		nl.IncorporationDate
	FROM
		Customer c
		INNER JOIN Company co ON c.CompanyId = co.Id
		INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
	WHERE
		c.Id = @CustomerID
		AND
		nl.IsActive = 1
END
GO
