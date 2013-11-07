To create a new DB:
1. Connect to 'master' DB
2. Run '01 - schema.sql' (To change DB name replace [ezbob] with [otherdbname], and change the name of the files at the top of the script)
3. Run '02 - SkeletonData.sql'
4. Update sql\<machine-name>.conf to the new db name
5. Execute all the new sqls (After Nov 13 release)
6. Update the environment config file
7. Run publisher