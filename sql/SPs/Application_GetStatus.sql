IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Application_GetStatus]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Application_GetStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Application_GetStatus] 
	(@pApplicationId bigint)
AS
BEGIN
	select signal.target,
       signal.label,
       signal.status,
       signal.starttime,
       signal.appspecific,
       strategyengine_executionstate.currentnodeid,
       strategy_node.name,
       strategyengine_executionstate.currentnodepostfix,
       application_application.executionPath,
       application_application.executionPathBin,
       application_application.lockedbyuserid,
       application_application.state,
       application_application.istimelimitexceeded,
       application_application.version,
       application_application.strategyid

       from application_application with (nolock)
        left outer join signal with (nolock) on
           application_application.applicationid = signal.applicationid 
        left outer join strategyengine_executionstate with (nolock) on 
           application_application.applicationid = strategyengine_executionstate.applicationid 
        left outer join strategy_node with (nolock) on 
           strategy_node.nodeid = strategyengine_executionstate.currentnodeid 
       
       where application_application.applicationid = @pApplicationId;
END
GO
