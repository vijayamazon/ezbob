using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public enum CashRequestOriginator {
        // When customer completes wizard in a standard way
        [EnumMember]
        [Description("Finished wizard")]
        FinishedWizard = 1,

        [EnumMember]
        [Description("Quick offer")]
        QuickOffer = 2,

        [EnumMember]
        [Description("Dashboard request cash button")]
        RequestCashBtn = 3,

        [EnumMember]
        [Description("UW new credit line button")]
        NewCreditLineBtn = 4,

        [EnumMember]
        [Description("Other")]
        Other = 5,

        [EnumMember]
        [Description("RequalifyCustomerStrategy")]
        RequalifyCustomerStrategy = 6,

        // When wizard is completed for customer from underwriter (Finish wizard button)
        [EnumMember]
        [Description("Forced wizard completion")]
        ForcedWizardCompletion = 7,

        [EnumMember]
        [Description("Approved")]
        Approved = 8,

        [EnumMember]
        [Description("Manual strategy activation")]
        Manual = 9,

        [EnumMember]
        [Description("UW new credit line button and selected 'Skip all' option")]
        NewCreditLineSkipAll = 10,

        [EnumMember]
        [Description("UW new credit line button and selected 'Skip all and go auto' option")]
        NewCreditLineSkipAndGoAuto = 11,

        [EnumMember]
        [Description("UW new credit line button and selected 'Update all and go manual' option")]
        NewCreditLineUpdateAndGoManual = 12,

        [EnumMember]
        [Description("UW new credit line button and selected 'Update all and go auto' option")]
        NewCreditLineUpdateAndGoAuto = 13,
    }
}
