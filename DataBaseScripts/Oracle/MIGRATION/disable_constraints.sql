begin
for i in (select constraint_name, table_name from user_constraints where constraint_type ='R'
and status = 'ENABLED') LOOP
execute immediate 'alter table '||i.table_name||' disable constraint '||i.constraint_name||'';
end loop;
end;
/

quit;

/