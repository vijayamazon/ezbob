namespace Ezbob.Backend.ModelsWithDB {
    using System;

    public class IovationLastCheckModel {
        public DateTime? LastCheckDate { get; set; }
        public bool FilledByBroker { get; set; }
        public bool FinishedWizard { get; set; }
    }
}
