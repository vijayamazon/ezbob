load data
infile 'security_user_test.txt' "str '\r'"
append
into table Security_User
fields terminated by '#' optionally enclosed by '"'
(userID char,
USERNAME char,
FULLNAME char,
PASSWORD char,
CREATIONDATE DATE "YYYY-MM-DD HH24:MI:SS" ,
ISDELETED char,
EMAIL char  ,
CREATEUSERID char ,
DELETIONDATE DATE "YYYY-MM-DD HH24:MI:SS"   ,
DELETEUSERID char ,
BRANCHID char,
PASSSETTIME DATE "YYYY-MM-DD HH24:MI:SS",
LOGINFAILEDCOUNT char,
DISABLEDATE DATE "YYYY-MM-DD HH24:MI:SS",
LASTBADLOGIN DATE "YYYY-MM-DD HH24:MI:SS",
PASSEXPPERIOD char,
FORCEPASSCHANGE char,
DISABLEPASSCHANGE char,
CERTIFICATETHUMBPRINT char
 )