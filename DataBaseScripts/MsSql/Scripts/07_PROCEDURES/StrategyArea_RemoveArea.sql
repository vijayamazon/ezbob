IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyArea_RemoveArea]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyArea_RemoveArea]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyArea_RemoveArea]
  @pAreaId int
AS
BEGIN

  DELETE FROM StrategyAreas where Id = @pAreaId;

END
GO
