IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastCashRequestForPricingModel]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastCashRequestForPricingModel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastCashRequestForPricingModel] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE	@LatestCashRequest INT
		
	SELECT @LatestCashRequest = MAX(Id) FROM CashRequests WHERE IdCustomer = @CustomerId
	
	SELECT 
		CASE WHEN ManagerApprovedSum IS NULL THEN SystemCalculatedSum ELSE ManagerApprovedSum END AS ApprovedAmount,
		RepaymentPeriod
	FROM 
		CashRequests 
	WHERE 
		Id = @LatestCashRequest
END
GO
