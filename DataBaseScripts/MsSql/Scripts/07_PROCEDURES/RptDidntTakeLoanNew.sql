IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDidntTakeLoanNew]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RptDidntTakeLoanNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptDidntTakeLoanNew 
AS
BEGIN
	SET NOCOUNT ON

	if OBJECT_ID('tempdb..#temp1') is not NULL
	BEGIN
		DROP TABLE #temp1
	END

	if OBJECT_ID('tempdb..#temp2') is not NULL
	BEGIN
		DROP TABLE #temp2
	END

	if OBJECT_ID('tempdb..#temp3') is not NULL
	BEGIN
		DROP TABLE #temp3
	END
	 
	if OBJECT_ID('tempdb..#temp4') is not NULL
	BEGIN
		DROP TABLE #temp4
	END
	--------------------------------------------------

	--------- GET LATEST DATE OF APPLICATION ---------
	SELECT R.IdCustomer,max(R.CreationDate) AS MaxAppDate
	INTO #temp1
	FROM CashRequests R
	GROUP BY R.IdCustomer

	------------ GET APPROVED CLIENT LIST  -----------
	SELECT T.IdCustomer,T.MaxAppDate
	INTO #temp2
	FROM #temp1 T
	JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
	WHERE R.UnderwriterDecision = 'approved'
		  AND T.MaxAppDate = R.CreationDate

	SELECT  T.IdCustomer,C.Name,
			convert(DATE,C.GreetingMailSentDate) AS SignUpDate, 
			C.Fullname,
			C.DaytimePhone, 
			C.MobilePhone, 
			C.LimitedBusinessPhone,
			C.NonLimitedBusinessPhone, 
			T.MaxAppDate AS ApprovedDate,
			R.UnderwriterDecision,
			R.UnderwriterDecisionDate,
			R.ManagerApprovedSum,
			R.InterestRate,
			R.RepaymentPeriod,
			CASE
			WHEN R.LoanTypeId = 2 THEN 'HalfWay Loan'
			WHEN R.LoanTypeId = 1 THEN 'Standard Loan'
			END AS LoansType,
			R.Id AS CashReqId
		
	INTO #temp3 
	FROM #temp2 T
	JOIN Customer C ON C.Id = T.IdCustomer
	JOIN CashRequests R ON R.IdCustomer = T.IdCustomer
	WHERE C.IsTest!=1
		  AND C.Name NOT like '%ezbob%' 
		  AND C.Name NOT LIKE '%liatvanir%'
		  AND C.Name NOT LIKE '%test@%'
		  AND T.MaxAppDate = R.CreationDate	

	SELECT L.Id,L.[Date],L.LoanAmount,L.RequestCashId
	INTO #temp4
	FROM Loan L

	SELECT  T1.IdCustomer,
			T1.Name,
			T1.SignUpDate, 
			T1.Fullname,
			T1.DaytimePhone, 
			T1.MobilePhone, 
			T1.LimitedBusinessPhone,
			T1.NonLimitedBusinessPhone, 
			T1.ApprovedDate,
			T1.UnderwriterDecision,
			T1.UnderwriterDecisionDate,
			T1.ManagerApprovedSum,
			T1.InterestRate,
			T1.RepaymentPeriod,
			T1.LoansType
		
	FROM #temp3 T1
	LEFT JOIN #temp4 T2 ON T1.CashReqId = T2.RequestCashId
	WHERE T2.LoanAmount IS NULL 
	ORDER BY 9

	SET NOCOUNT OFF
END
GO
