IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StrategyArea_NameExist]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[StrategyArea_NameExist]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StrategyArea_NameExist]
(
  @pName nvarchar(64)
)
AS
BEGIN
  
  DECLARE @cnt int;

  SELECT @cnt = COUNT(*) 
  FROM StrategyAreas 
  WHERE UPPER(Name) = UPPER(@pName);

  SELECT @cnt;
  RETURN @cnt;

END
GO
