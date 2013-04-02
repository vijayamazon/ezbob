CREATE OR REPLACE FUNCTION 
GetDataSourceHistParams
(pDataSourceId number) 
return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
	select  
		id, name, type, constraint, description, 
                IsHistorical, IsIdentity,
                DataSourceID, DictionaryID
      	from 	DataSourceParams
	where 	DataSourceId = pDataSourceId
      		and IsHistorical = 1;
  return lcur;
end GetDataSourceHistParams;
/
