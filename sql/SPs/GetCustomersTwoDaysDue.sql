IF OBJECT_ID('GetCustomersTwoDaysDue') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersTwoDaysDue AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetCustomersTwoDaysDue]
AS
BEGIN
	SELECT 
		ls.id, 
		ls.AmountDue, 
		c.FirstName, 
		c.Name AS Email, 
		convert(date, ls.Date) AS SceduledDate, 
		p.CardNo CreditCardNo,
		c.Id AS customerId
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
		LEFT JOIN PayPointCard p ON p.CustomerId=c.Id AND p.IsDefaultCard=1		
	WHERE 
		lo.AutoPayment IS NULL OR 
		lo.AutoPayment = 1
END

GO
