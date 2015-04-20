SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeStatusHistory') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeStatusHistory
GO

IF TYPE_ID('CreditSafeStatusHistoryList') IS NOT NULL
	DROP TYPE CreditSafeStatusHistoryList
GO

CREATE TYPE CreditSafeStatusHistoryList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	date DATETIME NULL,
	text NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeStatusHistory
@Tbl CreditSafeStatusHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeStatusHistory (
		CreditSafeBaseDataID,
		date,
		text
	) SELECT
		CreditSafeBaseDataID,
		date,
		text
	FROM @Tbl
END
GO


