IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomersForPayPoint]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCustomersForPayPoint]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCustomersForPayPoint]
AS
BEGIN
	DECLARE @Now DATETIME
	SET @Now = GETUTCDATE()
	
	SELECT 
		ls.id AS LoanScheduleId,
		ls.LoanId,
		c.FirstName,
		c.Fullname,
		c.Id AS CustomerId,
		c.Name AS Email,
		ISNULL(lo.ReductionFee, 1) AS ReductionFee,
		l.RefNum,
		CAST(
			CASE 
				WHEN 
					(SELECT MAX(ls1.Date) FROM LoanSchedule ls1 WHERE ls.LoanId = ls1.LoanId) = ls.date
				THEN 
					1 
				ELSE 
					0
			END
			AS BIT
			) AS LastInstallment
	INTO #GetCustomersForPayPoint
	FROM 
		Customer c 
		JOIN Loan l ON 
			l.CustomerId = c.Id 
		JOIN CustomerStatuses cs ON 
			cs.Id = c.CollectionStatus AND 
			cs.IsEnabled = 1
		LEFT JOIN LoanOptions lo ON 
			lo.LoanId = l.Id
		JOIN LoanSchedule ls ON 
			ls.LoanId = l.Id AND 
			(
				ls.Status = 'StillToPay' OR 
				ls.Status = 'Late'
			) AND 
			convert(date, ls.Date) <= @Now
	WHERE 
		(
			lo.AutoPayment IS NULL OR 
			lo.AutoPayment = 1
		) AND
		ls.Date =
		(
			SELECT 
				min(l1.Date)
			FROM 
				LoanSchedule l1
			WHERE 
				l1.LoanId =ls.LoanId AND 
				(
					l1.Status = 'StillToPay' OR 
					l1.Status = 'Late'
				) 
		)
		
	SELECT 
		LoanScheduleId,
		LoanId,
		FirstName,
		Fullname,
		CustomerId,
		Email,
		ReductionFee,
		RefNum,
		LastInstallment
	FROM 
		#GetCustomersForPayPoint
	WHERE 
		LoanScheduleId NOT IN (SELECT LoanScheduleId FROM PaymentRollover WHERE ExpiryDate > @Now) 
END
GO
