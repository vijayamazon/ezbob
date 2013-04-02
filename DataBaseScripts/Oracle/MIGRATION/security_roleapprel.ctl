load data
infile 'security_roleapprel.txt' "str '\r'"
into table security_roleapprel
fields terminated by '#' optionally enclosed by '"'
(ROLEID char,
APPID char
)


