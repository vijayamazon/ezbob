IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_PARRENT]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[APPLICATION_GET_PARRENT]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[APPLICATION_GET_PARRENT]
       @pApplicationId bigint,
       @pParentApplicationId bigint output
AS
BEGIN
    select @pParentApplicationId = app.parentappid 
    from Application_Application app 
    where app.ApplicationId = @pApplicationId;
END
GO
