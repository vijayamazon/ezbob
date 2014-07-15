SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdShareholders') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdShareholders
GO

IF TYPE_ID('ExperianLtdShareholdersList') IS NOT NULL
	DROP TYPE ExperianLtdShareholdersList
GO

CREATE TYPE ExperianLtdShareholdersList AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	DescriptionOfShareholder NVARCHAR(255) NULL,
	DescriptionOfShareholding NVARCHAR(255) NULL,
	RegisteredNumberOfALimitedCompanyWhichIsAShareholder NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdShareholders
@Tbl ExperianLtdShareholdersList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdShareholders (
		ExperianLtdID,
		DescriptionOfShareholder,
		DescriptionOfShareholding,
		RegisteredNumberOfALimitedCompanyWhichIsAShareholder
	) SELECT
		ExperianLtdID,
		DescriptionOfShareholder,
		DescriptionOfShareholding,
		RegisteredNumberOfALimitedCompanyWhichIsAShareholder
	FROM @Tbl
END
GO


