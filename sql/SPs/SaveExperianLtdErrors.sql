SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdErrors') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdErrors
GO

IF TYPE_ID('ExperianLtdErrorsList') IS NOT NULL
	DROP TYPE ExperianLtdErrorsList
GO

CREATE TYPE ExperianLtdErrorsList AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	ErrorMessage NTEXT NULL
)
GO

CREATE PROCEDURE SaveExperianLtdErrors
@Tbl ExperianLtdErrorsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdErrors (
		ExperianLtdID,
		ErrorMessage
	) SELECT
		ExperianLtdID,
		ErrorMessage
	FROM @Tbl
END
GO

