CREATE or replace procedure Log_TraceLogInsert
-- Created by A.Grechko
-- Date 12.02.2008

   (pApplicationId number, 
   pType number, 
   pCode number, 
   pMessage varchar2, 
   pData varchar2, 
   pInsertDate date, 
   pThreadId number )
as

begin
    INSERT into Log_TraceLog (id,
                              ApplicationId, Type, Code, 
                              Message, Data, InsertDate, ThreadId
                              ) 
    VALUES (seq_log_tracelog.nextval , pApplicationId, pType, pCode, 
            pMessage, pData, pInsertDate, pThreadId);
            
    
end ;
/