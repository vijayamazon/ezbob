CREATE OR REPLACE FUNCTION
CreateDataDestination
(
pDataSourceId number,
pName varchar2, 
pDescription varchar2,
pFactsTable varchar2,
pFactsSequence varchar2,
pLinksTable varchar2,
pHistoryTable varchar2
) 
RETURN NUMBER
AS
  l_seq number;
BEGIN

  select SEQ_DataDestination.nextval into l_seq from dual ;

  insert into DATADESTINATIONS
	(id, name, description,   
	IDENTITYFIELD, REFERENCEFIELD, 
	FACTS_TABLE, FACTS_SEQ,
	H2F_TABLE, HIST_TABLE,
  	DATASOURCEID, isdeleted)
  values
	(l_seq, pName, pDescription, 
	'ID', 'MASTERID', 
	pFactsTable, pFactsSequence,
	pLinksTable, pHistoryTable,
	pDataSourceId, 0);

  return l_seq;
commit;
END;
/
