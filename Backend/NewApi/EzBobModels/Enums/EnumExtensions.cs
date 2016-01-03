using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels.Enums {
    using System.ComponentModel;

    //TODO: may be we should find description in some other way (by using globalization service)
    public static class EnumExtensions {
        /// <summary>
        /// Gets the description attribute of enum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetDescriptionAttribute(this Enum value) {
            DescriptionAttribute attribute = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .SingleOrDefault() as DescriptionAttribute;
            return attribute == null ? value.ToString() : attribute.Description;
        }
    }
}
