IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Get_Application_Results]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Get_Application_Results]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Get_Application_Results]
    @pApplicationId bigint
AS
BEGIN
     select * from Application_Result
     where ApplicationId = @pApplicationId
END
GO
