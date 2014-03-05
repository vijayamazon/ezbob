IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomersTwoDaysDue]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomersTwoDaysDue]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomersTwoDaysDue]
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
			convert(date, ls.Date)  <= DateAdd(dd,2 ,GETUTCDATE()) AND 
			(
				ls.TwoDaysDueMailSent IS NULL OR 
				ls.TwoDaysDueMailSent = 0 OR 
				ls.TwoDaysDueMailSent = 2
			)
		LEFT JOIN LoanOptions lo ON 
			lo.LoanId = l.Id
	WHERE 
		lo.AutoPayment IS NULL OR 
		lo.AutoPayment = 1
END
GO
