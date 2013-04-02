CREATE OR REPLACE FUNCTION Strategy_StrategySelect
(
  pStrategyName in varchar2,
	pStrategyUserId in Number,
	pStrategyCheckOut in Number,
	pIsChampion in Number

) return sys_refcursor
AS
  l_StrategyXml sys_refcursor;
  l_cnt Number;
BEGIN
    SELECT Count(Name) into l_cnt FROM Strategy_Strategy WHERE Name = pStrategyName AND IsDeleted = 0;

    if l_cnt = 0 then
     RAISE_APPLICATION_ERROR(-20000, 'StrategyNoExist');
    end if;

	 SELECT Count(name) into l_cnt FROM Strategy_Strategy
		   WHERE UserID <> pStrategyUserID
			 AND SubState = 0 /* Locked */
			 AND Name = pStrategyName
			 AND IsDeleted = 0
       AND pStrategyCheckOut = 1;

   if l_cnt > 0 then
     RAISE_APPLICATION_ERROR(-20000, 'StrategyIsLocked');
   end if;

  OPEN l_StrategyXml FOR 'SELECT Xml, SignedDocument FROM Strategy_Strategy
                      WHERE Name = :pStrategyName
                            AND IsDeleted = 0' using pStrategyName;

  return l_StrategyXml;
  close l_strategyXml;

END;
/

