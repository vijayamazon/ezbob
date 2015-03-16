IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.UpdateScoringResultsNew') AND type in (N'P', N'PC'))
DROP PROCEDURE dbo.UpdateScoringResultsNew
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE dbo.UpdateScoringResultsNew 
	(@CustomerId int,
	 @CreditResult varchar(100),
	 @SystemDecision varchar(50),
	 @Status varchar(250),
	 @Medal nvarchar(50),
	 @ApplyForLoan datetime,
	 @ValidFor datetime)
AS
BEGIN
	DECLARE 
		@ValidForHours INT
		
	IF @ApplyForLoan = GETUTCDATE() OR @ApplyForLoan IS NULL 
		SET @ApplyForLoan = GETUTCDATE()

	SELECT 
		@ValidForHours = CONVERT(INT, Value) 
	FROM 
		ConfigurationVariables 
	WHERE 
		Name='OfferValidForHours'

	IF @ValidFor <  DATEADD(hh,@ValidForHours ,@ApplyForLoan) OR @ValidFor IS NULL
		SET @ValidFor = DATEADD(hh,@ValidForHours ,GETUTCDATE())


	UPDATE Customer
	SET
		CreditResult = @CreditResult, 
		Status = @Status,
		SystemDecision = @SystemDecision,
		ApplyForLoan= @ApplyForLoan,
		ValidFor = @ValidFor, 
		MedalType= @Medal
	WHERE 
		Id = @CustomerId
END
GO
