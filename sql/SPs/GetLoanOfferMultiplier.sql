IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLoanOfferMultiplier]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLoanOfferMultiplier]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLoanOfferMultiplier] 
	(@Score INT)
AS
BEGIN
	SELECT 
		Multiplier
	FROM 
		LoanOfferMultiplier
	WHERE 
		@Score >= StartScore AND 
		@Score <= EndScore
END
GO
