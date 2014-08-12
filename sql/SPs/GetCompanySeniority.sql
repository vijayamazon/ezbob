IF OBJECT_ID('GetCompanySeniority') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanySeniority AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetCompanySeniority
@CustomerID INT, @IsLimited BIT
AS
BEGIN
	SET NOCOUNT ON;
	IF @IsLimited = 1
	BEGIN
		SELECT
			IncorporationDate
		FROM
			CustomerAnalyticsCompany
		WHERE
			CustomerID = @CustomerID
			AND
			IsActive = 1
	END		
	ELSE
	BEGIN
		SELECT TOP 1 e.IncorporationDate AS IncorporationDate 
		FROM Customer c
		INNER JOIN Company co ON c.CompanyId=co.Id
		INNER JOIN ExperianNonLimitedResults e ON e.RefNumber = co.ExperianRefNum
		WHERE c.Id=@CustomerID
		ORDER BY e.Created DESC 	
	END		
END

GO
