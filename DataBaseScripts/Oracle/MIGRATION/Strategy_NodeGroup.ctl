load data
infile 'Strategy_NodeGroup.txt' "str '\r'"
into table Strategy_NodeGroup
fields terminated by '#' optionally enclosed by '"'
(NODEGROUPID char,
NAME char
 )