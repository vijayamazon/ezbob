CREATE OR REPLACE FUNCTION GetDictionaryReferenceCount
(pDictionaryId number)
  return number 
  
  as
  
  l_customers number;
  l_customer_hist number;
  l_accounts number;
  l_account_hist number;
  l_dictionaries number;

begin

    select count(*) 
      into l_dictionaries
      from DICTIONARYPARAMS
     where MASTERDICTIONARYID = pDictionaryId
     and NOT(DICTIONARYID = pDictionaryId);

    select count(*) 
      into l_customers
      from CustomerTypeParams
     where DictionaryId = pDictionaryId;

    select count(*)
      into l_customer_hist
      from CustomerTypeHistoricalParams
     where DictionaryId = pDictionaryId;

    select count(*) 
      into l_accounts
      from AccountTypeParams
     where DictionaryId = pDictionaryId;

    select count(*)
      into l_account_hist
      from AccountTypeHistoricalParams
     where DictionaryId = pDictionaryId;

  return 
l_customers + l_customer_hist + l_accounts + l_account_hist + l_dictionaries;


end GetDictionaryReferenceCount;
/
