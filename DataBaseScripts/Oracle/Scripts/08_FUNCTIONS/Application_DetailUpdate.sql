create or replace function Application_DetailUpdate
-- Created by A.Grechko
-- Date 29.01.08
-- Fixed when @pParentDetailId is NULL
-- 06.06.2008 by A.Grechko - fix of update with the same value
(pApplicationId  number,
 pDetailNameID   number,
 pParentDetailId number,
 pValueStr       CLOB,
 pIsBinary       number) return number

 AS
  lDetailNameId   number;
  lDetailId       number;
  lValueStr       CLOB;
  lValueNum       number;
  lValueDateTime  DATE;
BEGIN

  begin
    SELECT DetailId, ValueStr, ValueNum, ValueDateTime
      into lDetailId, lValueStr, lValueNum, lValueDateTime
      FROM Application_Detail
     WHERE ApplicationId = pApplicationId
       AND DetailNameId = pDetailNameID
       AND (ParentDetailId = pParentDetailId OR (ParentDetailId is NULL AND pParentDetailId is NULL) );

  exception
    when no_data_found then
      lDetailId := null;

  end;

  IF (lDetailId IS NULL) then

    begin

      select seq_app_detail.nextval into lDetailId from dual;

      INSERT INTO Application_Detail
        (detailid,
         ApplicationId,
         DetailNameId,
         ParentDetailId,
         ValueStr,
         ValueNum,
         ValueDateTime,
         IsBinary)
      VALUES
        (lDetailId,
         pApplicationId,
         lDetailNameID,
         pParentDetailId,
         pValueStr,
         Null,
         Null,
         pIsBinary);

         return lDetailId ;

    end;

  ELSE


    UPDATE Application_Detail
       SET ValueStr      = pValueStr
     WHERE DetailId = lDetailId;



  end if;

  return lDetailId;

END;
/