IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCashRequests]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCashRequests]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCashRequests] 
(@CustomerId int,
 @SystemCalculatedAmount int,
 @ManagerApprovedSum int,
 @SystemDecision varchar(50),
 @MedalType varchar(50),
 @ScorePoints numeric(8,3),
 @ExpirianRating int,
 @AnualTurnover int,
 @InterestRate decimal(18,4))

AS
BEGIN
declare @SystemDecisionDate datetime
set @SystemDecisionDate = GETUTCDATE()

UPDATE [dbo].[CashRequests]
   SET  [IdCustomer] = @CustomerId, 
		[SystemCalculatedSum] = @SystemCalculatedAmount,
		[ManagerApprovedSum] = @ManagerApprovedSum,
		[SystemDecision] = @SystemDecision,
		[SystemDecisionDate]= @SystemDecisionDate, 
		[MedalType]= @MedalType,
		[ScorePoints]= @ScorePoints,
		[ExpirianRating] = @ExpirianRating,
		[AnualTurnover] = @AnualTurnover,
		[InterestRate] = @InterestRate
 WHERE Id = (select MAX(id) from CashRequests
				where IdCustomer=@CustomerId)



 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
