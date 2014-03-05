IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationDetail_Delete]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[ApplicationDetail_Delete]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ApplicationDetail_Delete] 
	(@pApplicationId  BIGINT)
AS
BEGIN
	DELETE FROM [Application_Detail]
	WHERE [ApplicationId] = @pApplicationId
END
GO
