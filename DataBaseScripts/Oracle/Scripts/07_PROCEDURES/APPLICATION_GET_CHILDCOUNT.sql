--Updates app child count
CREATE OR REPLACE PROCEDURE APPLICATION_UPDATE_CHILDCOUNT
  (
    pApplicationId IN NUMBER,
    pNChild IN NUMBER
   )
AS
BEGIN
  UPDATE 
  application_application t
  SET 
  t.childcount = nvl(t.childcount,0) + pNChild
  WHERE
  t.applicationid = pApplicationId;
END APPLICATION_UPDATE_CHILDCOUNT;
/