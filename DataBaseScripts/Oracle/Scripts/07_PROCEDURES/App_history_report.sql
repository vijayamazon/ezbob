CREATE OR REPLACE PROCEDURE App_history_report
-- Created by A.Grechko
  -- Date 20.05.08
(pApplicationId number,
 pNodeId        number,
 pS1            out number,
 pl1            out number,
 plimitisover   out number)

 AS

  l_lock        date;
  l_unlock      date;
  l_lock_prev   date;
  l_unlock_prev date;
  l_s1          number;

BEGIN

  l_s1          := 0;
  l_lock_prev   := sysdate - 1000 * 365;
  l_unlock_prev := sysdate - 1000 * 365;
  plimitisover  := 0;

  while 1 < 2 loop

    begin
      select min(actiondatetime)
        into l_lock
        from application_history
       where applicationid = papplicationid
         and currentnodeid = pNodeId
         and actiontype = 0
   and securityapplicationid <> -1
         and actiondatetime > l_lock_prev;

    end;

    if l_lock is null then
      exit;
    end if;

    select nvl(min(actiondatetime), sysdate)
      into l_unlock
      from application_history
     where applicationid = papplicationid
       and currentnodeid = pNodeId
       and actiontype = 1
    and securityapplicationid <> -1
       and actiondatetime > l_unlock_prev;

l_s1          := l_s1 + round((l_unlock - l_lock) * 24 * 60 * 60);
    l_lock_prev   := l_lock;
    l_unlock_prev := l_unlock;
  end loop;

  ps1 := l_s1;

  begin
    select EXECUTIONDURATION
      into pl1
      from Strategy_Node
     where nodeid = pNodeId;
  
    if ps1 > pl1 then
    plimitisover := 1;
  end if;

  exception
    when no_data_found then
      plimitisover := 0;
  end;

/*
CREATE OR REPLACE PROCEDURE App_history_report
-- Created by A.Grechko
  -- Date 20.05.08
(pApplicationId number,
 pNodeId        number,
 pS1            out number,
 pl1            out number,
 plimitisover   out number)

 AS

  l_lock        date;
  l_unlock      date;
  l_lock_prev   date;
  l_unlock_prev date;
  l_s1          number;
  
BEGIN
  
  l_s1          := 0;
  l_lock_prev   := sysdate - 1000 * 365;
  l_unlock_prev := sysdate - 1000 * 365;
  plimitisover  := 0;

  begin
    select EXECUTIONDURATION
      into pl1
      from Strategy_Node
     where nodeid = pNodeId;
  exception
    when no_data_found then
      pl1 := 0;
  end;

  while 1 < 2 loop
  
    begin
      select min(actiondatetime)
        into l_lock
        from application_history
       where applicationid = papplicationid
         and currentnodeid = pNodeId
         and actiontype = 0
         and actiondatetime > l_lock_prev;
    
    end;
  
    if l_lock is null then
      exit;
    end if;
  
    select nvl(min(actiondatetime), sysdate)
      into l_unlock
      from application_history
     where applicationid = papplicationid
       and currentnodeid = pNodeId
       and actiontype = 1
       and actiondatetime > l_unlock_prev;
  
    l_s1          := l_s1 + round((l_unlock - l_lock) * 24 * 60 * 60);
    l_lock_prev   := l_lock;
    l_unlock_prev := l_unlock;
  end loop;

  ps1 := l_s1;

  if ps1 > pl1 then
    plimitisover := 1;
  end if;

END;*/
END;
/