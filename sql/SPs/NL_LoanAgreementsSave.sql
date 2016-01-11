SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanAgreementsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanAgreementsSave
GO

IF TYPE_ID('NL_LoanAgreementsList') IS NOT NULL
	DROP TYPE NL_LoanAgreementsList
GO

CREATE TYPE NL_LoanAgreementsList AS TABLE (
	[LoanHistoryID] BIGINT NOT NULL,
	[FilePath] NVARCHAR(250) NULL,
	[LoanAgreementTemplateID] INT NOT NULL
)
GO

CREATE PROCEDURE NL_LoanAgreementsSave
@Tbl NL_LoanAgreementsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanAgreements (
		[LoanHistoryID],
		[FilePath],
		[LoanAgreementTemplateID]
	) SELECT
		[LoanHistoryID],
		[FilePath],
		[LoanAgreementTemplateID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


