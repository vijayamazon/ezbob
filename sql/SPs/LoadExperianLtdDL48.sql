SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL48') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL48 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL48
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL48' AS DatumType,
		ExperianLtdDL48ID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		FraudCategory,
		SupplierName
	FROM
		ExperianLtdDL48
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO


