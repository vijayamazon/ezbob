IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Delete_DS_Strategy_Rel]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Delete_DS_Strategy_Rel]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Delete_DS_Strategy_Rel]
  @pStrategyId int
AS
BEGIN

  if @pStrategyId > 0
  begin
       DELETE FROM DataSource_StrategyRel WHERE StrategyId = @pStrategyId;
  end;

END
GO
