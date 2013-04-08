IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptCustomerReport]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptCustomerReport]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptCustomerReport]
	@DateStart DATETIME
AS
BEGIN
    SELECT 
    	Name,
    	IsSuccessfullyRegistered,
    	AccountNumber,
    	Status,
    	parsename(convert(VARCHAR, (CONVERT(MONEY, CONVERT(DECIMAL(13,0), CreditSum))),1),2) CreditSum,
    	ReferenceSource 
    FROM 
    	Customer 
    WHERE 
    	GreetingMailSentDate >= @DateStart
END
GO
