IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_UPDATE_CHILDCOUNT]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[APPLICATION_UPDATE_CHILDCOUNT]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[APPLICATION_UPDATE_CHILDCOUNT]
       @pApplicationId bigint,
       @pNChild int output
AS
BEGIN
declare @oldChildcound int
    select @oldChildcound = app.childcount 
    from Application_Application app 
    where app.ApplicationId = @pApplicationId;
    
    if @oldChildcound is NULL set @oldChildcound = 0;
    
    update 
		application_application
    set
		Application_Application.childcount = @oldChildcound + @pNChild
    where
		Application_Application.applicationid = @pApplicationId;
END
GO
