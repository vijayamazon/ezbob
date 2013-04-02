CREATE OR REPLACE PROCEDURE ApplicationDetail_Delete
-- Created by A.Grechko
-- Date 05.12.07
(pApplicationId  number)
AS
BEGIN
  DELETE FROM Application_Detail
  WHERE ApplicationId = pApplicationId;
END;
/
