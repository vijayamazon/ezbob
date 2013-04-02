/*
Written by Kirill Sorudeykin, 	May 08, 2008
*/
create or replace procedure Strategy_NodeStrategyRelDrop(pStrategyId in numeric)
as
begin
DELETE FROM Strategy_NodeStrategyRel WHERE StrategyId = pStrategyId;
end;
/
