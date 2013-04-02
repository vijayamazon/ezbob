SET FEEDBACK OFF

CREATE OR REPLACE PACKAGE DBO.DbTools
-- AlexTim 03/2002
IS
  PROCEDURE ExecSQL(pSQL VARCHAR2);  
  FUNCTION GetInvalidObjectsCount(SchemaName VARCHAR2) RETURN NUMBER;
  FUNCTION CompileInvalidObjects(SchemaName VARCHAR2) RETURN NUMBER;  
  FUNCTION GetObjectCount(SchemaName VARCHAR2, ObjectType VARCHAR2) RETURN NUMBER;
  FUNCTION DropObjects(SchemaName VARCHAR2, ObjectType VARCHAR2) RETURN NUMBER;
  FUNCTION DropAll(SchemaName VARCHAR2, PublicSynonymFlag BOOLEAN DEFAULT FALSE) RETURN NUMBER;
  PROCEDURE GeneratePublicSynonyms(SchemaName VARCHAR2, ObjectType VARCHAR2 DEFAULT NULL);
END;
/


CREATE OR REPLACE PACKAGE BODY DBO.DbTools
IS

PROCEDURE ExecSQL(pSQL VARCHAR2)
IS
BEGIN
  EXECUTE IMMEDIATE pSQL;
EXCEPTION
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE(pSQL);
    DBMS_OUTPUT.PUT_LINE(SQLERRM);
END;


FUNCTION GetInvalidObjectsCount(SchemaName VARCHAR2) RETURN NUMBER
AS
  lCount NUMBER;
BEGIN
  SELECT COUNT(*) INTO lCount
    FROM ALL_OBJECTS
   WHERE Owner = UPPER(SchemaName)
     AND Status = 'INVALID';
  RETURN lCount;
END;


FUNCTION CompileInvalidObjects(SchemaName VARCHAR2) RETURN NUMBER
AS
  lNoObj BOOLEAN;
  lSQL   VARCHAR2(8000);
BEGIN
  FOR i IN 1..6 LOOP
    lNoObj := FALSE;
    FOR O IN (SELECT 'ALTER ' || DECODE (OBJECT_TYPE, 'PACKAGE BODY', 'PACKAGE', OBJECT_TYPE)
                     || ' ' || OWNER || '.' || OBJECT_NAME || ' COMPILE '
                     || DECODE (OBJECT_TYPE, 'PACKAGE BODY', 'BODY ', '') AS SQLCommand, OBJECT_NAME
                FROM ALL_OBJECTS
               WHERE Owner = UPPER(SchemaName)
                 AND Status = 'INVALID')
    LOOP
      lNoObj := TRUE;
      lSQL := O.SQLCommand;
      BEGIN
        EXECUTE IMMEDIATE lSQL;
      EXCEPTION
        WHEN OTHERS THEN
          DBMS_OUTPUT.PUT_LINE(lSQL);
          DBMS_OUTPUT.PUT_LINE(SQLERRM);
      END;
    END LOOP;
    EXIT WHEN lNoObj = FALSE;
  END LOOP;
  RETURN GetInvalidObjectsCount(SchemaName);
END;


FUNCTION GetObjectCount(SchemaName VARCHAR2, ObjectType VARCHAR2) RETURN NUMBER
IS
  lCount INTEGER := 0;
BEGIN
  IF ObjectType = 'SYNONYM' THEN
     SELECT COUNT(*) INTO lCount
       FROM ALL_SYNONYMS
      WHERE TABLE_OWNER = UPPER(SchemaName);
  ELSIF ObjectType = 'PUBLIC SYNONYM' THEN
     SELECT COUNT(*) INTO lCount
       FROM ALL_SYNONYMS
      WHERE OWNER = 'PUBLIC'
        AND TABLE_OWNER = UPPER(SchemaName);
  ELSE
     SELECT COUNT(*) INTO lCount
       FROM ALL_OBJECTS
      WHERE OWNER = UPPER(SchemaName)
        AND OBJECT_TYPE = UPPER(ObjectType)
        AND OBJECT_NAME <> 'DBTOOLS';
  END IF;
  RETURN lCount;
END;


