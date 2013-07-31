IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoansOverall]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RptLoansOverall]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptLoansOverall
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		x.[Date] LoanDate,
		x.Name eMail,
		x.FirstName + ' ' + x.Surname AS Name,
		SUM(x.Repaid) Repaid,
		SUM(x.NetoRepaid) AS PrincipalRepaid,
		SUM(x.StillToPay) AS PrincipalOutstanding,
		SUM(x.Given) AS OriginalLoanPrincipal,
		SUM(x.Fees) Fees,
		SUM(x.Interest) Interest,
		CASE
			WHEN SUM(x.StillToPay) = 0 THEN '0'
			ELSE '1'
		END AS LoanStatus
	FROM (
		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			SUM(lt.Amount) Repaid,
			SUM(lt.Amount) - SUM(lt.Fees) + SUM(lt.Interest) AS NetoRepaid,
			0 StillToPay,
			0 Given,
			SUM(lt.Fees) Fees,
			SUM(lt.Interest) Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON l.Id = lt.LoanId
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname

		UNION

		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			0 Repaid,
			0 NetoRepaid,
			0 StillToPay,
			SUM(lt.Amount) Given,
			SUM(lt.Fees) Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname

		UNION

		SELECT
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname,
			0 Repaid,
			0 NetoRepaid,
			SUM(ls.LoanRepayment) StillToPay,
			0 Given,
			0 Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanSchedule ls ON ls.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			ls.Status IN ('StillToPay', 'Late')
			AND
			c.IsTest = 0
		GROUP BY
			l.[Date],
			c.Name,
			c.FirstName,
			c.Surname
	) AS x
	GROUP BY
		x.[Date],
		x.Name,
		x.FirstName,
		x.Surname

	UNION

	SELECT
		NULL LoanDate,
		'' Name,
		'' Name,
		SUM(x.Repaid) Repaid,
		SUM(x.NetoRepaid) AS PrincipalRepaid,
		SUM(x.StillToPay) AS PrincipalOutstanding,
		SUM(x.Given) AS OriginalLoanPrincipal,
		SUM(x.Fees) Fees,
		SUM(x.Interest) Interest,
		'' AS LoanStatus
	FROM (
		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			SUM(lt.Amount) Repaid,
			SUM(lt.Amount) - SUM(lt.Fees) + SUM(lt.Interest) AS NetoRepaid,
			0 StillToPay,
			0 Given,
			SUM(lt.Fees) Fees,
			SUM(lt.Interest) Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PaypointTransaction'
			AND
			c.IsTest = 0

		UNION

		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			0 Repaid,
			0 NetoRepaid,
			0 StillToPay,
			SUM(lt.Amount) Given,
			SUM(lt.Fees) Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanTransaction lt ON lt.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			lt.Status = 'Done'
			AND
			lt.Type = 'PacnetTransaction'
			AND
			c.IsTest = 0

		UNION

		SELECT
			NULL LoanDate,
			'' Name,
			'' FirstName,
			'' Surname,
			0 Repaid,
			0 NetoRepaid,
			SUM(ls.LoanRepayment) StillToPay,
			0 Given,
			0 Fees,
			0 Interest
		FROM
			Loan l
			JOIN LoanSchedule ls ON ls.LoanId = l.Id
			JOIN Customer c ON l.CustomerId = c.Id
		WHERE
			ls.Status IN ('StillToPay', 'Late')
			AND
			c.IsTest = 0
	) AS x

	ORDER BY x.[Date]
END
GO
