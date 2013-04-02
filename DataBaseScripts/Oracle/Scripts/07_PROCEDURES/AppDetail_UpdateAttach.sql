

create or replace  PROCEDURE AppDetail_UpdateAttach

-- created by A.Grechko
-- date 25.12.2007

 ( pApplicationId    number ,
  pParentDetailId  number ,
  pDetailName    VARCHAR2 ,
  pNewValueStr       VARCHAR2 )
  
AS

   pDetailIdUpdate number; 

BEGIN

begin

  SELECT  ad.DetailId into pDetailIdUpdate
  FROM Application_Detail ad
  INNER JOIN Application_DetailName adn 
      ON ad.DetailNameId = adn.DetailNameId
  WHERE  ad.ApplicationId = pApplicationId
      and ad.ParentDetailId = pParentDetailId
      and adn.Name = pDetailName ;

exception 
  
  when no_data_found 
  then pDetailIdUpdate := null;

end;

  UPDATE Application_Detail
  SET ValueStr = pNewValueStr
  WHERE DetailId = pDetailIdUpdate;
  
END;
/