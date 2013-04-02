load data
infile 'DictChecking.txt' "str '\r'"
into table DictChecking
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)