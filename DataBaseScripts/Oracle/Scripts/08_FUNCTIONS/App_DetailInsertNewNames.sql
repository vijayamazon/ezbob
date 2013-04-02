CREATE OR REPLACE function App_DetailInsertNewNames
                                  (pDetailName    VARCHAR2)
 return number
AS
  l_app_detailnameid   number;
begin
     select seq_app_detailname.nextval into l_app_detailnameid from dual;
     INSERT INTO Application_DetailName
       (detailnameid, Name)
     VALUES
       (l_app_detailnameid, pDetailName);

     return l_app_detailnameid;

END;
/
