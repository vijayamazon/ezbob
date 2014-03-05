IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetPublishNamesCount]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetPublishNamesCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetPublishNamesCount]
AS
BEGIN
		SELECT p.name, 
            (SELECT COUNT(strategyId) FROM Strategy_Publicrel 
            WHERE publicId = p.publicnameid) STRATEGYCOUNT, p.publicnameid, p.isstopped
                                FROM Strategy_PublicName p
            WHERE  (IsDeleted is null or IsDeleted = 0)
END
GO
