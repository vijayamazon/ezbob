IF OBJECT_ID('RptLoansForLsaExperian') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaExperian AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaExperian
AS
BEGIN
	SET NOCOUNT ON;

	;WITH coc AS (
		SELECT
			CustomerID = c.Id,
			CompanyNum = co.ExperianRefNum
		FROM
			Customer c
			LEFT JOIN Company co ON c.CompanyId = co.Id
	) SELECT
		LoanID = l.RefNum,
		ServiceType = sl.ServiceType,
		FetchTime = sl.InsertDate,
		ExperianRawData = sl.ResponseData
	FROM
		Loan l
		INNER JOIN loans_for_lsa lsa ON l.Id = lsa.LoanID
		INNER JOIN coc ON lsa.CustomerID = coc.CustomerID
		INNER JOIN MP_ServiceLog sl ON
			(sl.ServiceType IN ('AML A check','Consumer Request') AND sl.CustomerId = coc.CustomerID)
			OR
			(sl.ServiceType IN ('E-SeriesLimitedData','E-SeriesNonLimitedData') AND sl.CompanyRefNum = coc.CompanyNum)
END
GO
