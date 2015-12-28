SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_GetInvestorRules') IS NOT NULL
	DROP PROCEDURE I_GetInvestorRules
GO


CREATE PROCEDURE I_GetInvestorRules
@InvestorId INT = NULL,
@RuleType INT
AS
BEGIN
	SELECT 
	[RuleID],
	[UserID] [int] ,
	[RuleType] [int],
	[InvestorID] [int],
	[MemberNameSource] ,
	[MemberNameTarget] ,
	[LeftParamID],
	[RightParamID],
	[Operator] ,
	[IsRoot] 
	FROM
		I_InvestorRule

	WHERE
		InvestorID = @InvestorID
		AND
		RuleType = @RuleType
END