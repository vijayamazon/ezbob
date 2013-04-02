declare

  -- Created by A.Grechko
  -- Date 12.12.07

  
  v_schemaname varchar2(100);
  v_str        varchar2(1000);

begin
  
  v_schemaname := '&1';
  v_str := 'drop user '|| v_schemaname ||' cascade';

  execute immediate v_str;
  

end;
/

exit;
