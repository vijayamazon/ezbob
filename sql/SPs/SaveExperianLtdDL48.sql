SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL48') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL48
GO

IF TYPE_ID('ExperianLtdDL48List') IS NOT NULL
	DROP TYPE ExperianLtdDL48List
GO

CREATE TYPE ExperianLtdDL48List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	FraudCategory NVARCHAR(255) NULL,
	SupplierName NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL48
@Tbl ExperianLtdDL48List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL48 (
		ExperianLtdID,
		FraudCategory,
		SupplierName
	) SELECT
		ExperianLtdID,
		FraudCategory,
		SupplierName
	FROM @Tbl
END
GO


