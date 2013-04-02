load data
infile 'DictMaritalStatus.txt' "str '\r'"
into table DictMaritalStatus
fields terminated by '#' optionally enclosed by '"'
(
ID char,
VALUE char
)