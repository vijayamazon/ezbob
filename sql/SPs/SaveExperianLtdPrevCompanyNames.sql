SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdPrevCompanyNames') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdPrevCompanyNames
GO

IF TYPE_ID('ExperianLtdPrevCompanyNamesList') IS NOT NULL
	DROP TYPE ExperianLtdPrevCompanyNamesList
GO

CREATE TYPE ExperianLtdPrevCompanyNamesList AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	DateChanged DATETIME NULL,
	OfficeAddress1 NVARCHAR(255) NULL,
	OfficeAddress2 NVARCHAR(255) NULL,
	OfficeAddress3 NVARCHAR(255) NULL,
	OfficeAddress4 NVARCHAR(255) NULL,
	OfficeAddressPostcode NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdPrevCompanyNames
@Tbl ExperianLtdPrevCompanyNamesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdPrevCompanyNames (
		ExperianLtdID,
		DateChanged,
		OfficeAddress1,
		OfficeAddress2,
		OfficeAddress3,
		OfficeAddress4,
		OfficeAddressPostcode
	) SELECT
		ExperianLtdID,
		DateChanged,
		OfficeAddress1,
		OfficeAddress2,
		OfficeAddress3,
		OfficeAddress4,
		OfficeAddressPostcode
	FROM @Tbl
END
GO


