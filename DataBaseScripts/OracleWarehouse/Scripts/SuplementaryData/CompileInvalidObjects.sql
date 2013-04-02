SET FEEDBACK OFF
SET SERVEROUTPUT ON

BEGIN
  DBMS_OUTPUT.DISABLE;
  DBMS_OUTPUT.ENABLE(2000000);
  DBMS_OUTPUT.PUT_LINE(CHR(13) || 'Invalid objects recompilation...');
  DBMS_OUTPUT.PUT_LINE(dbtools.CompileInvalidObjects('DBO') || ' invalid objects left');
END;
/

EXIT;
