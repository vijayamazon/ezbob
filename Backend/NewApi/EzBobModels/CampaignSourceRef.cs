using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    using System.Runtime.Serialization;

    [DataContract]
    public class CampaignSourceRef
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        [DataMember]
        public string FUrl { get; set; }
        [DataMember]
        public string FSource { get; set; }
        [DataMember]
        public string FMedium { get; set; }
        [DataMember]
        public string FTerm { get; set; }
        [DataMember]
        public string FContent { get; set; }
        [DataMember]
        public string FName { get; set; }
        [DataMember]
        public DateTime? FDate { get; set; }
        [DataMember]
        public string RUrl { get; set; }
        [DataMember]
        public string RSource { get; set; }
        [DataMember]
        public string RMedium { get; set; }
        [DataMember]
        public string RTerm { get; set; }
        [DataMember]
        public string RContent { get; set; }
        [DataMember]
        public string RName { get; set; }
        [DataMember]
        public DateTime? RDate { get; set; }
    }
}

/*
 Id	int	Unchecked
CustomerId	int	Unchecked
FUrl	nvarchar(255)	Checked
FSource	nvarchar(255)	Checked
FMedium	nvarchar(255)	Checked
FTerm	nvarchar(255)	Checked
FContent	nvarchar(255)	Checked
FName	nvarchar(255)	Checked
FDate	datetime	Checked
RUrl	nvarchar(255)	Checked
RSource	nvarchar(255)	Checked
RMedium	nvarchar(255)	Checked
RTerm	nvarchar(255)	Checked
RContent	nvarchar(255)	Checked
RName	nvarchar(255)	Checked
RDate	datetime	Checked
TimestampCounter	timestamp	Unchecked
		Unchecked
 
 */
