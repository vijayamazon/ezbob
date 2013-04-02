CREATE OR REPLACE FUNCTION App_DetailInsertStrategyVar
                                  (pApplicationId number,
                                  pBodyId         number,
                                  pDetailNameId   number,
                                  pValueStr       CLOB)
RETURN NUMBER
 AS
  l_DetailID         number;
begin
    select seq_app_detail.nextval into l_DetailID from dual;
    INSERT INTO Application_Detail
        (Detailid,
         ApplicationId,
         DetailNameId,
         ParentDetailId,
         ValueStr,
         ValueNum,
         ValueDateTime,
         IsBinary)
    VALUES
        (l_DetailID,
         pApplicationId,
         pDetailNameId,
         pBodyId,
         pValueStr,
         null,
         null,
         null);
	return l_DetailID;
END;
/
