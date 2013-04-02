load data
infile 'Security_Role.txt' "str '\r'"
into table Security_Role
fields terminated by '#' optionally enclosed by '"'
(roleID char,
NAME char,
DESCRIPTION char
 )

