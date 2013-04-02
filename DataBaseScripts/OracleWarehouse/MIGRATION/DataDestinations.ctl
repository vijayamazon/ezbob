load data
infile 'DataDestinations.txt' "str '\r'"
into table DataDestinations
fields terminated by '#' optionally enclosed by '"'
(
  ID		char,
  NAME		char,
  DESCRIPTION	char,
  IDENTITYFIELD	char,
  REFERENCEFIELD char,
  FACTS_TABLE	char,
  FACTS_SEQ	char,
  H2F_TABLE	char,
  HIST_TABLE	char,
  DATASOURCEID	char,
  ISDELETED	char
)