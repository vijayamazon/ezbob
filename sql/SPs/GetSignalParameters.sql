IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSignalParameters]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSignalParameters]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSignalParameters] 
	(@pSignalId bigint,
    @pApplicationId bigint,
    @pAppSpecific bigint output,
    @pStrategyId bigint output)
AS
BEGIN
	select top 1 @pAppSpecific = AppSpecific
          ,@pStrategyId = APPLICATION_APPLICATION.STRATEGYID
    from Signal inner join Application_Application
      on SIGNAL.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
    where
		SIGNAL.Label like '%[_]' + cast(@pApplicationId as nvarchar)
		and SIGNAL.Id = @pSignalId
END
GO
