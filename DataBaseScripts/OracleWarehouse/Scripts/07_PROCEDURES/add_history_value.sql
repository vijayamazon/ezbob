CREATE OR REPLACE PROCEDURE add_history_value
(
 pLoadDate IN date,
 pName IN VARCHAR2,
 pReference IN NUMBER,
 pLoadtype IN NUMBER,
 pHistoryTableName IN VARCHAR2,
 pHistorySequenceName IN VARCHAR2,
 pHistoryId OUT NUMBER
)
as
 l_seq_history Number;
begin
   execute immediate 'select ' || pHistorySequenceName || '.Nextval from dual' into l_seq_history;

   execute immediate ' insert into ' || pHistoryTableName ||
                         ' (id, HRDate, HRLoadDate, HRName, HRECORDALIAS, HRTYPE)
                       values
                          (:l_seq_history, sysdate, :pLoadDate, :pName, :pReference, :pLoadtype)'
   using l_seq_history, pLoadDate, pName, pReference, pLoadtype;
   pHistoryId := l_seq_history;
END add_history_value;
/
