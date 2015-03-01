SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadCompanyDissolutionDate') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCompanyDissolutionDate AS SELECT 1')
GO

ALTER PROCEDURE LoadCompanyDissolutionDate
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		RowType = 'CompanyDissolutionDate',
		CompanyDissolutionDate = e.DissolutionDate
	FROM
		MP_ServiceLog l
		INNER JOIN ExperianLtd e ON l.Id = e.ServiceLogID
		INNER JOIN Company co ON l.CompanyRefNum = co.ExperianRefNum
		INNER JOIN Customer c ON co.Id = c.CompanyId
	WHERE
		c.Id = @CustomerID
		AND
		l.InsertDate < @Now
END
GO
