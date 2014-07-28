SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataCaisCardHistory') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataCaisCardHistory AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataCaisCardHistory
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataCaisCardHistory' AS DatumType,
		h.Id,
		ExperianConsumerDataCaisId,
		PrevStatementBal,
		PromotionalRate,
		PaymentAmount,
		NumCashAdvances,
		CashAdvanceAmount,
		PaymentCode
	FROM
		ExperianConsumerDataCaisCardHistory h INNER JOIN ExperianConsumerDataCais c ON h.ExperianConsumerDataCaisId = c.Id
	WHERE
		c.ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

