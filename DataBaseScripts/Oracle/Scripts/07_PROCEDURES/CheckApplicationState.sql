CREATE OR REPLACE PROCEDURE CheckApplicationState
(
  pApplicationId IN NUMBER
  )
AS
  lState Number := -1;
BEGIN

  begin
    Select State into lState
      from Application_Application where ApplicationId  = pApplicationId;
  exception
    when no_data_found then
        raise_application_error(DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_ERRCODE,
                                DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_MSG);
  end;

   if lState = 0 then
      raise_application_error(DBCONSTANTS.ERR_APP_EXEC_BY_SE_ERRCODE,
                              DBCONSTANTS.ERR_APP_EXEC_BY_SE_MSG);
   end if;
END;
/