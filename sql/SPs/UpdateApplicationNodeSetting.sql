IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateApplicationNodeSetting]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateApplicationNodeSetting]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateApplicationNodeSetting] 
	(@pApplicationId  bigint,
   @pNodeName       nVARCHAR(150),
   @pNodePostfix    nvarchar(100),
   @pSettingName    nvarchar(150),
   @pValue          bigint)
AS
BEGIN
	DECLARE @l_data_exists as int;
  DECLARE @l_node_id as int;

  SELECT @l_node_id = NodeId
  FROM STRATEGY_NODE
  WHERE Name = @pNodeName
    AND IsDeleted = 0;

  SELECT @l_data_exists = count(*)
  FROM Application_NodeSetting
  WHERE  ApplicationId = @pApplicationId
    AND  (NodeId = @l_node_id OR NodeId IS NULL)
    AND  NODEPOSTFIX = @pNodePostfix;

  If @l_data_exists = 0
    INSERT INTO Application_NodeSetting
      (ApplicationId, NodeId, NODEPOSTFIX, [Name], [Value])
    VALUES
      (@pApplicationId, @l_node_id, @pNodePostfix, @pSettingName, @pValue);
  else
    UPDATE Application_NodeSetting
    SET
      [Name] = @pSettingName,
      [Value] = @pValue
    WHERE  ApplicationId = @pApplicationId
      AND  (NodeId = @l_node_id OR NodeId IS NULL)
      AND  NODEPOSTFIX = @pNodePostfix
END
GO
