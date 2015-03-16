if exists(select * from sys.columns where Name = N'MartialStatus' and Object_ID = Object_ID(N'Customer'))
BEGIN
	EXEC sp_rename 'Customer.MartialStatus', 'MaritalStatus', 'COLUMN'
END
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoanStats_CashRequests]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptLoanStats_CashRequests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptLoanStats_CashRequests
AS
SELECT
	r.Id AS RequestID,
	r.LoanTypeId AS LoanTypeID,
	lt.Type AS LoanType,
	r.IsLoanTypeSelectionAllowed,
	CASE dp.IsDefault
		WHEN 1 THEN '0'
		ELSE dp.Name
	END AS DiscountPlanName,
	c.Id AS CustomerID,
	c.Fullname AS CustomerName,
	CASE
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END AS DecisionDate,
	ISNULL(CASE
		WHEN r.IdUnderwriter IS NULL
			THEN CASE
				WHEN r.UnderwriterComment = 'Auto Re-Approval' THEN r.ManagerApprovedSum
				ELSE r.SystemCalculatedSum
			END
		ELSE
			ISNULL(r.ManagerApprovedSum, r.SystemCalculatedSum)
	END, 0) AS ApprovedSum,
	r.InterestRate AS ApprovedRate,
	ISNULL(r.ExpirianRating, 0) AS CreditScore,
	ISNULL(r.AnualTurnover, 0) AS AnnualTurnover,
	r.MedalType,
	c.Gender,
	c.DateOfBirth,
	c.MaritalStatus,
	c.ResidentialStatus,
	c.TypeOfBusiness,
	c.ReferenceSource,
	ISNULL(lmt.LoanId, 0) AS LoanID,
	ISNULL(lmt.LoanAmount, 0) AS LoanAmount,
	ISNULL(lmt.Date, 'Jul 1 1976') AS LoanIssueDate,
	ISNULL(lmt.AgreementModel, '{ "Term": 0 }') AS AgreementModel
FROM
	CashRequests r
	INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	INNER JOIN LoanType lt ON r.LoanTypeId = lt.Id
	INNER JOIN DiscountPlan dp ON r.DiscountPlanId = dp.Id
	LEFT JOIN (
		SELECT
			l.Id AS LoanId,
			l.RequestCashId,
			ISNULL(ISNULL(mt.Amount, l.LoanAmount), 0) AS LoanAmount,
			l.Date,
			l.AgreementModel
		FROM
			Loan l
			INNER JOIN LoanTransaction mt
				ON l.Id = mt.LoanId
				AND mt.Type = 'PacnetTransaction'
				AND mt.Status = 'Done'
				AND mt.Description NOT LIKE 'Non-cash.%'
	) lmt ON r.Id = lmt.RequestCashId
WHERE
	(
		(r.IdUnderwriter IS NOT NULL AND r.UnderwriterDecision = 'Approved')
		OR
		(r.IdUnderwriter IS NULL AND r.SystemDecision = 'Approve')
	)
	AND
	r.CreationDate >= 'Sep 4 2012'
ORDER BY
	r.IdCustomer,
	CASE 
		WHEN r.IdUnderwriter IS NULL
			THEN r.SystemDecisionDate
		ELSE
			r.UnderwriterDecisionDate
	END


GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_NotClose]'))
DROP VIEW [dbo].[vw_NotClose]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create VIEW [dbo].[vw_NotClose]
AS
SELECT l.Id AS loanID
, l.CustomerId
, l.Date AS StartDate
, CASE WHEN cs.Name != 'Default' OR l.Status = 'PaidOff' THEN ISNULL(l.DateClosed, 0)
	ELSE max(csh.Timestamp) END AS DateClose
