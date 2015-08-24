using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Infrastructure.Html
{
    public class EzClickInputModel {

        public string InputWrapperClass  { get; set; }
        public string ErrorImgClass { get; set; }
        public string Name { get; set; }
        public string Caption { get; set; }
        public bool IsRequired { get; set; }
        public bool StatusIcon { get; set; }

        public List<EzInputBuilder> InputButtons { get; set; }

		public EzClickInputModel() { }

        public EzClickInputModel(
            string name = "",
			string caption = "" , 
            string errorImgClass = "",
			string inputWrapperClass = "",
			bool   isRequired = false,
            bool   statusIcon = false,
            IEnumerable<EzInputBuilder> inputButtons = null
		        ) {
			Name =name  ;
			Caption = caption  ;
            ErrorImgClass = errorImgClass;
            InputWrapperClass =inputWrapperClass  ;
			IsRequired = isRequired;
			StatusIcon = statusIcon;
            if (inputButtons != null) {
                InputButtons = new List<EzInputBuilder>(inputButtons);
            } else {
                InputButtons = null;
            }
            
        }//constructor
    }//EzClickInputModel
}