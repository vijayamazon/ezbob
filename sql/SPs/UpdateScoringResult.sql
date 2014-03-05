IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateScoringResult]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateScoringResult]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateScoringResult] 
	(@UserId int,
 @CreditResult varchar(100),
 @SystemDecision varchar(50),
 @Status varchar(250),
 @Medal nvarchar(50),
 @IsEliminated int,
 @ApplyForLoan datetime,
 @ValidFor datetime)
AS
BEGIN
	if @ApplyForLoan = GETUTCDATE() or @ApplyForLoan is null 
		SET @ApplyForLoan = GETUTCDATE()

DECLARE @ValidForHours INT
SELECT @ValidForHours = CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name='OfferValidForHours'

if @ValidFor <  DATEADD(hh,@ValidForHours ,@ApplyForLoan) or @ValidFor is NULL
set @ValidFor = DATEADD(hh,@ValidForHours ,GETUTCDATE())


UPDATE [dbo].[Customer]
   SET  [CreditResult] = @CreditResult, 
		[Status] = @Status,
		[SystemDecision] = @SystemDecision,
		[ApplyForLoan]= @ApplyForLoan,
		[ValidFor] = @ValidFor, 
		[MedalType]= @Medal,
		[Eliminated]=@IsEliminated
 WHERE Id = @UserId



 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO
