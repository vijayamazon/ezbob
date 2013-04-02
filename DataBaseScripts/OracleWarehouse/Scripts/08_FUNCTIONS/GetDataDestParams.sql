CREATE OR REPLACE FUNCTION 
GetDataDestParams
(pDataDestId number) 
return sys_refcursor
as
  lcur sys_refcursor;
begin
  open lcur for
	select  id, name, type, constraint, description,
                DestinationID, DictionaryID
      from DataDestinationParams
      where DestinationId = pDataDestId;
  return lcur;
end GetDataDestParams;
/
