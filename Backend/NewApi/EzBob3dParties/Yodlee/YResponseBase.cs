using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee
{
    using EzBob3dParties.Yodlee.Models;

    abstract class YResponseBase {
        private YError error;

        public void SetError(YError yError) {
            this.error = yError;
        }

        public YError GetError() {
            return this.error;
        }

        public bool HasError
        {
            get { return this.error != null; }
        }
    }
}
