CREATE OR REPLACE FUNCTION 
GetDataSourceById
(pDataSourceId number)
return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
  	select
		id, name, description, 
		REF_ACCTYPEID, REF_CUSTYPEID,
		basetable, identityfield, REFERENCEFIELD,
		conn_string, query_text,
		whereclause,
		creationdate, userid, historicalfacttableidfield,
		(select count(*) from datadestinations 
			where datasourceid = datasources.id) as destcount
	from datasources
	where id = pDataSourceId;
  return lcur;
end GetDataSourceById;
/
