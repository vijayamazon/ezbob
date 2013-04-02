load data
infile 'CAHistFacts.txt' "str '\r'"
into table CAHistFacts
fields terminated by '#' optionally enclosed by '"'
(
ID char,
MASTERID char,
Score char,
Risk char
)