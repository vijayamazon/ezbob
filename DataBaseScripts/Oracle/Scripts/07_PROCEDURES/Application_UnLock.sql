
CREATE OR REPLACE PROCEDURE Application_UnLock
(
  pApplicationId IN NUMBER,
  pState IN NUMBER,
  pIsVersion IN NUMBER, /* Indicates if Version of Application must be updated*/
  pSecurityAppId IN NUMBER, -- Add AG
  pAppNewVersion OUT NUMBER
)
AS
  l_currVersion Number;
  l_UserId      Number; -- Add AG
BEGIN

  /*Updates version of Application*/
  if pIsVersion = 1 then
   Select Version into l_currVersion from Application_Application
    where ApplicationId = pApplicationID;

    update Application_Application set Version = l_currVersion + 1
        where ApplicationId = pApplicationID;
  end if;

-- Start  add  AG
  begin

    select lockedbyuserid
      into l_userid
      from Application_Application
     where ApplicationId = pApplicationID;

  exception
    when NO_DATA_FOUND then
      l_UserId := null;

  end;

-- End add AG


  if pState is null then
    update Application_Application set LockedByUserId = null
        where ApplicationId = pApplicationID;
  else
    update Application_Application set LockedByUserId = null, State = pState
        where ApplicationId = pApplicationID;
  end if;

  select Version into pAppNewVersion from Application_Application where ApplicationId = pApplicationID;
  App_History_Insert (l_UserId, pSecurityAppId, 1, pApplicationId); -- Add AG

END Application_UnLock;
/