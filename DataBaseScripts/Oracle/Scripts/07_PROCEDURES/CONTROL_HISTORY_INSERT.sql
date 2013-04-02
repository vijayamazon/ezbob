CREATE OR REPLACE PROCEDURE CONTROL_HISTORY_INSERT
(
	pApplicationId  NUMBER,
	pUserId         NUMBER,
	pSecurityAppId  NUMBER,
	pStrategyId  NUMBER,
	pCurrentNodeId  NUMBER,
	pCurrentNodePostfix  VARCHAR2,
	pDetailName     VARCHAR2,
	pValue          VARCHAR2
)
AS
	historyid NUMBER;
BEGIN
	SELECT
		seq_control_hyistory.nextval
	INTO
		historyid
	FROM
		dual;
		
	INSERT INTO control_history
	  (
	    hisrotyid,
	    applicationid,
	    strategyid,
	    userid,
	    nodeid,
	    currentnodepostfix,
	    controlname,
	    controlvalue,
	    securityappid
	  )
	VALUES
	  (
	    historyid,
	    pApplicationId,
	    pStrategyId,
	    pUserId,
	    pCurrentNodeId,
	    pCurrentNodePostfix,
	    pDetailName,
	    pValue,
	    pSecurityAppId
	  );
END;
/

