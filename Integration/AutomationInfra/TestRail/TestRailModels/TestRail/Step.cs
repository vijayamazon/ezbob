namespace TestRailModels.TestRail {
    using System;
    using Newtonsoft.Json.Linq;

    /// <summary>stores information about a step</summary>
    [Serializable]
    public class Step {
        #region Public Properties
        /// <summary>description of the step</summary>
        public string Description;
        /// <summary>expected result for the step</summary>
        public string Expected;
        /// <summary>actual result for the step</summary>
        public string Actual;
        /// <summary>result of the step</summary>
        public ResultStatus? Status;
        #endregion

        #region Public Methods
        /// <summary>parses json into a step</summary>
        /// <param name="json">json to parse</param>
        /// <returns>step corresponding to the json</returns>
        public static Step Parse(JObject json) {
            Step s = new Step();
            s.Description = (string)json["content"];
            s.Expected = (string)json["expected"];
            s.Actual = (string)json["actual"];
            s.Status = (null != (int?)json["status_id"]) ? (ResultStatus)((int)json["status_id"]) : (ResultStatus?)null;
            return s;
        }

        /// <summary>Get the json object that describes this class</summary>
        /// <returns>json object for this class</returns>
        public JObject GetJsonObject() {
            dynamic json = new JObject();
            if (!string.IsNullOrWhiteSpace(this.Description)) { json.content = this.Description; }
            if (!string.IsNullOrWhiteSpace(this.Expected)) { json.expected = this.Expected; }
            if (!string.IsNullOrWhiteSpace(this.Actual)) { json.actual = this.Actual; }
            if (null != this.Status) { json.status_id = (int)this.Status; }
            return json;
        }
        #endregion
    }
}
