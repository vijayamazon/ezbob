SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeEventHistory') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeEventHistory
GO

IF TYPE_ID('CreditSafeEventHistoryList') IS NOT NULL
	DROP TYPE CreditSafeEventHistoryList
GO

CREATE TYPE CreditSafeEventHistoryList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	Date DATETIME NULL,
	Text NVARCHAR(500) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeEventHistory
@Tbl CreditSafeEventHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeEventHistory (
		CreditSafeBaseDataID,
		Date,
		Text
	) SELECT
		CreditSafeBaseDataID,
		Date,
		Text
	FROM @Tbl
END
GO


