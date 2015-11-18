IF OBJECT_ID('NL_LoanStatusGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_LoanStatusGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].NL_LoanStatusGet @LoanId INT AS 
BEGIN
	SELECT WasLate = CASE
						 WHEN
								(SELECT count(ch.Id)
								 FROM CustomerStatusHistory ch
								 JOIN CustomerStatuses cs ON ch.PreviousStatus=cs.Id
								 OR ch.NewStatus = cs.Id
								 WHERE cs.IsWarning =1 OR cs.IsDefault=1 OR cs.IsWarning=1 OR cs.IsDefault = 1) > 0 THEN 1
						 WHEN c.IsWasLate = 1 THEN 1
						 ELSE 0
					 END,
					 lh.Amount AS LoanAmount,
					 lst.LoanStatus,
					 l.Refnum AS RefNum,
					 lh.EventTime AS LoanDate
	FROM NL_loans l
	JOIN  (SELECT top 1 *   FROM NL_LoanHistory   WHERE LoanID = @LoanId   ORDER BY LoanHistoryID ASC) lh ON lh.LoanID = l.LoanID
	JOIN vw_NL_LoansCustomer v ON l.LoanID = v.LoanID
	JOIN Customer c ON c.id = v.CustomerID
	JOIN NL_LoanStatuses lst ON lst.LoanStatusID = l.LoanStatusID
	WHERE l.LoanId = @LoanId 
END