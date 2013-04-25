IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOpenPayments]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOpenPayments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE RptOpenPayments
	@DateStart    DATETIME
	,@DateEnd      DATETIME
AS
BEGIN
	SELECT 
		C.Id,C.Name,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), L.LoanAmount))),1) LoanAmount,
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), L.InterestRate * 100))),1) InterestRate,
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), S.AmountDue))),1) AmountDue,
		S.Position + 1 AS Payment,
		S.[Date],
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), S.LoanRepayment))),1) LoanRepayment,
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), S.Interest))),1) Interest,
		convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), S.Fees))),1) Fees
	FROM 
		LoanSchedule S,
		Customer C,
		Loan L 
	WHERE 
		C.Id = L.CustomerId AND 
		L.Id = S.LoanId AND  
		S.[Date] <= @DateEnd AND 
		S.Status NOT IN ( 'PaidEarly','PaidOnTime') AND 
		C.IsTest = 0 AND 
		S.AmountDue > 0 AND 
		C.Id NOT IN (381,1216,492,1013,938,368,460,792,347,517,522,394)

	ORDER BY C.Surname
	
	
END
GO
