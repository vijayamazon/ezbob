IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_CHILDCOUNT]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[APPLICATION_GET_CHILDCOUNT]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[APPLICATION_GET_CHILDCOUNT] 
	(@pApplicationId bigint,
       @pNChild int output)
AS
BEGIN
	select @pNChild = app.childcount 
    from Application_Application app 
    where app.ApplicationId = @pApplicationId
END
GO
