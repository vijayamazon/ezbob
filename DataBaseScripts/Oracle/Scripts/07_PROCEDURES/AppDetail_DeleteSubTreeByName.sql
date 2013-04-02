CREATE OR REPLACE PROCEDURE AppDetail_DeleteSubTreeByName
-- Created by E.Nesterov
-- Date 23.06.08
(pApplicationId number, pRootDetailName string )

AS

  l_pRootDetailId Number;

BEGIN


  BEGIN
  SELECT d.detailid 
  into l_pRootDetailId 
  FROM Application_Detail d
    WHERE d.applicationid = pApplicationId AND
      DetailNameID in (SELECT detailnameID 
                       FROM Application_Detailname dn 
                       WHERE dn.name =  pRootDetailName);
		EXCEPTION
        WHEN no_data_found
        THEN l_pRootDetailId:=-1;
   END;

  DELETE FROM Application_Detail
  WHERE DetailId in (SELECT DetailId
                     FROM (SELECT c.DetailId, c.ParentDetailId
                           FROM (select d.DetailId, d.ParentDetailId
                                 from Application_Detail d
                                 WHERE d.ApplicationId = pApplicationId) c
                           START WITH c.DetailId = l_pRootDetailId
                           CONNECT BY PRIOR c.DetailId = c.ParentDetailId) );

END;
/
