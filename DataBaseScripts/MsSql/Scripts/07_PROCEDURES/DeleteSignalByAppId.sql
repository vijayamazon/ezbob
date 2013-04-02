IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteSignalByAppId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteSignalByAppId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].DeleteSignalByAppId
(
    @pApplicationId bigint,
    @pAppSpecific bigint output,
    @pStrategyId bigint output
)
as
begin
    DECLARE @SignalId INT

    SET TRANSACTION ISOLATION LEVEL READ COMMITTED 
    select @SignalId = Id, @pAppSpecific = AppSpecific,  @pStrategyId = APPLICATION_APPLICATION.STRATEGYID
    from Signal WITH (NOLOCK) inner join Application_Application WITH (NOLOCK)
      on SIGNAL.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
    where
		SIGNAL.Label like '%[_]' + CAST(@pApplicationId as nvarchar) and
		SIGNAL.ApplicationId = @pApplicationId;

    delete from Signal WITH (ROWLOCK)
    where Id = @SignalId;

END;
GO
