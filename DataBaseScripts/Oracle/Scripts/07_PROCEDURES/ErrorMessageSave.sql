CREATE OR REPLACE PROCEDURE ErrorMessageSave(pApplicationId IN NUMBER, pErrorMessage IN VARCHAR2)
AS
BEGIN
BEGIN
 INSERT INTO Application_Error(ApplicationId, ErrorMessage) VALUES (pApplicationId, pErrorMessage);
EXCEPTION
 WHEN DUP_VAL_ON_INDEX THEN
      UPDATE Application_Error SET ErrorMessage =  pErrorMessage WHERE ApplicationId = pApplicationId;
END;
 INSERT INTO Application_Error_History(ApplicationId, ErrorMessage) VALUES (pApplicationId, pErrorMessage);     
END ErrorMessageSave;
/
