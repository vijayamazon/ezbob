load data
infile 'Dictionaries.txt' "str '\r'"
into table Dictionaries
fields terminated by '#' optionally enclosed by '"'
(
ID char,
DISPLAYNAME  char,
TABLENAME    char
)