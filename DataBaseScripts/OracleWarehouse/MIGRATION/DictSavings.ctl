load data
infile 'DictSavings.txt' "str '\r'"
into table DictSavings
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)