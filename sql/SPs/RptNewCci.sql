IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptNewCci]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptNewCci]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptNewCci] 
	(@DateStart DATE,
	 @DateEnd DATE)
AS
BEGIN
	SELECT
		Loan.CustomerId,
		Customer.FirstName,
		Customer.Surname,
		Customer.Name AS Email,
		CASE WHEN Customer.BrokerID IS NULL THEN 'No' ELSE 'Yes' END AS IsBroker,
		Loan.Id AS LoanId,
		Loan.LoanAmount,
		SUM(LoanRepayment) AS PrincipalOwed,
		CASE Loan.LoanSourceID WHEN 1 THEN 'No' ELSE 'Yes' END AS IsEu	
	FROM
		Loan,
		Customer,
		LoanSchedule
	WHERE
		Loan.CustomerId = Customer.Id AND
		Customer.CciMark = 1 AND
		Customer.IsTest = 0 AND
		Loan.Status != 'PaidOff' AND
		Loan.Id = LoanSchedule.LoanId AND
		LoanSchedule.Status != 'Paid' AND
		LoanSchedule.Status != 'PaidEarly' AND
		LoanSchedule.Status != 'PaidOnTime'
	GROUP BY
		Loan.CustomerId,
		Customer.FirstName,
		Customer.Surname,
		Customer.Name,
		Customer.BrokerID,
		Loan.Id,
		Loan.LoanAmount,
		Loan.LoanSourceID
END
GO
