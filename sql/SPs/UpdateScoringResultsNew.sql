IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateScoringResultsNew]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateScoringResultsNew]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateScoringResultsNew] 
	(@CustomerId INT,
	 @CreditResult VARCHAR(100),
	 @SystemDecision VARCHAR(50),
	 @Status VARCHAR(250),
	 @Medal NVARCHAR(50),
	 @ValidFor DATETIME,
	 @Now DATETIME)
AS
BEGIN
	DECLARE 
		@ValidForHours INT

	SELECT 
		@ValidForHours = CONVERT(INT, Value) 
	FROM 
		ConfigurationVariables 
	WHERE 
		Name='OfferValidForHours'

	IF @ValidFor <  DATEADD(hh,@ValidForHours ,@Now) OR @ValidFor IS NULL
		SET @ValidFor = DATEADD(hh,@ValidForHours ,@Now)

	UPDATE Customer
	SET
		CreditResult = @CreditResult, 
		Status = @Status,
		SystemDecision = @SystemDecision,
		ApplyForLoan= @Now,
		ValidFor = @ValidFor, 
		MedalType= @Medal
	WHERE 
		Id = @CustomerId
END
GO
