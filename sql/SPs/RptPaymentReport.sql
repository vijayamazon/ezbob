IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaymentReport]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaymentReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaymentReport] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
AS
BEGIN
	SELECT
		LoanSchedule.Id,
		Customer.FirstName,
		Customer.Surname,
		Customer.Name,
		LoanSchedule.[Date],
		AmountDue
	FROM
		LoanSchedule
		INNER JOIN Loan ON Loan.Id = LoanSchedule.LoanId
		INNER JOIN Customer ON Customer.Id = Loan.CustomerId
	WHERE
		LoanSchedule.Status = 'StillToPay'
		AND
		Customer.IsTest = 0
		AND
		CONVERT(DATE, @DateStart) <= LoanSchedule.[Date] AND LoanSchedule.[Date] < CONVERT(DATE, @DateEnd)
	ORDER BY
		LoanSchedule.DATE
END
GO
