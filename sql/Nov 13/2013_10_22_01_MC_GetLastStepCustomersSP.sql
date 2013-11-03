IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastStepCustomers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastStepCustomers]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE GetLastStepCustomers
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
SELECT DISTINCT
 c.Name AS eMail,
 c.FirstName AS FirstName,
 c.Surname AS SurName,
 CASE WHEN cr.ManagerApprovedSum IS NOT NULL THEN cr.ManagerApprovedSum ELSE cr.SystemCalculatedSum END AS MaxApproved
FROM
 CashRequests cr
 INNER JOIN Customer c ON cr.IdCustomer = c.Id AND c.IsTest = 0
 LEFT JOIN (
  SELECT DISTINCT
   l.CustomerId
  FROM
   Loan l
   INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
  WHERE
   CONVERT(DATE, l.Date) >= @DateStart
 ) lt ON c.Id = lt.CustomerId
WHERE
 lt.CustomerId IS NULL
 AND
 (
  (cr.IdUnderwriter IS NOT NULL AND cr.UnderwriterDecision = 'Approved')
  OR
  (cr.IdUnderwriter IS NULL AND cr.SystemDecision = 'Approve')
 )
 AND
 (
  (cr.IdUnderwriter IS NOT NULL AND CONVERT(DATE, cr.UnderwriterDecisionDate) = @DateStart)
  OR
  (cr.IdUnderwriter IS NULL AND CONVERT(DATE, cr.SystemDecisionDate) = @DateStart)
 )

END
GO

