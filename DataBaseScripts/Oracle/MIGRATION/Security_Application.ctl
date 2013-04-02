load data
infile 'Security_Application.txt' "str '\r'"
into table Security_Application
fields terminated by '#' optionally enclosed by '"'
(APPLICATIONID char,
NAME char,
Description char,
APPLICATIONTYPE char
 )

