load data
infile 'DictHomeOwn.txt' "str '\r'"
into table DictHomeOwn
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)