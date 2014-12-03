IF OBJECT_ID('UpdateCashRequestsReApproval') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateCashRequestsReApproval AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateCashRequestsReApproval
@CustomerId int,
@UnderwriterDecision nvarchar(50),
@ManagerApprovedSum decimal(18, 0),
@APR decimal(18, 0),
@RepaymentPeriod int, 
@InterestRate decimal(18, 4),
@UseSetupFee int,
@EmailSendingBanned int,
@LoanTypeId int,
@UnderwriterComment nvarchar(200),
@IsLoanTypeSelectionAllowed int,
@DiscountPlanId int,
@ExperianRating INT,
@LoanSourceId INT,
@IsCustomerRepaymentPeriodSelectionAllowed INT,
@UseBrokerSetupFee BIT,
@Now DATETIME,
@AutoDecisionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE
		@OfferStart DATETIME,
		@OfferValidUntil DATETIME

	SET @OfferStart = @Now

	DECLARE @ValidForHours INT

	SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name = 'OfferValidForHours'

	SET @OfferValidUntil = DATEADD(hh, @ValidForHours, @Now)

	UPDATE CashRequests SET  
		UnderwriterDecision = @UnderwriterDecision,
		UnderwriterDecisionDate = @Now,
		ManagerApprovedSum = @ManagerApprovedSum,
		APR = @APR,
		RepaymentPeriod = @RepaymentPeriod, 
		InterestRate = @InterestRate,
		UseSetupFee = @UseSetupFee, 
		EmailSendingBanned = @EmailSendingBanned,
		LoanTypeId  = @LoanTypeId,
		UnderwriterComment = @UnderwriterComment,
		IsLoanTypeSelectionAllowed= @IsLoanTypeSelectionAllowed,
		DiscountPlanId = @DiscountPlanId,
		ExpirianRating = @ExperianRating,
		IsCustomerRepaymentPeriodSelectionAllowed = @IsCustomerRepaymentPeriodSelectionAllowed,
		LoanSourceId = @LoanSourceId,
		UseBrokerSetupFee = @UseBrokerSetupFee,
		AutoDecisionID = @AutoDecisionID
	WHERE
		Id = (select MAX(id) from CashRequests where IdCustomer=@CustomerId)
				
	UPDATE Customer SET
		CreditSum = @ManagerApprovedSum
	WHERE
		Id = @CustomerId
END
GO
