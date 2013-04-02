load data
infile 'DataSourceParams.txt' "str '\r'"
into table DataSourceParams
fields terminated by '#' optionally enclosed by '"'
( 
ID char,
NAME char,
TYPE char,
CONSTRAINT char,
DESCRIPTION char,
ISHISTORICAL char,
ISIDENTITY char,
DICTIONARYID char "decode(:DICTIONARYID,'NULL','',:DICTIONARYID)",
DATASOURCEID char
)