CREATE OR REPLACE Procedure Get_Application_Results
(
  pApplicationId IN number,
  cur_OUT in out sys_refcursor
) 
AS
BEGIN

  OPEN cur_OUT FOR
    select *
    from Application_Result
    where ApplicationId = pApplicationId;
END;
/

