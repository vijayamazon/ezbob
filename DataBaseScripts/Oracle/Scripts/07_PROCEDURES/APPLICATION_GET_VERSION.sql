--Gets app version
CREATE OR REPLACE PROCEDURE APPLICATION_GET_VERSION
  (
    pApplicationId IN NUMBER,
    pVersionId OUT NUMBER
   )
AS
  l_Application_ver Number;
BEGIN
    select app.version 
    into l_Application_ver
    from Application_Application app
    where app.ApplicationId = pApplicationId;
 pVersionId := l_Application_ver;
END APPLICATION_GET_VERSION;
/