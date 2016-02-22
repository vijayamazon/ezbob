namespace TestRailModels.Automation {
    public class AutomationModels {
        public enum Browser {
            None = 0,
            Chrome = 12,
            Firefox = 13,
            IE = 14,
            Safari = 15
        }

        public enum Brand {
            None = 0,
            Alibaba = 9,
            Everline = 8,
            Ezbob = 7
        }

        public enum Environment {
            None = 0,
            Production = 4,
            QA = 6,
            Staging = 5
        }

        public enum Label {
            None = 0,
            Regression = 1,
            Sanity = 2,
            Unlabeled = 3
        }

    }
}
