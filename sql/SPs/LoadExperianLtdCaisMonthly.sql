SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdCaisMonthly') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdCaisMonthly AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdCaisMonthly
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdCaisMonthly' AS DatumType,
		ExperianLtdCaisMonthlyID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		NumberOfActiveAccounts
	FROM
		ExperianLtdCaisMonthly
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO


