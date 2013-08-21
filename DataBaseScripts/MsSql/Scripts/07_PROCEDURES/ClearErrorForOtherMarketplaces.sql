IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClearErrorForOtherMarketplaces]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ClearErrorForOtherMarketplaces]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE ClearErrorForOtherMarketplaces
@MarketPlacesWithErrors VARCHAR(500),
@CustomerId INT
AS
BEGIN
	DECLARE @Delimiter VARCHAR(500),
		@TempValue VARCHAR(500),
        @TempIntValue INT

	SET @Delimiter = ','

	CREATE TABLE #tmp
	(
        id INT
	)

	SET @MarketPlacesWithErrors = isnull(@MarketPlacesWithErrors,'')

	DECLARE @TempString VARCHAR(500),
		@Ordinal INT,
		@CharIndex INT

	SET @TempString = @MarketPlacesWithErrors
	SET @CharIndex = charindex(@Delimiter, @TempString)
	SET @Ordinal = 0

	WHILE @CharIndex != 0 
	BEGIN     
		SET @Ordinal += 1        
		SET @TempIntValue = 0
		SET @TempValue = substring(@TempString, 0, @CharIndex)
		SET @TempIntValue = convert(INT, @TempValue) 

		IF @TempIntValue IS NOT NULL AND @TempIntValue != 0
		BEGIN
			INSERT INTO #tmp VALUES (@TempIntValue)
		END

		SET @TempString = substring(@TempString, @CharIndex + 1, len(@TempString) - @CharIndex)     
		SET @CharIndex = charindex(@Delimiter, @TempString)
	END

	IF @TempString != '' 
	BEGIN
		SET @Ordinal += 1 
		SET @TempIntValue = 0
		SET @TempIntValue = convert(INT, @TempString) 

		IF @TempIntValue IS NOT NULL AND @TempIntValue != 0
		BEGIN
			INSERT INTO #tmp VALUES (@TempIntValue)
		END
	END

	UPDATE MP_CustomerMarketPlace SET UpdateError = NULL WHERE CustomerId = @CustomerId AND Id NOT IN (SELECT Id FROM #tmp)
	DROP TABLE #tmp
END
GO
