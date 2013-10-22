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
, CASE WHEN cs.Name != 'Default' THEN ISNULL(l.DateClosed, 0)
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
, c.MartialStatus
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
 , c.SortCode, l.IsDefaulted, lo.CaisAccountStatus, cs.IsEnabled, c.CollectionStatus, c.MartialStatus, lo.ManualCaisFlag, cs.Name
GO
