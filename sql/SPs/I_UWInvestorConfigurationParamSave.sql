SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_UWInvestorConfigurationParamSave') IS NOT NULL
	DROP PROCEDURE I_UWInvestorConfigurationParamSave
GO

IF TYPE_ID('I_UWInvestorConfigurationParamList') IS NOT NULL
	DROP TYPE I_UWInvestorConfigurationParamList
GO

CREATE TYPE I_UWInvestorConfigurationParamList AS TABLE (
	[InvestorID] INT NOT NULL,
	[ParameterID] INT NOT NULL,
	[Value] DECIMAL(18, 6) NOT NULL,
	[AllowedForConfig] BIT NOT NULL
)
GO

CREATE PROCEDURE I_UWInvestorConfigurationParamSave
@Tbl I_UWInvestorConfigurationParamList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_UWInvestorConfigurationParam (
		[InvestorID],
		[ParameterID],
		[Value],
		[AllowedForConfig]
	) SELECT
		[InvestorID],
		[ParameterID],
		[Value],
		[AllowedForConfig]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


