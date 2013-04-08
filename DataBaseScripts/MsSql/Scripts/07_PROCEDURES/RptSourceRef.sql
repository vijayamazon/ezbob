IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptSourceRef]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptSourceRef]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptSourceRef
	@DateStart    DATETIME,
	@DateEnd      DATETIME
AS
BEGIN
	SELECT ReferenceSource,parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), sum(LoanAmount)))),1),2) LoanAmount,count(1) NumOfSources
	FROM Customer,Loan 
	WHERE Loan.CustomerId = Customer.Id 
	AND GreetingMailSentDate >=@DateStart  
	AND GreetingMailSentDate < @DateEnd
	AND Customer.isTest = 0 
	GROUP BY ReferenceSource
END
GO
