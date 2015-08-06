SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanLienLinksSave') IS NOT NULL
	DROP PROCEDURE NL_LoanLienLinksSave
GO

IF TYPE_ID('NL_LoanLienLinksList') IS NOT NULL
	DROP TYPE NL_LoanLienLinksList
GO

CREATE TYPE NL_LoanLienLinksList AS TABLE (
	[LoanID] BIGINT NOT NULL,
	[LoanLienID] INT NOT NULL,
	[Amount] DECIMAL(18, 6) NOT NULL
)
GO

CREATE PROCEDURE NL_LoanLienLinksSave
@Tbl NL_LoanLienLinksList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanLienLinks (
		[LoanID],
		[LoanLienID],
		[Amount]
	) SELECT
		[LoanID],
		[LoanLienID],
		[Amount]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


