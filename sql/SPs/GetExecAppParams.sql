IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExecAppParams]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExecAppParams]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetExecAppParams] 
	(@pApplicationId INT)
AS
BEGIN
	SELECT
    STRATEGYENGINE_EXECUTIONSTATE.ID,
    STRATEGY_NODE.NODEID,
    STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID,
    APPLICATION_APPLICATION.CREATORUSERID,
    APPLICATION_APPLICATION.VERSION,
    STRATEGY_NODE.ISHARDREACTION
FROM
    STRATEGY_NODE
    INNER JOIN STRATEGYENGINE_EXECUTIONSTATE
        ON STRATEGY_NODE.NODEID = STRATEGYENGINE_EXECUTIONSTATE.CURRENTNODEID
    INNER JOIN APPLICATION_APPLICATION
        ON STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
WHERE
    STRATEGYENGINE_EXECUTIONSTATE.APPLICATIONID=@pApplicationId
END
GO
