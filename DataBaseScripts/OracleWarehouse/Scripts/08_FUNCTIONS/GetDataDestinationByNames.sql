CREATE OR REPLACE FUNCTION 
GetDataDestinationByNames
(
pDataSourceName varchar2,
pDataDestName varchar2
)
return sys_refcursor
as
  lcur sys_refcursor;

begin
  open lcur for
   	select	dd.id, dd.name, dd.description, dd.identityfield, dd.REFERENCEFIELD,
		dd.facts_table, dd.facts_seq,
		dd.h2f_table, dd.hist_table, dd.datasourceid 
	from 	datadestinations dd, datasources ds
	where   dd.datasourceid = ds.id
		and ds.name = pDataSourceName
		and dd.name = pDataDestName;
  return lcur;
end GetDataDestinationByNames;
/
