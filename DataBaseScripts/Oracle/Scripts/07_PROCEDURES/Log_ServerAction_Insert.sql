create or replace procedure Log_ServerAction_Insert
	-- Add the parameters for the stored procedure here
  (
  	pCommand in varchar2,
    pRequest in clob,
    pApplicationId in Number,
    pUserHost in varchar2,
    pId out Number
  )
AS
 l_serviceactionid Number;
BEGIN
  Select seq_serviceAction.Nextval into l_serviceactionid from dual;
  pid := l_serviceactionid;
	INSERT INTO Log_ServiceAction (LogServiceActionID, Command, Request, ApplicationId, UserHost, DateTime)
	VALUES (l_serviceactionid, pCommand,pRequest, pApplicationId, pUserHost, sysdate);
--  commit;
END;
/

