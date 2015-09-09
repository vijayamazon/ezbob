IF OBJECT_ID('GetCompanyHistoricalScoreAndIncorporationDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyHistoricalScoreAndIncorporationDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanyHistoricalScoreAndIncorporationDate
@CustomerID INT,
@TakeMinScore BIT,
@Now DATETIME,
@CompanyScore INT OUTPUT,
@IncorporationDate DATETIME OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		@CompanyScore = (CASE
			WHEN a.MaxScore IS     NULL AND a.Score IS     NULL THEN 0
			WHEN a.MaxScore IS NOT NULL AND a.Score IS     NULL THEN a.MaxScore
			WHEN a.MaxScore IS     NULL AND a.Score IS NOT NULL THEN a.Score
			ELSE (
				CASE @TakeMinScore WHEN 1 THEN
					(CASE WHEN a.MaxScore < a.Score THEN a.MaxScore ELSE a.Score END)
				ELSE
					(CASE WHEN a.MaxScore > a.Score THEN a.MaxScore ELSE a.Score END)
				END
			)
		END),
		@IncorporationDate = a.IncorporationDate
	FROM
		dbo.udfGetCustomerCompanyAnalytics(@CustomerID, @Now, 0, 0, 1) a

	SELECT @CompanyScore = ISNULL(@CompanyScore, 0)
END
GO
