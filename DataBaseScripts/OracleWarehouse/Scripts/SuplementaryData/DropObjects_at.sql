WHENEVER SQLERROR EXIT 
SET FEEDBACK OFF
SET SERVEROUTPUT ON
set echo off
set verify off
DEFINE LOG_filE =&1
define sysdba_name =&2
define passwd=&3
define DUMP_NAME=&4
var cnf varchar2(100);

prompt
prompt ******************************************************************************
prompt *
prompt *         	BE  CAREFUL !!!! 
prompt *        
prompt *      THIS PROCEDURE IS GOING TO DROP ALL USER'S OBJECTS
prompt *
prompt *
prompt *     	Please confirm this operation...
prompt *                 Press  Y  to perform this operation
prompt *
prompt ******************************************************************************
prompt

accept cnf prompt ' Default value N (exit): '

SPOOL &LOG_filE

DECLARE

  Stop_process EXCEPTION;

  PROCEDURE DropUser(pUser VARCHAR2, pDropSynonyms BOOLEAN DEFAULT FALSE)
  IS
    lCount INTEGER;
  BEGIN
    DBMS_OUTPUT.PUT_LINE('Drop user ' || pUser);
    lCount := dbtools.DropAll(pUser, pDropSynonyms);
    IF lCount > 0 THEN
       DBMS_OUTPUT.PUT_LINE(lCount || ' objects left');
    END IF;
  END;




BEGIN

  DBMS_OUTPUT.DISABLE;
  DBMS_OUTPUT.ENABLE(2000000);

  if (upper('&cnf') <> 'Y') OR ('&cnf'  IS null) then 
    DBMS_OUTPUT.PUT_LINE(' User stopped process ');
    RAISE Stop_process;
  end if;

  DBMS_OUTPUT.PUT_LINE(' User CONTINUE process ');
   DropUser('DBO', TRUE);
   DropUser('MAXRATE');
   DropUser('USAGE', TRUE);
--  DropUser('REPORT');
/*  EXCEPTION
   WHEN Stop_process THEN
      DBMS_OUTPUT.PUT_LINE('User stopped process');
      RAISE;            */
END;
/

SPOOL OFF;

host imp &sysdba_name/&PASSWD@&5 file=&4  fromuser=(dbo, maxrate, USAGE) touser=(dbo, maxrate, USAGE) log=import.log statistics=none;


EXIT
/