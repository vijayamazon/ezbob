namespace EzBob3dParties.Yodlee.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using EzBobCommon.Utils;

    public class YError
    {
        private static readonly string errordetail = "errorDetail";
        public IEnumerable<IDictionary<string, string>> Error { get; set; }

        public string ErrorMessage
        {
            get
            {
                if (CollectionUtils.IsNotEmpty(Error)) {
                    return Error.First()[errordetail];
                }

                return string.Empty;
            }
        }
    }
}
