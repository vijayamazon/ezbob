CREATE OR REPLACE PROCEDURE Strategy_StrategyCheckIn
(
	pStrategyName in varchar2,
	pStrategyUserID in Number
)
AS
  l_name Varchar2(255);
  l_cnt Number;
  l_cnt_s Number;
BEGIN
  begin
    SELECT Count(displayname) into l_cnt_s
    FROM Strategy_Strategy
    WHERE displayname = pStrategyName
          AND IsDeleted = 0;

    if (l_cnt_s <= 1) then
      SELECT displayname into l_name FROM Strategy_Strategy
      WHERE displayname = pStrategyName
            AND IsDeleted = 0;
    end if;
  exception when no_data_found then
    raise_application_error(DBCONSTANTS.STRATEGY_ERRCODE_NOTEXISTS, DBCONSTANTS.STRATEGY_ERR_NOTEXISTS);
  end;

  SELECT Count(displayname) into l_cnt FROM Strategy_Strategy
  WHERE UserID <> pStrategyUserID
			  AND SubState = DBCONSTANTS.STRATEGY_LOCKED /* Locked */
  			AND displayname = pStrategyName
				AND IsDeleted = 0;

  If l_cnt > 0 then
    raise_application_error(DBCONSTANTS.STRATEGY_ERRCODE_ISLOCKED, DBCONSTANTS.STRATEGY_ERR_ISLOCKED);
  end if;

	UPDATE Strategy_Strategy
       SET UserID = pStrategyUserID,
           SubState = DBCONSTANTS.STRATEGY_UNLOCKED /* Unlocked */
	 WHERE displayname = pStrategyName
         AND State = 0 -- Challenge
    	   AND IsDeleted = 0;
--   commit;
END;
/
