CREATE OR REPLACE PROCEDURE Application_Lock
 (
   pApplicationId IN NUMBER,
   pUserId IN NUMBER,
   pVersion IN NUMBER,
   pSecurityAppId IN NUMBER -- Add AG
 )
AS
  l_currUserId Number;
  l_currVersion Number;
BEGIN

  begin
    Select Version into l_currVersion from Application_Application where ApplicationId = pApplicationID for update nowait;
    Select LockedByUserId into l_currUserId from Application_Application where ApplicationId = pApplicationID;
 
 exception 
  when others then 
    
    if sqlcode = -54 then -- Another user updated, but no commit was issued (the row is locked by Oracle lock)  
    raise_application_error(DBCONSTANTS.ERR_APP_LCKD_BY_USER_ERRCODE, DBCONSTANTS.ERR_APP_LCKD_BY_USER_MSG); 
    
    elsif sqlcode = 100 then -- NO_DATA_FOUND
    raise_application_error(DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_ERRCODE, DBCONSTANTS.ERR_APP_ID_DOESN_EXIST_MSG); 
    
    else raise ; 
   
    end if ; 
 
 
  end;

  if Nvl(l_currUserId, -1) != pUserId
  then
    if (pVersion = -1) or (Nvl(l_currVersion, -1) = pVersion)
    then
      update Application_Application set
          LockedByUserId = pUserId
      where ApplicationId = pApplicationID
      and LockedByUserId is null;

      if sql%rowcount = 0 then
        App_RaiseAppNotExistError(pApplicationId => pApplicationId);
      end if;

    else
       raise_application_error(DBCONSTANTS.ERR_APP_WAS_PROCESSED_ERRCODE, DBCONSTANTS.ERR_APP_WAS_PROCESSED_MSG);
    end if;
  end if;

App_History_Insert (pUserId, pSecurityAppId, 0, pApplicationId); -- Add AG

END Application_Lock;
/