, ISNULL(l.MaxDelinquencyDays, 0) as MaxDelinquencyDays
, l.RepaymentsNum AS RepaymentPeriod
, l.Balance AS CurrentBalance
, c.Gender
, c.FirstName
, c.MiddleInitial
, c.Surname
, c.RefNumber
, ca.Line1
, ca.Line2
, ca.Line3
, ca.Town
, ca.County
, ca.Postcode
, c.DateOfBirth
, ld.lsdate as  MinLSDate
, LoanAmount.am AS LoanAmount
, LoanAmount.SceduledRepayments AS SceduledRepayments
, c.TypeOfBusiness as CompanyType
, c.LimitedRefNum
, c.NonLimitedRefNum
, c.CreditResult as CustomerState
, c.SortCode
, l.IsDefaulted
, lo.CaisAccountStatus
, convert(INT, cs.IsEnabled) AS CustomerStatusIsEnabled
, c.MaritalStatus
, lo.ManualCaisFlag
FROM         
(
  SELECT 
	  SUM(l.LoanAmount) AS am	  
     , l.Id
     , COUNT(ls.Id) as SceduledRepayments
 FROM dbo.LoanSchedule AS ls 
  LEFT OUTER JOIN dbo.Loan AS l ON l.Id = ls.LoanId
    GROUP BY l.Id
 ) AS LoanAmount

 LEFT OUTER JOIN dbo.Loan AS l ON l.Id = LoanAmount.Id 
 LEFT OUTER JOIN MinLoanSchedule as ld ON ld.Id = LoanAmount.Id 
 LEFT OUTER JOIN dbo.Customer AS c ON c.Id = l.CustomerId 
LEFT OUTER JOIN dbo.CustomerAddress AS ca ON ca.customerId = c.Id
LEFT OUTER JOIN dbo.LoanOptions AS lo ON lo.LoanId = l.Id
LEFT JOIN CustomerStatuses AS cs ON cs.Id = c.CollectionStatus
LEFT OUTER JOIN dbo.LoanTransaction AS lt ON lt.LoanId = l.Id AND lt.Status='Done' AND lt.Type = 'PaypointTransaction'
LEFT OUTER JOIN CustomerStatusHistory AS csh ON csh.CustomerId = c.Id
WHERE c.IsTest <> 1 and 
 (
 ((c.TypeOfBusiness = 'PShip3P' OR c.TypeOfBusiness = 'SoleTrader') AND ca.addressType = 5) OR 
 ((c.TypeOfBusiness = 'Limited' OR c.TypeOfBusiness = 'PShip' OR c.TypeOfBusiness = 'LLP')and  (ca.addressType = 3)) OR 
  (c.TypeOfBusiness = 'Entrepreneur' and  (ca.addressType = 1))
 )
 GROUP BY l.Id, l.CustomerId, l.[Date], l.DateClosed, l.MaxDelinquencyDays, l.RepaymentsNum, l.Balance, c.Gender, c.FirstName, c.MiddleInitial, c.Surname, c.RefNumber, ca.Line1, ca.Line2, ca.Line3, ca.Town, ca.County
 , ca.Postcode, c.DateOfBirth, ld.lsdate, LoanAmount.am, LoanAmount.SceduledRepayments, c.TypeOfBusiness, c.LimitedRefNum, c.NonLimitedRefNum, c.CreditResult
 , c.SortCode, l.IsDefaulted, lo.CaisAccountStatus, cs.IsEnabled, c.CollectionStatus, c.MaritalStatus, lo.ManualCaisFlag, cs.Name, l.Status
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_Fill' AND object_id = OBJECT_ID('Customer'))
BEGIN
	DROP INDEX Customer.IX_Customer_Fill
	CREATE NONCLUSTERED INDEX [IX_Customer_Filled] ON [dbo].[Customer] 
	(
		[WizardStep] ASC,
		[IsTest] ASC
	)
	INCLUDE ( [Id],
	[CreditResult],
	[FirstName],
	[MiddleInitial],
	[Surname],
	[DateOfBirth],
	[TimeAtAddress],
	[ResidentialStatus],
	[Gender],
	[MaritalStatus],
	[TypeOfBusiness],
	[DaytimePhone],
	[MobilePhone],
	[Fullname],
	[OverallTurnOver],
	[WebSiteTurnOver]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, 
	IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
	ON [PRIMARY]
END
GO

