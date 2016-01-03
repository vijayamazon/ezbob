using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Experian.Targeting
{
    public partial class TargetingRequest {
        private static readonly string YES = "Y";
        private static readonly string NO = "N";
        private string companyName;
        private string postCode;
        private string regNum;
        private string isNonLimied;
        private string isLimited;

        public TargetingRequest(string companyName, string postCode, string regNum, bool isLimited) {
            this.companyName = companyName;
            this.postCode = postCode;
            this.regNum = regNum;
            if (isLimited) {
                this.isNonLimied = NO;
                this.isLimited = YES;
            } else {
                this.isNonLimied = YES;
                this.isLimited = NO;
            }
        }

        public string CompanyName {
            get { return this.companyName; }
        }

        public string PostCode
        {
            get { return this.postCode; }
        }

        public string RegNum
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.regNum)) {
                    return string.Empty;
                }

                return this.regNum;
            }
        }

        public string Limited
        {
            get { return this.isLimited; }
        }

        public string NonLimited
        {
            get { return this.isNonLimied; }
        }
    }
}
