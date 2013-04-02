using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class FunctionValueModel
    {
        public FunctionValueModel(MP_AnalyisisFunctionValue v)
        {
            Id = (int)v.Id;
            Name = v.AnalyisisFunction.Name;
            Value = v.Value;
        }

        public string Value { get; set; }
        public int Id { get; set; } 
        public string Name { get; set; } 
    }
}