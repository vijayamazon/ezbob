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
	SELECT Customer.Name AS eMail, Customer.FirstName AS FirstName, Customer.Surname AS SurName, ManagerApprovedSum AS MaxApproved 
	FROM CashRequests, Customer 
	WHERE IdCustomer = Customer.Id 
	AND UnderwriterDecision = 'Approved' 
	AND UnderwriterDecisionDate BETWEEN @DateStart AND @DateEnd 
	AND IdCustomer NOT IN(SELECT CustomerId FROM Loan WHERE CreationDate BETWEEN @DateStart AND @DateEnd) 
	AND ManagerApprovedSum IS NOT NULL 
	--AND OfferValidUntil < @DateEnd
	AND Customer.IsTest = 0
END
GO
