SET QUOTED_IDENTIFIER ON
GO

IF object_id('RapprovalGetLastApproveTerms') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RapprovalGetLastApproveTerms AS SELECT 1')
END
GO

ALTER PROCEDURE RapprovalGetLastApproveTerms
  @LacrID INT
AS 
BEGIN
	SELECT 
		LoanTypeId LoanTypeID, 
		LoanSourceId LoanSourceID, 
		ManualSetupFeeAmount, 
		ManualSetupFeePercent, 
		CAST(UseSetupFee AS BIT) UseSetupFee, 
		UseBrokerSetupFee, 
		InterestRate, 
		RepaymentPeriod, 
		DiscountPlanId DiscountPlanID, 
		IsCustomerRepaymentPeriodSelectionAllowed
	FROM
		CashRequests 
	WHERE 
		Id=@LacrID
END
GO