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

	-- There is no historical data for this field.

	DECLARE @TypeOfBusiness NVARCHAR(100) = ISNULL((
		SELECT
			c.TypeOfBusiness
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID
	), '')

	------------------------------------------------------------------------------

	IF @TypeOfBusiness IN ('Limited', 'LLP')
	BEGIN
		SELECT TOP 1
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
			CustomerAnalyticsCompany a
		WHERE
			a.CustomerID = @CustomerID
			AND
			a.AnalyticsDate < @Now
		ORDER BY
			a.AnalyticsDate DESC
	END
	ELSE BEGIN
		SELECT TOP 1
			@CompanyScore = nl.CommercialDelphiScore,
			@IncorporationDate = nl.IncorporationDate
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
			INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
			INNER JOIN MP_ServiceLog l ON nl.ServiceLogId = l.Id
		WHERE
			c.Id = @CustomerID
			AND
			l.InsertDate < @Now
		ORDER BY
			l.InsertDate DESC
	END

	SELECT @CompanyScore = ISNULL(@CompanyScore, 0)
END
GO
