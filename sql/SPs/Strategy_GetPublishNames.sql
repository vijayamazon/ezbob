IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetPublishNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetPublishNames]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetPublishNames] 
AS
BEGIN
	SELECT PublicnameID, Name FROM Strategy_Publicname
       WHERE (IsStopped is null or IsStopped = 0)
         and (IsDeleted is null or IsDeleted = 0)
END
GO
