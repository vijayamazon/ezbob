namespace TestRailModels.Automation {
    using System;
    using System.Collections.Generic;
    using TestRailModels.TestRail;

    /// <summary>stores information about a case</summary>
    [Serializable]
    public class AtutomationCaseRun 
    {
        public AtutomationCaseRun() {
            this.Browser = AutomationModels.Browser.None;
            this.Brand = AutomationModels.Brand.None;
            this.Environment = AutomationModels.Environment.None;
        }

        public ulong RunId { get; set; }
        public Case CaseBase { get; set; }
        public ulong SuiteId { get; set; }        
        public AutomationModels.Browser Browser { get; set; }
        public AutomationModels.Brand Brand { get; set; }
        public AutomationModels.Environment Environment { get; set; }
        public ResultStatus ResultStatus { get; set; }
        public List<ulong> Dependencies { get; set; }
    }

    }

