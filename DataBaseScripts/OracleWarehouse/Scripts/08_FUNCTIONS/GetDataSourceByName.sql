CREATE OR REPLACE FUNCTION 
GetDataSourceByName
(pDataSourceName varchar2)
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
	where 	isdeleted = 0
		and name = pDataSourceName;
  return lcur;
end GetDataSourceByName;
/
