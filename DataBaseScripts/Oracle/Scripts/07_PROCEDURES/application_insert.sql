CREATE OR REPLACE PROCEDURE Application_Insert
(
    pUserId IN NUMBER,
    pStrategyId IN NUMBER,
    pParentAppId IN NUMBER,
    pApplicationId OUT NUMBER
)
AS
  l_Application_id Number;
  l_DetailNameID   Number;
  l_RootDetailID    Number;
  l_var            number;
  realCounter      number;

BEGIN
         if pParentAppId is null then
            Select seq_app_realcounter.nextval into realCounter from dual;
         else
            select appcounter into realCounter from application_application
            where applicationid = pParentAppId;
         end if;

         Select seq_application_application.nextval into l_Application_id from dual;
         insert into Application_Application
          (ApplicationId, CreationDate, CreatorUserId, StrategyId, ParentAppId, appcounter)
         values
          (l_Application_id, sysdate, pUserId, pStrategyId, pParentAppId, realCounter);


          select DetailNameId into l_DetailNameID
          from Application_DetailName
          where Application_DetailName.Name = 'Root';

          Select seq_app_detail.nextval into l_RootDetailID from dual;
          INSERT INTO Application_Detail
           (Detailid, ApplicationId, DetailNameId, ParentDetailId, ValueStr, ValueNum, ValueDateTime, IsBinary)
          VALUES
            (l_RootDetailID, l_Application_id , l_DetailNameID , null , null , null , null , null);

          select DetailNameId into l_DetailNameID
          from Application_DetailName
          where Application_DetailName.Name = 'Body' ;

          Select seq_app_detail.nextval into l_var from dual;
          INSERT INTO Application_Detail
            (Detailid, ApplicationId, DetailNameId,ParentDetailId ,ValueStr ,ValueNum ,ValueDateTime ,IsBinary)
          VALUES
            (l_var, l_Application_id , l_DetailNameID , l_RootDetailID , null , null , null , null);


 pApplicationId := l_Application_id;


END Application_Insert;
/