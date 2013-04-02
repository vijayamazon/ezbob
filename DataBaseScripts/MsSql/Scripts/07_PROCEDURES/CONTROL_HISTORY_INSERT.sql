IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CONTROL_HISTORY_INSERT]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CONTROL_HISTORY_INSERT]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE CONTROL_HISTORY_INSERT 
(
    @pApplicationId  BIGINT,
    @pUserId         BIGINT,
    @pSecurityAppId  BIGINT,
    @pStrategyId     BIGINT,
    @pCurrentNodeId     BIGINT,
    @pCurrentNodePostfix NVARCHAR(MAX),
    @pDetailName     NVARCHAR(MAX),
    @pValue          NVARCHAR(MAX)
)
AS
BEGIN
   
    INSERT INTO control_history
      (
        applicationid,
        strategyid,
        userid,
        nodeid,
        currentnodepostfix,
        controlname,
        controlvalue,
        securityappid
      )
    VALUES
      (
        @pApplicationId,
        @pStrategyId,
        @pUserId,
        @pCurrentNodeId,
        @pCurrentNodePostfix,
        @pDetailName,
        @pValue,
        @pSecurityAppId
      );
END
GO
