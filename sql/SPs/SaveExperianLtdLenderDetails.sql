SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdLenderDetails') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdLenderDetails
GO

IF TYPE_ID('ExperianLtdLenderDetailsList') IS NOT NULL
	DROP TYPE ExperianLtdLenderDetailsList
GO

CREATE TYPE ExperianLtdLenderDetailsList AS TABLE (
	DL65ID BIGINT NOT NULL,
	LenderName NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdLenderDetails
@Tbl ExperianLtdLenderDetailsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdLenderDetails (
		DL65ID,
		LenderName
	) SELECT
		DL65ID,
		LenderName
	FROM @Tbl
END
GO


