CREATE OR REPLACE PROCEDURE Strategy_UpdateChampionStatus
  (
    pPublicId IN number
   )
AS
 stratId number;
BEGIN

    update strategy_strategy
        set state = 0
      where strategyid in (select strategyid from strategy_publicrel
   where publicid = pPublicId);

  select strategyId into stratId
    from strategy_publicrel
   where publicid = pPublicId
   and percent = (select MAX(percent) from strategy_publicrel
   where publicid = pPublicId)
   and rownum <=1;
      
   if stratId > 0 then
     update strategy_strategy
        set state = 1
      where strategyid = stratId;
   end if;

END;
/

