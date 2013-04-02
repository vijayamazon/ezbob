set echo off

define TNS_NAME=&1
define SYSTEMPASS=&2
define SCHEMA_NAME=&3
define TS_TAB=&4
define TS_INDEX=&5

spool KernelSetup.log

connect SYSTEM/&&SYSTEMPASS.@&&TNS_NAME. 

prompt Processing SYS_TASKS...

@@SYS_TASKS/runall.sql


connect SYSTEM/&&SYSTEMPASS.@&&TNS_NAME. 

prompt Running SYSRECOMPILE...

@@SYSRECOMPILE/runall.sql

prompt Running User's tasks

connect &&SCHEMA_NAME./&&SCHEMA_NAME.@&&TNS_NAME.

prompt Creating sequences...

@@01_SEQUENCES/runall.sql

prompt Creating tables...

@@02_TABLES/runall.sql

prompt Creating views...

@@04_VIEWS/runall.sql

prompt Creating Packages...

@@05_PACKAGES/runall.sql

prompt Creating Package Bodies...

@@06_PACKAGEBODIES/runall.sql

prompt Creating procedures...

@@07_PROCEDURES/runall.sql

prompt Creating Functions...

@@08_FUNCTIONS/runall.sql

prompt Creating triggers...

@@09_TRIGGERS/runall.sql

prompt Setting constraints...

@@10_CONSTRAINTS/runall.sql

prompt Running SYSRECOMPILE...

@@SYSRECOMPILE/runall.sql

spool off

exit
