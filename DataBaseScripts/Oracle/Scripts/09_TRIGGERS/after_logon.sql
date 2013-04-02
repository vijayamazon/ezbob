create or replace trigger after_logon
after logon on schema
declare 

v_sql_1 varchar2(100) := 'alter session set optimizer_index_caching = 100' ;
v_sql_2 varchar2(100) := 'alter session set optimizer_index_cost_adj = 1' ;

begin 

execute immediate v_sql_1 ; 
execute immediate v_sql_2 ;

end;
/