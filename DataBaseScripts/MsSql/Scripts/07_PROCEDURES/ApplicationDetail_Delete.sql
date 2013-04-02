IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApplicationDetail_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ApplicationDetail_Delete]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Igor Borzenkov
-- Create date: 05.09.2007
-- Description:	Delete all details of given @ApplicationId
-- =============================================
CREATE PROCEDURE [dbo].[ApplicationDetail_Delete]
	-- Add the parameters for the stored procedure here
	@pApplicationId  BIGINT
AS
BEGIN
	DELETE FROM [Application_Detail]
	WHERE [ApplicationId] = @pApplicationId
END
GO
