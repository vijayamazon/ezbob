namespace Ezbob.Backend.ModelsWithDB.Wrappers {
    using System.Runtime.Serialization;
    [DataContract(IsReference = true)]
    public class IntWrapper {
		[DataMember]
		public int Value { get; set; }
	} 
} 

