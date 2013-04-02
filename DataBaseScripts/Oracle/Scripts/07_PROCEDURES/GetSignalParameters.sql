CREATE OR REPLACE procedure GetSignalParameters
(
    pSignalId in Number,
    pApplicationId in Number,
    pAppSpecific OUT Number,
    pStrategyId OUT Number
)
as
 l_app_specific Number;
 l_strategy_id Number;
begin
  begin
    select AppSpecific,  APPLICATION_APPLICATION.STRATEGYID
    into l_app_specific, l_strategy_id
    from Signal inner join Application_Application
      on SIGNAL.APPLICATIONID = APPLICATION_APPLICATION.APPLICATIONID
    where
		SIGNAL.Label like '%_' || pApplicationId and 
		rownum=1 and
		SIGNAL.Id = pSignalId;

  exception
    when no_data_found
    then l_app_specific := 0;
  end;
  pAppSpecific := l_app_specific;
  pStrategyId := l_strategy_id;

end GetSignalParameters;
/
