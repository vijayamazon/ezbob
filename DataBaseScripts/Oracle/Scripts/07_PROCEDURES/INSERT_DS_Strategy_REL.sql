CREATE OR REPLACE PROCEDURE INSERT_DS_Strategy_REL
  (
    pDataSourceName IN Varchar2,
    pStrategyId IN number
  )
AS
  newId number;
  pDataSourceId number;
BEGIN

  begin

    select tbl.id into newId from DATASOURCE_STRATEGYREL tbl
    where tbl.DATASOURCENAME = pDataSourceName 
	  and tbl.strategyid = pStrategyId;

    exception
      when no_data_found then
        begin
          select SEQ_DATASOURCE_STRATEGYREL.nextval into newId from dual;
          insert into DATASOURCE_STRATEGYREL
            (id, DATASOURCENAME, strategyid)
          values
            (newId, pDataSourceName, pStrategyId);
        end;
  end;

END;
/
