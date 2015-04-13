1. Using ODBC manager (Control panel -> Administrative tools) create
   a SYSTEM data source that points to desired data server (this
   source is needed to specify what ODBC driver is used).

2. In this directory create a configuration file. File name should
   be hostname.conf where hostname is the name of the machine where
   the script runs. File format: text. It should contain four rows:
   ODBC dsn_name
   DB db-name
   USER dbuser
   PASS dbuser_password
   where
   * dsn_name: is the name of the source that has been created on step 1
   * db-name: name of the database (usually ezbob)
   * dbuser: login to connect to the data server (usually sa or ezbobuser)
   * dbuser_password: login's password to the data server (usually *************)

3. Run run.cmd. Observe results.

