
load data
infile 'Application_DetailName.txt' "str '\r'"
into table Application_DetailName
fields terminated by '#' optionally enclosed by '"'

(DETAILNAMEID char, 
NAME char
)
