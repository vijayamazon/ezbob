IF OBJECT_ID('UpdateExperianBusiness') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateExperianBusiness AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateExperianBusiness
@CompanyRefNumber NVARCHAR(50),
@ExperianError NVARCHAR(MAX),
@ExperianScore INT,
@ExperianMaxScore INT,
@CustomerId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE MP_ExperianDataCache SET
		ExperianError = @ExperianError, 
		ExperianScore = @ExperianScore, 
		ExperianMaxScore = @ExperianMaxScore, 
		CustomerId = @CustomerId
	WHERE
		CompanyRefNumber = @CompanyRefNumber
END

GO