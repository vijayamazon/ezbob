SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('AV_GetYodleeRevenues') IS NULL
	EXECUTE('CREATE PROCEDURE AV_GetYodleeRevenues AS SELECT 1')
GO

ALTER PROCEDURE AV_GetYodleeRevenues
@CustomerMarketplaceId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @YodleeRevenues DECIMAL(18,4) = 0
	DECLARE @IsParsedBank BIT = 0
	DECLARE @MinDate DATETIME
	DECLARE @MaxDate DATETIME
	DECLARE @TranDayCount INT

	EXECUTE GetYodleeRevenues
		@CustomerMarketplaceId, NULL, NULL,
		@YodleeRevenues OUTPUT, @IsParsedBank OUTPUT, @MinDate OUTPUT, @MaxDate OUTPUT, @TranDayCount OUTPUT

	SELECT
		@YodleeRevenues AS YodleeRevenues,
		@MinDate AS MinDate,
		@MaxDate AS MaxDate
END
GO
