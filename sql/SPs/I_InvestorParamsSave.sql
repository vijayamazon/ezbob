SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorParamsSave') IS NOT NULL
	DROP PROCEDURE I_InvestorParamsSave
GO

IF TYPE_ID('I_InvestorParamsList') IS NOT NULL
	DROP TYPE I_InvestorParamsList
GO

CREATE TYPE I_InvestorParamsList AS TABLE (
	[InvestorID] INT NOT NULL,
	[ParameterID] INT NOT NULL,
	[Value] DECIMAL(18, 6) NOT NULL,
	[Type] INT NOT NULL,
	[AllowedForConfig] BIT NOT NULL
)
GO

CREATE PROCEDURE I_InvestorParamsSave
@Tbl I_InvestorParamsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorParams (
		[InvestorID],
		[ParameterID],
		[Value],
		[Type],
		[AllowedForConfig]
	) SELECT
		[InvestorID],
		[ParameterID],
		[Value],
		[Type],
		[AllowedForConfig]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


