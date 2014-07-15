SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL52') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL52
GO

IF TYPE_ID('ExperianLtdDL52List') IS NOT NULL
	DROP TYPE ExperianLtdDL52List
GO

CREATE TYPE ExperianLtdDL52List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	NoticeType NVARCHAR(255) NULL,
	DateOfNotice DATETIME NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL52
@Tbl ExperianLtdDL52List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL52 (
		ExperianLtdID,
		NoticeType,
		DateOfNotice
	) SELECT
		ExperianLtdID,
		NoticeType,
		DateOfNotice
	FROM @Tbl
END
GO


