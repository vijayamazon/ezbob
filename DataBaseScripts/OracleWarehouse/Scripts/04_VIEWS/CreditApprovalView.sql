create or replace view creditapprovalview as
select
  CreditApproval.Id,
  Dictsex.Value as Sex,
  CreditApproval.Age,
  CreditApproval.AddressTime,
  DictMaritalStatus.Value as MAritalStatus,
  CreditApproval.JobTime,
  DictChecking.Value as Checking,
  DictSavings.Value as Savings,
  CreditApproval.PaymentHistory,
  Dicthomeown.Value as HomeOwnership,
  CreditApproval.FinRatio1,
  CreditApproval.FinRatio2,
  CAHistFacts.Score,
  DictRisk.Value as Risk,
  CA_H2HR.HistoryRecordId as HISTORYRECORDID,
  CAHISTFACTS.Id as HISTORICALID
from CreditApproval
  INNER JOIN CAHistFacts ON CreditApproval.ID = CAHistFacts.Masterid
  LEFT OUTER JOIN DictSex ON CreditApproval.Sex = DictSex.ID
  LEFT OUTER JOIN DictMaritalStatus ON CreditApproval.MaritalStatus = DictMaritalStatus.ID
  LEFT OUTER JOIN DictChecking ON CreditApproval.Checking = DictChecking.ID
  LEFT OUTER JOIN DictSavings ON CreditApproval.Savings = DictSavings.ID
  LEFT OUTER JOIN DictHomeOwn ON CreditApproval.HomeOwnership = DictHomeOwn.ID
  LEFT OUTER JOIN DictRisk ON CAHistFacts.Risk = DictRisk.ID
  INNER JOIN CA_H2HR ON CA_H2HR.HISTORICALID = CAHistFacts.ID
ORDER BY CreditApproval.Id
/