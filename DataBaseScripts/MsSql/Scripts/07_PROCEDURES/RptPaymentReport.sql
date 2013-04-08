IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaymentReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaymentReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptPaymentReport]
	@DateStart DATETIME,
	@DateEnd DATETIME
AS
BEGIN
    SELECT
    	LoanSchedule.Id,
    	Customer.FirstName,
    	Customer.Surname,
    	Customer.Name, 
    	LoanSchedule.DATE,
    	AmountDue 
    FROM
    	LoanSchedule,
    	Loan,
    	Customer
    WHERE
    	LoanSchedule.DATE > @DateStart AND 
		LoanSchedule.DATE < @DateEnd AND
		Loan.Id = LoanSchedule.LoanId AND
		Customer.Id = Loan.CustomerId AND
		LoanSchedule.Status = 'StillToPay'
	ORDER BY 
		LoanSchedule.DATE
END
GO
