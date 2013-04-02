load data
infile 'StrategyAreas.txt' "str '\r'"
into table StrategyAreas
fields terminated by '#' optionally enclosed by '"'
(
ID char,
NAME char
)