IF OBJECT_ID('RptLoansForLsaSms') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaSms AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaSms
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LoanID = l.RefNum,
		s.DateSent,
		s.Sid,
		s.[To],
		s.Body
	FROM
		Loan l
		INNER JOIN loans_for_lsa lsa ON l.Id = lsa.LoanID
		INNER JOIN Customer c ON lsa.CustomerID = c.Id
		INNER JOIN SmsMessage s ON s.UserID = c.Id OR SUBSTRING(c.MobilePhone, 2, 10) = SUBSTRING(s.[To], 4, 10)
	WHERE
		s.Body IS NOT NULL
END
GO
