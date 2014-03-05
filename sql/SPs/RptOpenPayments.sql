IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptOpenPayments]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptOpenPayments]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptOpenPayments] 
	(@DateStart DATETIME,
@DateEnd   DATETIME)
AS
BEGIN
	SELECT
		C.Id,
		C.Name,
		C.FirstName,
		C.Surname,
		C.DaytimePhone,
		C.MobilePhone,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), L.LoanAmount)), 1) LoanAmount,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), L.InterestRate * 100)), 1) InterestRate,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.AmountDue)), 1) AmountDue,
		S.Position + 1 AS Payment,
		S.[Date],
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.LoanRepayment)), 1) LoanRepayment,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.Interest)), 1) Interest,
		CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), S.Fees)), 1) Fees
	FROM 
		LoanSchedule S
		INNER JOIN Loan L ON S.LoanId = L.Id
		INNER JOIN Customer C
			ON L.CustomerId = C.Id
			AND C.IsTest = 0
			AND C.Id NOT IN (381, 1216, 492, 1013, 938, 368, 460, 792, 347, 517, 522, 394)
	WHERE 
		S.[Date] < CONVERT(DATE, @DateEnd)
		AND
		S.Status NOT IN ('PaidEarly', 'PaidOnTime')
		AND
		S.AmountDue > 0
	ORDER BY
		C.Surname
END
GO
