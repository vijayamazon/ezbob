load data
infile 'Security_UserRoleRelation.txt' "str '\r'"
into table Security_UserRoleRelation
fields terminated by '#' optionally enclosed by '"'
(USERID char,
ROLEID char
 )
