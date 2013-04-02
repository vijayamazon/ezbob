CREATE OR REPLACE PROCEDURE init_session

AS

BEGIN

execute immediate 

'alter session set optimizer_index_caching = 100' ;

execute immediate 

'alter session set optimizer_index_cost_adj = 1' ;

END;
/