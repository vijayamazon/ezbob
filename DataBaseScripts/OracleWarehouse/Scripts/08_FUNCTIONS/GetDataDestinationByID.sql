CREATE OR REPLACE FUNCTION 
GetDataDestinationByID
(pDataDestId NUMBER)
return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
   	select	id, name, description, identityfield, REFERENCEFIELD,
		facts_table, facts_seq,
		h2f_table, hist_table, datasourceid 
	from 	datadestinations
	where 	id = pDataDestId;
  return lcur;
end GetDataDestinationByID;
/
