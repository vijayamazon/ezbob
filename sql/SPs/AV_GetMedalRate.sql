IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_GetMedalRate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_GetMedalRate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_GetMedalRate] 
	(@CustomerId INT)
AS
BEGIN
	SELECT TOP 1 ScoreResult FROM CustomerScoringResult WHERE CustomerId=@CustomerId ORDER BY ScoreDate DESC
END
GO
