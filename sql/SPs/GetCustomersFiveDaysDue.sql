IF OBJECT_ID('GetCustomersFiveDaysDue') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersFiveDaysDue AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetCustomersFiveDaysDue]
(@Now DATETIME)
AS
BEGIN
	SELECT 
		ls.id, 
		ls.AmountDue, 
		c.FirstName, 
		c.Name AS Email, 
		convert(date, ls.Date) AS SceduledDate, 
		p.CardNo AS CreditCardNo,
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
			ls.Date  >= @Now AND 
			convert(date, ls.Date)  <= DateAdd(dd,5 ,@Now) AND 
			(
				ls.FiveDaysDueMailSent IS NULL OR 
				ls.FiveDaysDueMailSent = 0
			)
		LEFT JOIN PayPointCard p ON p.CustomerId=c.Id AND p.IsDefaultCard=1	
		LEFT JOIN LoanOptions lo ON 
			lo.LoanId = l.Id
	WHERE 
		lo.AutoPayment IS NULL OR 
		lo.AutoPayment = 1
END

GO
