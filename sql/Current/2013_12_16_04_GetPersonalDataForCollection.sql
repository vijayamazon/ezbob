IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetPersonalDataForCollection]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetPersonalDataForCollection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPersonalDataForCollection]
	(@LoanId INT) 
AS
BEGIN
	SELECT 
		c.Id AS CustomerId, 
		c.Name AS Email, 
		c.Fullname, 
		CONVERT(DATE, c.DateOfBirth) AS DateOfBirth, 
		c.MobilePhone, 
		c.DaytimePhone, 
		c.TypeOfBusiness, 
		c.Gender,
		c.NonLimitedBusinessPhone, 
		c.NonLimitedCompanyName, 
		c.NonLimitedRefNum, 
		c.LimitedBusinessPhone, 
		c.LimitedCompanyName, 
		c.LimitedRefNum,
		l.Date AS StartDate, 
		l.InterestRate, 
		lt.Name AS LoanType, 
		l.LoanAmount, 
		l.repayments AS AmountPaid, 
		l.Balance, 
		(
			SELECT 
				COUNT(1) 
			FROM 
				LoanSchedule ls 
			WHERE 
				ls.LoanId = l.id
		) AS RepaymentsNum,
		(
			SELECT 
				COUNT(1) 
			FROM 
				LoanSchedule ls 
			WHERE 
				ls.LoanId = l.id AND 
				ls.Status = 'Late'
		) AS LateRepaymentsNum,
		(
			SELECT 
				MIN(ls.Date) 
			FROM 
				LoanSchedule ls 
			WHERE 
				ls.LoanId = l.id AND 
				ls.Status = 'Late' 
		) AS MinDate
	FROM 
		Customer c
		LEFT JOIN Loan l ON 
			l.CustomerId = c.Id
		LEFT JOIN LoanType lt ON 
			lt.Id = l.LoanTypeId
	WHERE 
		l.id = @LoanId
END
GO
