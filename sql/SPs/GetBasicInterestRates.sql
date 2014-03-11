IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasicInterestRates]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBasicInterestRates]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetBasicInterestRates]
AS
BEGIN
	SELECT 
		Id,
		FromScore,
		ToScore,
		LoanInterestBase
	FROM 
		BasicInterestRate
END
GO
