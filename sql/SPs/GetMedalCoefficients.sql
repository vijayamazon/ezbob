IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMedalCoefficients]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMedalCoefficients]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetMedalCoefficients]
	(@MedalFlow NVARCHAR(50),
	 @Medal NVARCHAR(50))
AS
BEGIN
	SELECT 
		AnnualTurnover,
		ValueAdded,
		FreeCashFlow
	FROM
		MedalCoefficients
	WHERE
		MedalFlow = @MedalFlow AND
		Medal = @Medal
END
GO
