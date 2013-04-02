create or replace procedure UpdateCreatorUser
(pCreatorUserId in number,
 pAppId in number)
is
begin
     Update Application_Application
     set CreatorUserId = pCreatorUserId
     where ApplicationId=pAppId;
end UpdateCreatorUser;
/
