IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_GetDelStrategy]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_GetDelStrategy]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_GetDelStrategy]
(
  @pStrategyName nvarchar(MAX)
) 
AS
BEGIN
    select strategyid from strategy_strategy 
    where strategy_strategy.name = @pStrategyName;   
END;
GO
