SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL68') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL68
GO

IF TYPE_ID('ExperianLtdDL68List') IS NOT NULL
	DROP TYPE ExperianLtdDL68List
GO

CREATE TYPE ExperianLtdDL68List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	SubsidiaryRegisteredNumber NVARCHAR(255) NULL,
	SubsidiaryStatus NVARCHAR(255) NULL,
	SubsidiaryLegalStatus NVARCHAR(255) NULL,
	SubsidiaryName NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL68
@Tbl ExperianLtdDL68List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL68 (
		ExperianLtdID,
		SubsidiaryRegisteredNumber,
		SubsidiaryStatus,
		SubsidiaryLegalStatus,
		SubsidiaryName
	) SELECT
		ExperianLtdID,
		SubsidiaryRegisteredNumber,
		SubsidiaryStatus,
		SubsidiaryLegalStatus,
		SubsidiaryName
	FROM @Tbl
END
GO


