IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyTask_NameExist]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyTask_NameExist]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyTask_NameExist]
(
  @pAreaID int,
  @pName nvarchar(64)
)
AS
BEGIN

  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
    FROM StrategyTasks
    WHERE AreaId = @pAreaId
      AND UPPER(Name) = UPPER(@pName);

  SELECT @cnt;
  RETURN @cnt;

END
GO
