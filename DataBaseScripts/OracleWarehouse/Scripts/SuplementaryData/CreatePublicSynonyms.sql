SET FEEDBACK OFF

BEGIN
  dbtools.GeneratePublicSynonyms('DBO');
  dbtools.GeneratePublicSynonyms('USAGE');
END;
/

EXIT;
