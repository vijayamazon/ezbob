load data
infile 'Security_UserRoleRelation_test.txt' "str '\r'"
append
into table Security_UserRoleRelation
fields terminated by '#' optionally enclosed by '"'
(USERID char,
ROLEID char
 )
