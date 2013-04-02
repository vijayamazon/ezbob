IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_NodeStrategyRelDrop]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_NodeStrategyRelDrop]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
Written by Kirill Sorudeykin, 	May 08, 2008
*/

create procedure Strategy_NodeStrategyRelDrop
	@pStrategyId int
as
begin
DELETE FROM Strategy_NodeStrategyRel
			WHERE StrategyId = @pStrategyId
end
GO
