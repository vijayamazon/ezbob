--Gets app version
CREATE OR REPLACE PROCEDURE APPLICATION_GET_STRATEGY
  (
    pApplicationId IN NUMBER,
    pStrategyId OUT NUMBER
   )
AS
  l_Strategy Number;
BEGIN
    select app.strategyid 
    into l_Strategy
    from Application_Application app
    where app.ApplicationId = pApplicationId;
 pStrategyId := l_Strategy;
END APPLICATION_GET_STRATEGY;
/