using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.Models.Transactions
{
    class YCategory
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; } // for example: "Deposits"
        public int categoryTypeId { get; set; }
        public string localizedCategoryName { get; set; }// for example: "Deposits"
        public bool isBusiness { get; set; }
    }
}
