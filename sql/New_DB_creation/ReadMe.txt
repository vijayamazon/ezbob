To create a new DB:
1. Connect to 'master' DB
2. For sql server 2012 - Run '01 - schema (for sql server 2012).sql' 
   For sql server 2008 - Run '01 - schema (for sql server 2008).sql' 
   (To change DB name replace [ezbob] with [otherdbname], and change the name of the files at the top of the script)
3. Run '02 - SkeletonData.sql'
4. Run the relevant insert statement:
	INSERT INTO ConfigurationVariables VALUES ('Environment', 'Dev', 'Defines the environment of the DB')
	OR
	INSERT INTO ConfigurationVariables VALUES ('Environment', 'QA', 'Defines the environment of the DB')
	OR
	INSERT INTO ConfigurationVariables VALUES ('Environment', 'UAT', 'Defines the environment of the DB')
	OR
	INSERT INTO ConfigurationVariables VALUES ('Environment', 'Prod', 'Defines the environment of the DB')
5. Update sql\<machine-name>.conf to the new db name
6. Execute all the new sqls (After Nov 13 release)
7. Execute this query: INSERT INTO ServiceRegistration ([key]) VALUES ('\\<machine-name>\<Path of scorto service executable>')
8. Update the environment config file
