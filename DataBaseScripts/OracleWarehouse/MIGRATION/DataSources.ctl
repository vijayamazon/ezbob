load data
infile 'DataSources.txt' "str '\r'"
into table DataSources
fields terminated by '#' optionally enclosed by '"'
(
  ID		char,
  NAME		char,
  REF_CUSTYPEID	char "decode(:REF_CUSTYPEID,'NULL','',:REF_CUSTYPEID)",
  REF_ACCTYPEID	char "decode(:REF_ACCTYPEID,'NULL','',:REF_ACCTYPEID)",
  BASETABLE	char,
  IDENTITYFIELD	char,
  REFERENCEFIELD char,
  CONN_STRING	char,
  QUERY_TEXT	char,
  ISDELETED	char,
  HISTORICALFACTTABLEIDFIELD	char
)