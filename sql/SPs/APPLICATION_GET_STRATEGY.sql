IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[APPLICATION_GET_STRATEGY]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[APPLICATION_GET_STRATEGY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[APPLICATION_GET_STRATEGY] 
	(@pApplicationId bigint,
       @pStrategyId int output)
AS
BEGIN
	select @pStrategyId = app.strategyid 
    from Application_Application app 
    where app.ApplicationId = @pApplicationId;
END
GO
