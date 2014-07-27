IF OBJECT_ID('GetCompanyIsLimited') IS NULL
	EXECUTE('CREATE PROCEDURE GetCompanyIsLimited AS SELECT 1')
GO

ALTER PROCEDURE GetCompanyIsLimited
	(@RefNumber NVARCHAR(50))
AS
BEGIN
	SET NOCOUNT ON;
	IF EXISTS
	(
		SELECT 1
		FROM ExperianNonLimitedResults 
		WHERE RefNumber = @RefNumber AND 
			IsActive = 1
	)
	BEGIN
		SELECT 0 AS IsLimited
	END
	ELSE
	BEGIN
		SELECT 1 AS IsLimited
	END
END
GO
