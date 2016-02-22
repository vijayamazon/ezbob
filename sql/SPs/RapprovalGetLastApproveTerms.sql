SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RapprovalGetLastApproveTerms') IS NULL
	EXECUTE('CREATE PROCEDURE RapprovalGetLastApproveTerms AS SELECT 1')
GO

ALTER PROCEDURE RapprovalGetLastApproveTerms
@LacrID BIGINT
AS 
BEGIN
	SET NOCOUNT ON;

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
		IsCustomerRepaymentPeriodSelectionAllowed,
		BrokerSetupFeePercent,
		SpreadSetupFee = ISNULL(SpreadSetupFee, 0),
		ProductSubTypeID
	FROM
		CashRequests
	WHERE 
		Id = @LacrID
END
GO
