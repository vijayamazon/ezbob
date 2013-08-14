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
	SELECT DISTINCT c.Name AS eMail, c.FirstName AS FirstName, c.Surname AS SurName, cr.ManagerApprovedSum AS MaxApproved 
	FROM CashRequests cr, Customer c  
	WHERE cr.UnderwriterDecision = 'Approved'
	AND cr.IdCustomer = c.Id 
	AND UnderwriterDecisionDate BETWEEN @DateStart AND @DateEnd
	AND cr.Id IN (SELECT cr.Id FROM CashRequests cr EXCEPT SELECT l.RequestCashId FROM Loan l)
	AND cr.ManagerApprovedSum IS NOT NULL 
	AND c.IsTest = 0
END
GO
