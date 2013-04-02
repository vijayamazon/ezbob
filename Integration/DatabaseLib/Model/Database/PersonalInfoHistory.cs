using System;

namespace EZBob.DatabaseLib.Model.Database
{
   public  class PersonalInfoHistory
    {
     
       public virtual int Id { get; set; }
       public virtual Customer Customer { get; set; }
        public virtual  string FieldName { get; set; }
        public virtual  string OldValue { get; set; }
        public  virtual string NewValue { get; set; }
        public virtual  DateTime DateModifed { get; set; }
        public virtual string AddressId { get; set; }
    }
}
