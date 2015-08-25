using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    using System.Runtime.Serialization;
    using Models.Enums;

    [DataContract(IsReference = true)]
    public class WizardFinishArgs
    {
        public WizardFinishArgs()
        {
            DoSendEmail = true;
            DoMain = true;
            DoFraud = true;
            NewCreditLineOption = NewCreditLineOption.UpdateEverythingAndApplyAutoRules;
            FraudMode = FraudMode.FullCheck;
            AvoidAutoDecision = 0;
            CashRequestOriginator = CashRequestOriginator.Other;
        }

        [DataMember]
        public int CustomerID { get; set; }

        [DataMember]
        public bool DoSendEmail { get; set; }

        [DataMember]
        public bool DoMain { get; set; }

        [DataMember]
        public bool DoFraud { get; set; }

//        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public NewCreditLineOption NewCreditLineOption { get; set; }

        [DataMember]
        public int AvoidAutoDecision { get; set; }

//        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public FraudMode FraudMode { get; set; }

//        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public CashRequestOriginator CashRequestOriginator { get; set; }
    }
}
