SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdCreditSummary') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdCreditSummary
GO

IF TYPE_ID('ExperianLtdCreditSummaryList') IS NOT NULL
	DROP TYPE ExperianLtdCreditSummaryList
GO

CREATE TYPE ExperianLtdCreditSummaryList AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	CreditEventType NVARCHAR(255) NULL,
	DateOfMostRecentRecordForType DATETIME NULL
)
GO

CREATE PROCEDURE SaveExperianLtdCreditSummary
@Tbl ExperianLtdCreditSummaryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdCreditSummary (
		ExperianLtdID,
		CreditEventType,
		DateOfMostRecentRecordForType
	) SELECT
		ExperianLtdID,
		CreditEventType,
		DateOfMostRecentRecordForType
	FROM @Tbl
END
GO


