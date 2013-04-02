begin
for i in (select constraint_name, table_name from user_constraints where constraint_type ='R'
and status = 'DISABLED') LOOP
execute immediate 'alter table '||i.table_name||' enable novalidate constraint '||i.constraint_name||'';
end loop;
end;
/
quit;

/