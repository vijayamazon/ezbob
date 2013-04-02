create or replace function App_DetailSelectChildIDs
-- Created by A.Grechko
-- Date 06.12.07
(pName VARCHAR2, pApplicationId number, pParentDetailID number)

 return sys_refcursor

 AS

  l_cur sys_refcursor;

BEGIN

  IF pParentDetailID IS NULL THEN

    open l_cur for

     'SELECT Application_Detail.DetailID DetailID
      FROM Application_Detail
      JOIN Application_DetailName ON Application_Detail.DetailNameID = Application_DetailName.DetailNameId
     WHERE Application_DetailName.Name = :pName
       AND Application_Detail.ApplicationID = :pApplicationId
       AND Application_Detail.ParentDetailID IS NULL
     ORDER BY DetailID'
      using pName, pApplicationId;

  ELSE

    open l_cur for

     'SELECT Application_Detail.DetailID DetailID
      FROM Application_Detail
      JOIN Application_DetailName ON Application_Detail.DetailNameID = Application_DetailName.DetailNameId
     WHERE Application_DetailName.Name = :pName
       AND Application_Detail.ApplicationID = :pApplicationId
       AND Application_Detail.ParentDetailID = :pParentDetailID
     ORDER BY DetailID'
      using pName, pApplicationId, pParentDetailID;

  end if;

  return l_cur;

END;
/