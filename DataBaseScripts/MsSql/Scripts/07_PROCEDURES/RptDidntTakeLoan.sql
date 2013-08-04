IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDidntTakeLoan]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDidntTakeLoan]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptDidntTakeLoan
AS
BEGIN
	SET NOCOUNT ON

	IF OBJECT_ID('tempdb..#tmp1') IS NOT NULL
		DROP TABLE #tmp1

	IF OBJECT_ID('tempdb..#tmp2') IS NOT NULL
		DROP TABLE #tmp2

	SELECT
		IdCustomer,
		CreationDate,
		UnderwriterDecision,
		UnderwriterDecisionDate,
		ManagerApprovedSum,
		InterestRate,
		RepaymentPeriod,
		LoanTypeId
	INTO
		#tmp1
	FROM
		dbo.CashRequests

	SELECT
		L.CustomerId,
		SUM(L.LoanAmount) AS LoanAmount
	INTO
		#tmp2
	FROM
		Loan L
	GROUP BY
		L.CustomerId

	SELECT
		C.Id,
		C.Name,
		CONVERT(DATE, C.GreetingMailSentDate) AS SignUpDate,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		C.LimitedBusinessPhone,
		C.NonLimitedBusinessPhone,
		T1.UnderwriterDecisionDate AS ApprovedDate,
		T1.ManagerApprovedSum,
		T1.InterestRate,
		T1.RepaymentPeriod,
		CASE T1.LoanTypeId
			WHEN 2 THEN 'HalfWay Loan'
			WHEN 1 THEN 'Standard Loan'
		END LoanType
	FROM
		Customer C
		LEFT JOIN #tmp1 T1 ON T1.IdCustomer = C.Id
		LEFT JOIN #tmp2 T2 ON T2.CustomerId = C.Id
	WHERE
		C.IsTest = 0
		AND
		C.Name NOT like '%ezbob%'
		AND
		C.Name NOT LIKE '%liatvanir%'
		AND
		C.Name NOT LIKE '%test@%'
		AND
		T1.UnderwriterDecision = 'Approved'
		AND
		T2.LoanAmount IS NULL
		AND
		T1.ManagerApprovedSum IS NOT NULL
	GROUP BY
		C.Id,
		C.Name,
		C.GreetingMailSentDate,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		C.LimitedBusinessPhone,
		C.NonLimitedBusinessPhone,
		T1.UnderwriterDecision,
		T1.UnderwriterDecisionDate,
		T1.ManagerApprovedSum,
		T1.InterestRate,
		T1.RepaymentPeriod,
		T1.LoanTypeId

	DROP TABLE #tmp1
	DROP TABLE #tmp2

	SET NOCOUNT OFF
END
GO
