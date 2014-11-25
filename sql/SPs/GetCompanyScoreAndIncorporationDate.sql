IF OBJECT_ID('GetCompanyScoreAndIncorporationDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyScoreAndIncorporationDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanyScoreAndIncorporationDate
@CustomerID INT,
@CompanyScore INT OUTPUT,
@IncorporationDate DATETIME OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

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
		SELECT
			@CompanyScore = (CASE
				WHEN a.MaxScore IS     NULL AND a.Score IS     NULL THEN 0
				WHEN a.MaxScore IS NOT NULL AND a.Score IS     NULL THEN a.MaxScore
				WHEN a.MaxScore IS     NULL AND a.Score IS NOT NULL THEN a.Score
				WHEN a.MaxScore < a.Score THEN a.MaxScore ELSE a.Score
			END),
			@IncorporationDate = a.IncorporationDate
		FROM
			CustomerAnalyticsCompany a
		WHERE
			a.CustomerID = @CustomerID
			AND
			a.IsActive = 1
	END
	ELSE BEGIN
		SELECT
			@CompanyScore = nl.CommercialDelphiScore,
			@IncorporationDate = nl.IncorporationDate
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
			INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
		WHERE
			c.Id = @CustomerID
			AND
			nl.IsActive = 1
	END

	SELECT @CompanyScore = ISNULL(@CompanyScore, 0)
END
GO
