IF OBJECT_ID('RptLoansForLsaCRM') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaCRM AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaCRM
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanID = l.RefNum,
		LoanInternalID = l.Id,
		crm.Type,
		EventTime = crm.Timestamp,
		Action = a.Name,
		Status = s.Name,
		Rank = r.Name,
		crm.PhoneNumber,
		crm.Comment
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN CustomerRelations crm ON l.CustomerID = crm.CustomerID
		INNER JOIN CRMActions a ON crm.ActionId = a.Id
		INNER JOIN CRMStatuses s ON crm.StatusId = s.Id
		LEFT JOIN CRMRanks r ON crm.RankId = r.Id
	ORDER BY
		l.RefNum,
		crm.Timestamp
END
GO
