load data
infile 'StrategyTasks.txt' "str '\r'"
into table StrategyTasks
fields terminated by '#' optionally enclosed by '"'
(
ID char,
NAME char,
AreaID char
)