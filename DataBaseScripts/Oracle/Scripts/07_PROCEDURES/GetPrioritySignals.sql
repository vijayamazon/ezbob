CREATE OR REPLACE Procedure GetPrioritySignals
(
    pCount in number,
    pPriority in number,
    pOwnerAppId in number,
    cur_OUT in out sys_refcursor
)
as
BEGIN
 update Signal set Status = -1 
 where id in
  (select *
   from (
       select id 
       from Signal 
       where status= 0 
         and ((priority is null) or (priority <= pPriority))
         and ((ownerApplicationId is null) or (ownerApplicationId <= pOwnerAppId))
         and (IsExternal is null or IsExternal = 0) 
       order by priority) j
 where rownum <= pCount);
  
 open cur_OUT for
   select
      applicationId,
      appSpecific,
      id,
      label,
      priority,
      ownerApplicationId,
      message body,
      executionType
   from
      signal
   where
      status = -1
   order by priority;
END GetPrioritySignals;
/