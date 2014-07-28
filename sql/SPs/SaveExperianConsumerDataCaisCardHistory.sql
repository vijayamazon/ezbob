SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianConsumerDataCaisCardHistory') IS NOT NULL
	DROP PROCEDURE SaveExperianConsumerDataCaisCardHistory
GO

IF TYPE_ID('ExperianConsumerDataCaisCardHistoryList') IS NOT NULL
	DROP TYPE ExperianConsumerDataCaisCardHistoryList
GO

CREATE TYPE ExperianConsumerDataCaisCardHistoryList AS TABLE (
	ExperianConsumerDataCaisId BIGINT NULL,
	PrevStatementBal INT NULL,
	PromotionalRate NVARCHAR(255) NULL,
	PaymentAmount INT NULL,
	NumCashAdvances INT NULL,
	CashAdvanceAmount INT NULL,
	PaymentCode NVARCHAR(255) NULL
)
GO

CREATE PROCEDURE SaveExperianConsumerDataCaisCardHistory
@Tbl ExperianConsumerDataCaisCardHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianConsumerDataCaisCardHistory (
		ExperianConsumerDataCaisId,
		PrevStatementBal,
		PromotionalRate,
		PaymentAmount,
		NumCashAdvances,
		CashAdvanceAmount,
		PaymentCode
	) SELECT
		ExperianConsumerDataCaisId,
		PrevStatementBal,
		PromotionalRate,
		PaymentAmount,
		NumCashAdvances,
		CashAdvanceAmount,
		PaymentCode
	FROM @Tbl
END
GO