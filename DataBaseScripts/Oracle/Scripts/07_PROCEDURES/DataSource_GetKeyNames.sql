CREATE OR REPLACE Procedure DataSource_GetKeyNames
(
	cur_OUT out sys_refcursor
)
AS
begin
  open cur_OUT for
	SELECT * FROM DataSource_KeyFields;
end;
/