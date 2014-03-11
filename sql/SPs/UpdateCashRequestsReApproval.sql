IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCashRequestsReApproval]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCashRequestsReApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequestsReApproval] 
	(@CustomerId int,
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
 @Now DATETIME)
AS
BEGIN
	declare @OfferStart Datetime, @OfferValidUntil datetime
set @OfferStart = @Now
DECLARE @ValidForHours INT
SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name='OfferValidForHours'
set   @OfferValidUntil = DATEADD(hh, @ValidForHours ,@Now)

UPDATE [dbo].[CashRequests]
   SET  
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
 UseBrokerSetupFee = @UseBrokerSetupFee 

 WHERE Id = (select MAX(id) from CashRequests
				where IdCustomer=@CustomerId)
				
UPDATE Customer SET CreditSum = @ManagerApprovedSum WHERE Id = @CustomerId

 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
