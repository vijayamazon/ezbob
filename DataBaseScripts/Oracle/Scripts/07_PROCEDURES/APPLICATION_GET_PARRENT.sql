CREATE OR REPLACE PROCEDURE APPLICATION_GET_PARRENT
  (
    pApplicationId IN NUMBER,
    pParentApplicationId OUT NUMBER
   )
AS
  l_Application_id Number;
BEGIN
    select app.parentappid 
    into l_Application_id
    from Application_Application app
    where app.ApplicationId = pApplicationId;
 pParentApplicationId := l_Application_id;
END APPLICATION_GET_PARRENT;
/