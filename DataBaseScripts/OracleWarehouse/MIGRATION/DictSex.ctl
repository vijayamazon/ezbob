load data
infile 'DictSex.txt' "str '\r'"
into table DictSex
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)