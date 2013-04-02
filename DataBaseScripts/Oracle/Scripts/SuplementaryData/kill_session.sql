declare

  -- Created by A.Grechko
  -- Date 27.11.07

  v_str        varchar2(1000);
  v_schemaname varchar2(100);

begin
  
  v_schemaname := '&1';

  for rec1 in (select sid, serial#
                 from v$session
                where (upper(username) = upper(v_schemaname))
                  and status <> 'KILLED') loop
  
    v_str := 'alter system kill session ' || '''' || rec1.sid || ',' ||
             rec1.serial# || '''';
  
    execute immediate v_str;
  
  end loop;

end;
/
exit;



