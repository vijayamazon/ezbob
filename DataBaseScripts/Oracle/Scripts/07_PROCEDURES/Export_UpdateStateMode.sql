CREATE OR REPLACE PROCEDURE Export_UpdateStateMode
  (
    pApplicationId in number,
    pStatusMode IN number
  )
AS
BEGIN
  update export_results
     set statusmode = pStatusMode
   where applicationid = pApplicationId;
END;
/

