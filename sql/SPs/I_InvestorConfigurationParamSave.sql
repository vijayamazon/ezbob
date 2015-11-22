SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorConfigurationParamSave') IS NOT NULL
	DROP PROCEDURE I_InvestorConfigurationParamSave
GO

IF TYPE_ID('I_InvestorConfigurationParamList') IS NOT NULL
	DROP TYPE I_InvestorConfigurationParamList
GO

CREATE TYPE I_InvestorConfigurationParamList AS TABLE (
	[InvestorID] INT NOT NULL,
	[ParameterID] INT NOT NULL,
	[Value] DECIMAL(18, 6) NOT NULL
)
GO

CREATE PROCEDURE I_InvestorConfigurationParamSave
@Tbl I_InvestorConfigurationParamList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorConfigurationParam (
		[InvestorID],
		[ParameterID],
		[Value]
	) SELECT
		[InvestorID],
		[ParameterID],
		[Value]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


