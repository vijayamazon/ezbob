SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCompanyTypeOfBusiness') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyTypeOfBusiness AS SELECT 1')
GO

ALTER PROCEDURE GetCompanyTypeOfBusiness
@CompanyID INT,
@TypeOfBusinessName NVARCHAR(50) OUTPUT
AS
BEGIN
	SET @TypeOfBusinessName = NULL

	SELECT
		@TypeOfBusinessName = TypeOfBusiness
	FROM
		Company
	WHERE
		Id = @CompanyID
END
GO
