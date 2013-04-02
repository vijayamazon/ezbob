CREATE OR REPLACE PROCEDURE AppDetail_DeleteSubTree
-- Created by A.Grechko
-- Date 07.12.07
(pApplicationId number, pRootDetailId number)

AS

BEGIN

  DELETE FROM Application_Detail
  WHERE DetailId in (SELECT DetailId
                     FROM (SELECT c.DetailId, c.ParentDetailId
                           FROM (select d.DetailId, d.ParentDetailId
                                 from Application_Detail d
                                 WHERE d.ApplicationId = pApplicationId) c
                           START WITH c.DetailId = pRootDetailId
                           CONNECT BY PRIOR c.DetailId = c.ParentDetailId) );

END;
/