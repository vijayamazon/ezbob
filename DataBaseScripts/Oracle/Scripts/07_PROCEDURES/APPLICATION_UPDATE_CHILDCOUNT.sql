--Gets app child count
CREATE OR REPLACE PROCEDURE APPLICATION_GET_CHILDCOUNT
  (
    pApplicationId IN NUMBER,
    pNChild OUT NUMBER
   )
AS
  l_nChild Number;
BEGIN
    select app.childcount
    into l_nChild
    from Application_Application app
    where app.ApplicationId = pApplicationId;
 pNChild := l_nChild;
END APPLICATION_GET_CHILDCOUNT;
/