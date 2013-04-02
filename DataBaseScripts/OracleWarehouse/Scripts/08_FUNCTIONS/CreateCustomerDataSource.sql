CREATE OR REPLACE FUNCTION
CreateCustomerDataSource 
(
pName varchar2, 
pDescription varchar2,
pTypeID number,
pWhereClause varchar2
) 
RETURN NUMBER
AS
  l_tablename varchar2(30);
  l_seq number;
BEGIN

select SEQ_DataSource.nextval into l_seq from dual ;

  l_tablename := 'DataSource_'||l_seq;

  insert into datasources
	(id, name, description, creationdate, basetable,
	ref_custypeid, identityfield, 
	referencefield, historicalfacttableidfield, whereclause, isdeleted)
  values
	(l_seq, pName, pDescription, sysdate, l_tablename,
	pTypeID, 'ID', 'MASTERID', 'HISTORICALID', pWhereClause, 0);

  return l_seq;
commit;
END;
/
