IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetBasicInterestRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetBasicInterestRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetBasicInterestRate] 
	(@Score INT)
AS
BEGIN
	SELECT 
		LoanIntrestBase
	FROM 
		BasicInterestRate
	WHERE 
		@Score >= FromScore AND 
		@Score <= ToScore
END
GO
