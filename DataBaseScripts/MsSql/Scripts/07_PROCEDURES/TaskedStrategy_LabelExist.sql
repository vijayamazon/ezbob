IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaskedStrategy_LabelExist]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[TaskedStrategy_LabelExist]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[TaskedStrategy_LabelExist]
(
  @pTaskId int,
  @pLabel nvarchar(64)
)
AS
BEGIN

  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
    FROM  TaskedStrategies
    WHERE TaskId = @pTaskId
      AND UPPER(Label) = UPPER(@pLabel);

  SELECT @cnt;
  RETURN @cnt;

END
GO
