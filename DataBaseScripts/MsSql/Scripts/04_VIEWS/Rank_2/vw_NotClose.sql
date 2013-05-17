IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_NotClose]'))
DROP VIEW [dbo].[vw_NotClose]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*order by Id*/
create VIEW [dbo].[vw_NotClose]
AS
SELECT l.Id AS loanID
, l.CustomerId
, l.Date AS StartDate
, ISNULL(l.DateClosed, 0) as DateClose
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
, c.CollectionStatus
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
--LEFT OUTER JOIN dbo.CustomerAddressRelation AS car ON car.customerId = c.Id 
LEFT OUTER JOIN dbo.CustomerAddress AS ca ON ca.customerId = c.Id
LEFT OUTER JOIN dbo.LoanOptions AS lo ON lo.LoanId = l.Id
WHERE c.IsTest <> 1 and 
 (
 ((c.TypeOfBusiness = 'PShip3P' OR c.TypeOfBusiness = 'SoleTrader') AND ca.addressType = 5) OR 
 ((c.TypeOfBusiness = 'Limited' OR c.TypeOfBusiness = 'PShip' OR c.TypeOfBusiness = 'LLP')and  (ca.addressType = 3)) OR 
  (c.TypeOfBusiness = 'Entrepreneur' and  (ca.addressType = 1))
 )
GO
