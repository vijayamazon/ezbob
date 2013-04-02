load data
infile 'DataDestinationParams.txt' "str '\r'"
into table DataDestinationParams
fields terminated by '#' optionally enclosed by '"'
( 
ID char,
NAME char,
TYPE char,
DESCRIPTION char,
CONSTRAINT char,
DICTIONARYID char "decode(:DICTIONARYID,'NULL','',:DICTIONARYID)",
DESTINATIONID char
)