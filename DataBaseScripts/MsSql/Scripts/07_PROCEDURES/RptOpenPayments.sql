IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOpenPayments]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOpenPayments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*-------------------------------*/
Create PROCEDURE RptOpenPayments
	@DateStart    DATETIME
	--@DateEnd      DATETIME
AS
BEGIN

	--SELECT DISTINCT Status FROM LoanSchedule
	--SELECT C.Id,C.Name,C.FirstName,C.Surname,C.DaytimePhone,C.MobilePhone,C.MedalType,sum(S.AmountDue) Amount,min(S.[Date]) FirstDebt FROM LoanSchedule S,Customer C,Loan L WHERE C.Id = L.CustomerId AND L.Id = S.LoanId and S.[Date] <= @DateStart AND S.Status IN ( 'StillToPay','Late') AND C.IsTest = 0 AND S.AmountDue > 0 GROUP BY C.Id,C.Name,C.FirstName,C.Surname,C.DaytimePhone,C.MobilePhone,C.MedalType ORDER BY Amount desc
	SELECT 
		C.Id,C.Name,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		C.MedalType,
		L.LoanAmount,
		L.InterestRate * 100 InterestRate,
		S.AmountDue,
		S.Position + 1 AS Payment,
		S.[Date]
	FROM 
		LoanSchedule S,
		Customer C,
		Loan L 
	WHERE 
		C.Id = L.CustomerId AND 
		L.Id = S.LoanId and 
		S.[Date] <= @DateStart AND 
		S.Status NOT IN ( 'PaidEarly','PaidOnTime') AND 
		C.IsTest = 0 AND 
		S.AmountDue > 0 
	ORDER BY C.Surname 
	
	
END
GO
