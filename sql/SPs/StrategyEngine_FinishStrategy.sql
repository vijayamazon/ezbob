IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyEngine_FinishStrategy]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyEngine_FinishStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyEngine_FinishStrategy] 
	(@pApplicationId bigint,
	@pErrorMsg nvarchar(512),
	@pUserId int)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @state int;
	
	if @pErrorMsg is null
	  set @state = 2 -- 2-strategy has been finished without errors;
    else
      set @state = 3 -- 3-strategy has been finished with errors.

    update Application_Application set
           state = @state,
           ErrorMsg = @pErrorMsg,
           LockedByUserId = null,
           LastUpdateDate = GetDate(),
           IsTimeLimitExceeded = ( SELECT CASE 
	                                        WHEN DATEADD( ss, ISNULL( s.ExecutionDuration, 1000000000), a.CreationDate) < GETDATE() THEN 1
                                            ELSE 0
                                          END
                                     FROM Application_Application a JOIN Strategy_Strategy s ON a.StrategyID = s.StrategyID
                                    WHERE a.ApplicationID = @pApplicationId )
    where ApplicationId = @pApplicationID
    and LockedByUserId = @pUserId
    
    if @@ROWCOUNT = 0
       execute App_RaiseAppNotExistError @pApplicationId
    
    DELETE FROM StrategyEngine_ExecutionState
    WHERE ApplicationID = @pApplicationID

    DELETE FROM APPLICATION_NODESETTING
    WHERE ApplicationID = @pApplicationID
END
GO