FUNCTION DropObjects(SchemaName VARCHAR2, ObjectType VARCHAR2) RETURN NUMBER
IS
BEGIN
  IF ObjectType = 'SYNONYM' THEN
     FOR O IN (SELECT SYNONYM_NAME FROM ALL_SYNONYMS WHERE OWNER <> 'PUBLIC' AND TABLE_OWNER = UPPER(SchemaName)) LOOP
       ExecSQL('DROP SYNONYM ' || O.SYNONYM_NAME);
     END LOOP;
  ELSIF ObjectType = 'PUBLIC SYNONYM' THEN
     FOR O IN (SELECT SYNONYM_NAME FROM ALL_SYNONYMS WHERE OWNER = 'PUBLIC' AND TABLE_OWNER = UPPER(SchemaName)) LOOP
       ExecSQL('DROP PUBLIC SYNONYM ' || O.SYNONYM_NAME);
     END LOOP;
  ELSIF ObjectType = 'JOB' THEN
     FOR O IN (SELECT JOB FROM ALL_JOBS WHERE SCHEMA_USER = UPPER(SchemaName) AND PRIV_USER = UPPER(SchemaName)) LOOP
       ExecSQL('BEGIN SYS.DBMS_JOB.REMOVE(' || O.JOB || ');END;');
     END LOOP;
  ELSE
     FOR O IN (SELECT OBJECT_TYPE, OBJECT_NAME
                 FROM ALL_OBJECTS
                WHERE OWNER = UPPER(SchemaName)
                  AND OBJECT_TYPE = UPPER(ObjectType)
                  AND OBJECT_NAME <> 'DBTOOLS')
     LOOP
       IF ObjectType = 'TABLE' THEN
          ExecSQL('DROP TABLE ' || SchemaName || '.' || O.OBJECT_NAME || ' CASCADE CONSTRAINT');
       ELSE
          ExecSQL('DROP ' || O.OBJECT_TYPE || ' ' || SchemaName || '.' || O.OBJECT_NAME);
       END IF;
     END LOOP;
  END IF;

  RETURN GetObjectCount(SchemaName, ObjectType);
END;


FUNCTION DropAll(SchemaName VARCHAR2, PublicSynonymFlag BOOLEAN DEFAULT FALSE) RETURN NUMBER
IS
  lPublicSynonyms NUMBER := 0;
BEGIN
  IF PublicSynonymFlag THEN
     lPublicSynonyms := DropObjects(SchemaName, 'PUBLIC SYNONYM');
  END IF;

  RETURN DropObjects(SchemaName, 'TABLE')
       + DropObjects(SchemaName, 'SEQUENCE')
       + DropObjects(SchemaName, 'PROCEDURE')
       + DropObjects(SchemaName, 'FUNCTION')
       + DropObjects(SchemaName, 'PACKAGE')
       + DropObjects(SchemaName, 'PACKAGE')
       + DropObjects(SchemaName, 'PACKAGE BODY')
       + DropObjects(SchemaName, 'VIEW')
       + DropObjects(SchemaName, 'SYNONYM')
       + DropObjects(SchemaName, 'TYPE')
       + DropObjects(SchemaName, 'TRIGGER')
       + DropObjects(SchemaName, 'INDEX')
       + DropObjects(SchemaName, 'JOB')
       + lPublicSynonyms;
END;


PROCEDURE GeneratePublicSynonyms(SchemaName VARCHAR2, ObjectType VARCHAR2 DEFAULT NULL)
IS
BEGIN
  FOR C IN (SELECT OBJECT_NAME 
              FROM ALL_OBJECTS  
             WHERE OWNER = UPPER(SchemaName)
               AND (NOT ObjectType IS NULL AND OBJECT_TYPE = ObjectType OR  
                    OBJECT_TYPE IN ('TABLE', 
                                    'VIEW', 
                                    'PROCEDURE', 
                                    'FUNCTION', 
                                    'PACKAGE', 
                                    'SEQUENCE')))
  LOOP
    ExecSQL('CREATE PUBLIC SYNONYM ' || C.OBJECT_NAME || ' FOR ' || SchemaName || '.' || C.OBJECT_NAME); 
  END LOOP;
END;

END;
/

EXIT;
