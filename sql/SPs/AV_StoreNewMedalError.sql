SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_StoreNewMedalError') IS NULL
	EXECUTE('CREATE PROCEDURE AV_StoreNewMedalError AS SELECT 1')
GO

ALTER PROCEDURE AV_StoreNewMedalError
(@CustomerId INT
,@CalculationTime DATETIME
,@MedalType NVARCHAR(50)
,@Medal NVARCHAR(50)
,@Error NVARCHAR(500)
,@NumOfHmrcMps INT
,@CashRequestID BIGINT
,@Tag NVARCHAR(256)
,@NLCashRequestID BIGINT =NULL
)
AS 
BEGIN 
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @TagID BIGINT = NULL

	EXECUTE SaveOrGetDecisionTrailTag @Tag, @TagID OUTPUT

	------------------------------------------------------------------------------

	UPDATE MedalCalculationsAV SET IsActive = 0 WHERE IsActive = 1 AND CustomerId = @CustomerId

	------------------------------------------------------------------------------

	INSERT INTO MedalCalculationsAV (
		CustomerId, IsActive, CalculationTime, MedalType, Medal, Error, NumOfHmrcMps,
		CashRequestID, TrailTagID, NLCashRequestID
	) VALUES (
		@CustomerId, 1, @CalculationTime, @MedalType, @Medal, @Error, @NumOfHmrcMps,
		@CashRequestID, @TagID, @NLCashRequestID
	)

	------------------------------------------------------------------------------
END 
GO
