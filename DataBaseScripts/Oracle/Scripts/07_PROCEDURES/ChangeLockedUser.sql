create or replace procedure ChangeLockedUser(pAppID in number,pUserID in number)
as
begin
  update application_application aa
  set aa.lockedbyuserid = pUserID
  where aa.applicationid = pAppID;
end ChangeLockedUser;
/
