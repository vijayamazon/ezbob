CREATE OR REPLACE PROCEDURE UpdateApplicationStrategyId
  (
    pApplicationId IN NUMBER,
    pNewStrategyId IN NUMBER
   )
AS
BEGIN
   UPDATE Application_Application
      SET StrategyId = pNewStrategyId
   WHERE ApplicationId = pApplicationId;
END UpdateApplicationStrategyId;
/