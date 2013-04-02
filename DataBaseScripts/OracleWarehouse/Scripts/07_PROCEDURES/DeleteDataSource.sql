CREATE OR REPLACE PROCEDURE DeleteDataSource
(pDataSourceId number)
 AS
  l_tablename varchar2(30);

BEGIN

    select basetable
      into l_tablename
      from datasources
     where id = pDataSourceId;

drop_view (l_tablename);

delete from DataDestinationParams
where 
(select datasourceid from DataDestinations where id = destinationid) = pDataSourceId;

delete from DATADESTINATIONS where DATASOURCEID = pDataSourceId;
delete from datasourceparams where DATASOURCEID = pDataSourceId;
delete from datasources where ID = pDataSourceId;

commit;
END;
/