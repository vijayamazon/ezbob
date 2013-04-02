load data
infile 'DictRisk.txt' "str '\r'"
into table DictRisk
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)