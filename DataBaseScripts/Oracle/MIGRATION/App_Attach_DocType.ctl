
load data
infile 'App_Attach_DocType.txt' "str '\r'"
into table App_Attach_DocType
fields terminated by '#' optionally enclosed by '"'

(ATTACHMENTTYPEID char,
ATTACHMENTTYPE char,
ATTACHMENTGROUP char
 )

