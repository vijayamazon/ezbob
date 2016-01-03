using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.Transactions
{
    class YInvestmentTransactionView
    {
        public YLotHandling lotHandling { get; set; }
        public YHoldingType holdingType { get; set; }
        public decimal netCost { get; set; }
    }
}
