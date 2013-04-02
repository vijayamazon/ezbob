CREATE OR REPLACE FUNCTION GetAvailableStrategyTypes
 return sys_refcursor
AS
  l_Cursor sys_refcursor;
BEGIN

  OPEN l_Cursor FOR
    SELECT DISTINCT StrategyType FROM Strategy_Strategy;
    
 return l_Cursor;
 
END;
/
