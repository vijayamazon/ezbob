IF OBJECT_ID('GetCompanyScoreAndIncorporationDate') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyScoreAndIncorporationDate AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanyScoreAndIncorporationDate
@CustomerID INT,
@TakeMinScore BIT,
@CompanyScore INT OUTPUT,
@IncorporationDate DATETIME OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	EXECUTE GetCompanyScoreAndIncorporationDate @CustomerID, @TakeMinScore, NULL, @CompanyScore OUTPUT, @IncorporationDate OUTPUT
END
GO
