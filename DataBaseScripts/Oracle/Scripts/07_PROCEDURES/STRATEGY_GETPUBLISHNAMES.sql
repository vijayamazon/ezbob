CREATE OR REPLACE Procedure Strategy_GetPublishNames
 (cur_OUT in out sys_refcursor)
AS
BEGIN

  OPEN cur_OUT FOR
       SELECT PublicnameID, Name FROM Strategy_Publicname
       WHERE (IsStopped is null or IsStopped = 0) 
         and (IsDeleted is null or IsDeleted = 0);
END;
/

