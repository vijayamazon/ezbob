create or replace function str2tbl( p_str in varchar2, p_delim in varchar2 default ',' )
    return tbl_result PIPELINED
   as
      l_str      long default p_str || p_delim;
      l_n        number;
    begin
        loop
            l_n := instr( l_str, p_delim );
            exit when (nvl(l_n,0) = 0);
            pipe row( ltrim(rtrim(substr(l_str,1,l_n-1))) );
            l_str := substr( l_str, l_n+1 );
       end loop;
       return;
   end;
/
