CREATE OR REPLACE PROCEDURE App_RaiseAppNotExistError
  (
    pApplicationId IN NUMBER
  )
AS
 l_LockedByUser NUMBER := NULL;
BEGIN
     select LockedByUserId into l_LockedByUser
        from Application_Application
          where ApplicationId = pApplicationID;

     if l_LockedByUser is null then
        raise_application_error(DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_ERRCODE,
                                DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_MSG);
      else
        raise_application_error(DBCONSTANTS.ERR_APP_LCKD_BY_USER_ERRCODE,
                                DBCONSTANTS.ERR_APP_LCKD_BY_USER_MSG);
     end if;

END App_RaiseAppNotExistError;
/