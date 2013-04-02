load data
infile 'DataSource_Sources.txt' "str '\r'"
into table DataSource_Sources
fields terminated by '#' optionally enclosed by '"'
(
  ID           char,
  NAME         char,
  TYPE         char,
  DOCUMENT     char(1500),
  SIGNEDDATA   char,
  USERID       char,
  ISDELETED    char,
  CREATIONDATE DATE "YYYY-MM-DD HH24:MI:SS"
)

