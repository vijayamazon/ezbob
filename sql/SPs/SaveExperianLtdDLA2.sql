SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDLA2') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDLA2
GO

IF TYPE_ID('ExperianLtdDLA2List') IS NOT NULL
	DROP TYPE ExperianLtdDLA2List
GO

CREATE TYPE ExperianLtdDLA2List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	Date DATETIME NULL,
	NumberOfEmployees INT NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDLA2
@Tbl ExperianLtdDLA2List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDLA2 (
		ExperianLtdID,
		Date,
		NumberOfEmployees
	) SELECT
		ExperianLtdID,
		Date,
		NumberOfEmployees
	FROM @Tbl
END
GO


