IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataSource_GetLinkedStr]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DataSource_GetLinkedStr]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DataSource_GetLinkedStr]
(
  @pDataSourceName nvarchar(255)
)
AS
BEGIN

  SELECT [Strategy_Strategy].[StrategyId] as [ID], 
         [Strategy_Strategy].[displayname] as [Name]
  FROM [Strategy_Strategy], [DataSource_StrategyRel]
  WHERE [DataSource_StrategyRel].[DataSourceName] = @pDataSourceName
    AND [Strategy_Strategy].[StrategyId] = [DataSource_StrategyRel].[StrategyId]
    AND ([Strategy_Strategy].[IsDeleted] IS NULL OR [Strategy_Strategy].[IsDeleted] = 0);

END
GO
