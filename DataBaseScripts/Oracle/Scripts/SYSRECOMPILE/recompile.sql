-----------------------------------------------------------------------
-- Название: recompile.sql 
-----------------------------------------------------------------------
-- Назначение: Компилирует невалидные объекты в текущей схеме
-----------------------------------------------------------------------
-- Число: 2 января 1998 г.
-----------------------------------------------------------------------
-- Выполнять из: SQL*Plus
-- Выполнять от имени пользователя: имя владельца схемы
-----------------------------------------------------------------------
  PROMPT Executing recompile.sql
--=====================================================================
-- NB:
-- 1. При работе не создает временного файла и не использует спулинг
--    в файл, поэтому может использоваться в составе SQL скриптов,
--    которые записывают результаты работы в файл.  Данный скрипт не
--    нарушит записи результатов работы родительского скрипта.
-- 2. Перекомпилирует все объекты, которые могут быть перекомпилированы,
--    за один запуск.  Для этого делает несколько итераций, если
--    необходимо.
-----------------------------------------------------------------------
SET SERVEROUTPUT ON SIZE 100000
SET PAGESIZE 0
SET LINESIZE 80
COLUMN COUNT(*) FORMAT '9999999' HEADING 'NUM'

PROMPT
SELECT COUNT(*), ' Invalid Object(s) in Schema ' || USER || '.'
   FROM USER_OBJECTS
   WHERE STATUS <> 'VALID'
     AND STATUS <> 'N/A'
     AND OBJECT_TYPE NOT LIKE 'TABLE%'
     AND OBJECT_TYPE NOT LIKE 'INDEX%';

PROMPT Now Recompiling...
PROMPT

declare
--
   cursor invobjC is
      SELECT ('ALTER '
               || DECODE(OBJECT_TYPE, 'PACKAGE BODY', 'PACKAGE', OBJECT_TYPE)
               || ' ' || OBJECT_NAME || ' COMPILE'
               || DECODE(OBJECT_TYPE, 'PACKAGE BODY', ' BODY', '') ) recomp_statement,
             (OBJECT_TYPE || ' ' || OBJECT_NAME) invalid_object,
              OBJECT_NAME object_name,
              OBJECT_TYPE object_type
         FROM USER_OBJECTS
         WHERE STATUS <> 'VALID'
           AND STATUS <> 'N/A'
           AND OBJECT_TYPE NOT LIKE 'TABLE%'
           AND OBJECT_TYPE NOT LIKE 'INDEX%';
--    
   recomp_handle integer;
   num_of_errors integer;
   iteration     integer;
   last_invalid_objects integer;
   curr_invalid_objects integer;
--
begin
--
   recomp_handle  := 0;
   iteration      := 0;
   last_invalid_objects := 0;
--   
   select count(*)
      into curr_invalid_objects
      from user_objects
      where status <> 'VALID'
        and status <> 'N/A'
        and object_type not like 'TABLE%'
        and object_type not like 'INDEX%';
--   
   while last_invalid_objects <> curr_invalid_objects loop  /* Do compile interations until */
      iteration := iteration + 1;                           /* all objects that could be    */
      dbms_output.put_line('--- Iteration ' || to_char(iteration) || ':   ' ||
                           to_char(curr_invalid_objects) || ' Invalid Object(s) Left');
                                                            /* compiled are compiled.       */
      for InvObjRec in invobjC loop
--         
         recomp_handle := dbms_sql.open_cursor;
--         
         if dbms_sql.is_open(recomp_handle) then
--            
            begin                              /* Try to recompile object */
               dbms_sql.parse(recomp_handle, InvObjRec.recomp_statement, dbms_sql.native);
            exception when others then
               if SQLCODE <> -24344 then       /* Ignore 24344 error: compiled with error */
                  raise;
               end if;
            end;
            dbms_sql.close_cursor(recomp_handle);
--            
            select count(*)
               into num_of_errors
               from user_errors
               where name = InvObjRec.Object_Name
                 and type = InvObjRec.Object_Type;
            if num_of_errors = 0 then
               dbms_output.put_line(InvObjRec.invalid_object || ' -- OK.');
            else
               dbms_output.put_line('');
               dbms_output.put_line('Warning: ' || InvObjRec.invalid_object ||
                                    ' COMPILED WITH ERRORS.');
            end if;
--         
         else
--            
            dbms_output.put_line('Error compiling ' || InvObjRec.invalid_object || '.');
--         
         end if;
--         
      end loop;  /* An iteration */
--      
      last_invalid_objects := curr_invalid_objects;
      select count(*)
         into curr_invalid_objects
         from user_objects
         where status <> 'VALID'
           and status <> 'N/A'
           and object_type not like 'TABLE%'
           and object_type not like 'INDEX%';
--      
   end loop;
--      
   if dbms_sql.is_open(recomp_handle) then
      dbms_sql.close_cursor(recomp_handle);
   end if;
--      
   exception
      when others then
         if dbms_sql.is_open(recomp_handle) then
         dbms_sql.close_cursor(recomp_handle);
      end if;
     raise;
     return;
--   
end;
/

SET PAGESIZE 30
