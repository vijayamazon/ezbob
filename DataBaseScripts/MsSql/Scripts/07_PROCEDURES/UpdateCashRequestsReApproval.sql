IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCashRequestsReApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCashRequestsReApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequestsReApproval] 
(@CustomerId int,

 @ManagerApprovedSum decimal(18, 0),
 @APR decimal(18, 0),
 @RepaymentPeriod int, 
 @InterestRate decimal(18, 4),
 @UseSetupFee int, 
 @OfferValidDays int,
 @EmailSendingBanned int,
 @LoanTypeId int,
 @UnderwriterComment nvarchar(200))
 
 
AS
BEGIN

declare @OfferStart Datetime, @OfferValidUntil datetime
set @OfferStart = GETUTCDATE()
set   @OfferValidUntil = DATEADD(DD, @OfferValidDays ,GETUTCDATE())

UPDATE [dbo].[CashRequests]
   SET  
 ManagerApprovedSum = @ManagerApprovedSum,
 APR = @APR,
 RepaymentPeriod = @RepaymentPeriod, 
 InterestRate = @InterestRate,
 UseSetupFee = @UseSetupFee, 
 OfferStart = @OfferStart, 
 OfferValidUntil = @OfferValidUntil, 
 EmailSendingBanned = @EmailSendingBanned,
 LoanTypeId  = @LoanTypeId,
 UnderwriterComment = @UnderwriterComment

 WHERE Id = (select MAX(id) from CashRequests
				where IdCustomer=@CustomerId)

 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
