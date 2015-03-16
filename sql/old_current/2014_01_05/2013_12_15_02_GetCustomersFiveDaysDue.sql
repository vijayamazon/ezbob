IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.GetCustomersFiveDaysDue') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.GetCustomersFiveDaysDue
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.GetCustomersFiveDaysDue 
AS
BEGIN
	SELECT 
		ls.id, 
		ls.AmountDue, 
		c.FirstName, 
		c.Name AS Email, 
		convert(date, ls.Date) AS SceduledDate, 
		c.CreditCardNo
	FROM 
		Customer c 
		JOIN Loan l ON 
			l.CustomerId = c.Id 
		JOIN CustomerStatuses cs ON 
			cs.Id = c.CollectionStatus AND 
			cs.IsEnabled = 1 
		JOIN LoanSchedule ls ON 
			ls.LoanId = l.Id AND 
			ls.Status = 'StillToPay' AND 
			ls.AmountDue > 0 AND 
			ls.Date  >= GETUTCDATE() AND 
			convert(date, ls.Date)  <= DateAdd(dd,5 ,GETUTCDATE()) AND 
			(
				ls.FiveDaysDueMailSent IS NULL OR 
				ls.FiveDaysDueMailSent = 0
			)
		LEFT JOIN LoanOptions lo ON 
			lo.LoanId = l.Id
	WHERE 
		lo.AutoPayment IS NULL OR 
		lo.AutoPayment = 1
END
GO
