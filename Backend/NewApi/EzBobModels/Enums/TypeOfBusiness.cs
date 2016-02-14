using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums
{
    using System.ComponentModel;

    public enum TypeOfBusiness
    {
        [Description("Sole trader (not Inc.)")]
        Entrepreneur = 0, //consumer
        [Description("Limited liability partnership")]
        LLP = 1,          //company
        [Description("Partnership (less than three)")]
        PShip3P = 2,      //consumer
        [Description("Partnership (more than three)")]
        PShip = 3,        //company
        [Description("Limited company")]
        Limited = 5       //company
    }
}
