IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianCacheDate]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianCacheDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExperianCacheDate]
	@Keys StringList READONLY
AS
BEGIN
	SELECT 
		MIN(LastUpdateDate)
	FROM 
		MP_ExperianBankCache,
		@Keys
	WHERE
		KeyData IN (SELECT Value FROM @Keys)
END
GO
