IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptArrearsLetter]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptArrearsLetter]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptArrearsLetter
@DateStart DATE,
@DateEnd DATE
AS
SELECT max(S.[Date]) PaymentDate,C.Id ClientId,L.Id LoanId,count(1) MissedPayments,C.Name,C.FirstName,C.Surname,sum(S.AmountDue) AmountDue
FROM LoanSchedule S,Customer C,Loan L 
WHERE 
	C.IsTest = 0 AND 
	C.Id = L.CustomerId AND 
	S.LoanId = L.Id AND 
--	dateadd(d,14,S.Date) >= @DateStart AND 
--	dateadd(d,14,S.Date) < @DateEnd AND 
	S.Status IN ('Late') AND 
	C.CollectionStatus NOT in (1,4) AND
	C.CciMark = 0
GROUP BY C.Id,L.Id,C.Name,C.FirstName,C.Surname
HAVING count(1) >= 2
ORDER BY PaymentDate desc
GO