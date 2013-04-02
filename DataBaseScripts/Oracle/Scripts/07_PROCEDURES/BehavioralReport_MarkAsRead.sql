create or replace procedure BehavioralReport_MarkAsRead
(
pReportId in number
)
as

begin

UPDATE BehavioralReports
SET 
ISNOTRead = 0
WHERE
ID = pReportId;

end BehavioralReport_MarkAsRead;
/
