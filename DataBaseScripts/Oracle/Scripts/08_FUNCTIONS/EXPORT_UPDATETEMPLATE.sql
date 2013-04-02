CREATE OR REPLACE FUNCTION Export_UpdateTemplate
  (
    pId in number,
    pFileName in varchar2,
    pDescription in varchar2,
    pExceptionType in number,
    pBinaryBody in blob,
    pVariablesXml in clob,
    pUserId in number,
    pDisplayName in varchar2,
    pSignedDocument in clob
  ) return number
AS
 l_Count NUMBER;
 l_Result NUMBER;

BEGIN
   IF pId is null THEN
   BEGIN
      SELECT COUNT("ID") INTO l_Count  FROM  export_templateslist
      WHERE UPPER(filename) = UPPER(pFileName) AND isdeleted IS NULL;

      IF l_Count > 0 THEN
         raise_application_error(-20000, 'dublicated_name');
      ELSE
         update export_templateslist set terminationdate = sysdate
         where upper(displayname) = upper(pDisplayName)
           and terminationdate is null;

         select seq_export_templatelist.nextval into l_Result from dual;
            
         insert into export_templateslist
            (id, filename, description, binarybody, variablesxml, exceptiontype, userId, displayname, SignedDocument)
         values
            (l_Result, pFileName, pDescription, pBinaryBody, pVariablesXml, pExceptionType, pUserId, pDisplayName, pSignedDocument);
      END IF;

   END;
   ELSE
   BEGIN
      update export_templateslist
         set description = pDescription,
             exceptiontype = pExceptionType
      where id = pId;
      l_Result := pId;
   END;
   END IF;
   RETURN l_Result;
END;
/

