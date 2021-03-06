﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ezbob.CreditSafeLib
{
    //------------------------------------------------------------------------------
    // <auto-generated>
    //     This code was generated by a tool.
    //     Runtime Version:4.0.30319.34209
    //
    //     Changes to this file may cause incorrect behavior and will be lost if
    //     the code is regenerated.
    // </auto-generated>
    //------------------------------------------------------------------------------
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    // 
    // This source code was auto-generated by xsd, Version=4.0.30319.33440.
    // 


    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public partial class rop
    {

        private string casenrField;

        private string ccjdateField;

        private string ccjdatepaidField;

        private string courtField;

        private string ccjstatusField;

        private string ccjamountField;

        private string againstField;

        private string addressField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string casenr
        {
            get
            {
                return this.casenrField;
            }
            set
            {
                this.casenrField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ccjdate
        {
            get
            {
                return this.ccjdateField;
            }
            set
            {
                this.ccjdateField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ccjdatepaid
        {
            get
            {
                return this.ccjdatepaidField;
            }
            set
            {
                this.ccjdatepaidField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string court
        {
            get
            {
                return this.courtField;
            }
            set
            {
                this.courtField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ccjstatus
        {
            get
            {
                return this.ccjstatusField;
            }
            set
            {
                this.ccjstatusField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ccjamount
        {
            get
            {
                return this.ccjamountField;
            }
            set
            {
                this.ccjamountField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string against
        {
            get
            {
                return this.againstField;
            }
            set
            {
                this.againstField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CreditSafeNonLtdResponse
    {

        private object[] itemsField;

        /// <remarks/>
        [XmlElement("body", typeof(xmlresponsecompanyBody), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("header", typeof(xmlresponsecompanyHeader), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("rop", typeof(rop))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBody
    {

        private string reportidField;

        private string reportnameField;

        private xmlresponsecompanyBodyCompaniesCompany[] companiesField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string reportid
        {
            get
            {
                return this.reportidField;
            }
            set
            {
                this.reportidField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string reportname
        {
            get
            {
                return this.reportnameField;
            }
            set
            {
                this.reportnameField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("company", typeof(xmlresponsecompanyBodyCompaniesCompany), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompany[] companies
        {
            get
            {
                return this.companiesField;
            }
            set
            {
                this.companiesField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompany
    {

        private xmlresponsecompanyBodyCompaniesCompanyBaseinformation[] baseinformationField;

        private xmlresponsecompanyBodyCompaniesCompanyRatingsRatingdetail[] ratingsField;

        private xmlresponsecompanyBodyCompaniesCompanyLimitsLimitdetail[] limitsField;

        private xmlresponsecompanyBodyCompaniesCompanyMatchedccjsummary[] matchedccjsummaryField;

        private rop[] matchedrecordofpaymentsField;

        private xmlresponsecompanyBodyCompaniesCompanyPossiblematchedccjsummary[] possiblematchedccjsummaryField;

        private rop[] possiblematchedrecordofpaymentsField;

        private xmlresponsecompanyBodyCompaniesCompanySeniorexecutive[] seniorexecutiveField;

        private xmlresponsecompanyBodyCompaniesCompanyEventsEventdetail[] eventsField;

        /// <remarks/>
        [XmlElement("baseinformation", Form = XmlSchemaForm.Unqualified)]
        public xmlresponsecompanyBodyCompaniesCompanyBaseinformation[] baseinformation
        {
            get
            {
                return this.baseinformationField;
            }
            set
            {
                this.baseinformationField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("ratingdetail", typeof(xmlresponsecompanyBodyCompaniesCompanyRatingsRatingdetail), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompanyRatingsRatingdetail[] ratings
        {
            get
            {
                return this.ratingsField;
            }
            set
            {
                this.ratingsField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("limitdetail", typeof(xmlresponsecompanyBodyCompaniesCompanyLimitsLimitdetail), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompanyLimitsLimitdetail[] limits
        {
            get
            {
                return this.limitsField;
            }
            set
            {
                this.limitsField = value;
            }
        }

        /// <remarks/>
        [XmlElement("matchedccjsummary", Form = XmlSchemaForm.Unqualified)]
        public xmlresponsecompanyBodyCompaniesCompanyMatchedccjsummary[] matchedccjsummary
        {
            get
            {
                return this.matchedccjsummaryField;
            }
            set
            {
                this.matchedccjsummaryField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("rop", typeof(rop), IsNullable = false)]
        public rop[] matchedrecordofpayments
        {
            get
            {
                return this.matchedrecordofpaymentsField;
            }
            set
            {
                this.matchedrecordofpaymentsField = value;
            }
        }

        /// <remarks/>
        [XmlElement("possiblematchedccjsummary", Form = XmlSchemaForm.Unqualified)]
        public xmlresponsecompanyBodyCompaniesCompanyPossiblematchedccjsummary[] possiblematchedccjsummary
        {
            get
            {
                return this.possiblematchedccjsummaryField;
            }
            set
            {
                this.possiblematchedccjsummaryField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("rop", typeof(rop), IsNullable = false)]
        public rop[] possiblematchedrecordofpayments
        {
            get
            {
                return this.possiblematchedrecordofpaymentsField;
            }
            set
            {
                this.possiblematchedrecordofpaymentsField = value;
            }
        }

        /// <remarks/>
        [XmlElement("seniorexecutive", Form = XmlSchemaForm.Unqualified)]
        public xmlresponsecompanyBodyCompaniesCompanySeniorexecutive[] seniorexecutive
        {
            get
            {
                return this.seniorexecutiveField;
            }
            set
            {
                this.seniorexecutiveField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("eventdetail", typeof(xmlresponsecompanyBodyCompaniesCompanyEventsEventdetail), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompanyEventsEventdetail[] events
        {
            get
            {
                return this.eventsField;
            }
            set
            {
                this.eventsField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyBaseinformation
    {

        private string numberField;

        private string nameField;

        private string address1Field;

        private string address2Field;

        private string address3Field;

        private string address4Field;

        private string postcodeField;

        private string mpsregisteredField;

        private string addressdateField;

        private string addressreasonField;

        private string premisestypeField;

        private string activitiesField;

        private string employeesField;

        private string websiteField;

        private string emailField;

        private string safenumberField;

        private xmlresponsecompanyBodyCompaniesCompanyBaseinformationTelephonenumbersTelephonenumber[] telephonenumbersField;

        private xmlresponsecompanyBodyCompaniesCompanyBaseinformationFaxnumbersFaxnumber[] faxnumbersField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string address1
        {
            get
            {
                return this.address1Field;
            }
            set
            {
                this.address1Field = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string address2
        {
            get
            {
                return this.address2Field;
            }
            set
            {
                this.address2Field = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string address3
        {
            get
            {
                return this.address3Field;
            }
            set
            {
                this.address3Field = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string address4
        {
            get
            {
                return this.address4Field;
            }
            set
            {
                this.address4Field = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string postcode
        {
            get
            {
                return this.postcodeField;
            }
            set
            {
                this.postcodeField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string mpsregistered
        {
            get
            {
                return this.mpsregisteredField;
            }
            set
            {
                this.mpsregisteredField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string addressdate
        {
            get
            {
                return this.addressdateField;
            }
            set
            {
                this.addressdateField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string addressreason
        {
            get
            {
                return this.addressreasonField;
            }
            set
            {
                this.addressreasonField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string premisestype
        {
            get
            {
                return this.premisestypeField;
            }
            set
            {
                this.premisestypeField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string activities
        {
            get
            {
                return this.activitiesField;
            }
            set
            {
                this.activitiesField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string employees
        {
            get
            {
                return this.employeesField;
            }
            set
            {
                this.employeesField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string website
        {
            get
            {
                return this.websiteField;
            }
            set
            {
                this.websiteField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string safenumber
        {
            get
            {
                return this.safenumberField;
            }
            set
            {
                this.safenumberField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("telephonenumber", typeof(xmlresponsecompanyBodyCompaniesCompanyBaseinformationTelephonenumbersTelephonenumber), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompanyBaseinformationTelephonenumbersTelephonenumber[] telephonenumbers
        {
            get
            {
                return this.telephonenumbersField;
            }
            set
            {
                this.telephonenumbersField = value;
            }
        }

        /// <remarks/>
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("faxnumber", typeof(xmlresponsecompanyBodyCompaniesCompanyBaseinformationFaxnumbersFaxnumber), Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public xmlresponsecompanyBodyCompaniesCompanyBaseinformationFaxnumbersFaxnumber[] faxnumbers
        {
            get
            {
                return this.faxnumbersField;
            }
            set
            {
                this.faxnumbersField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyBaseinformationTelephonenumbersTelephonenumber
    {

        private string telephoneField;

        private string tpsregisteredField;

        private string mainField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string telephone
        {
            get
            {
                return this.telephoneField;
            }
            set
            {
                this.telephoneField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string tpsregistered
        {
            get
            {
                return this.tpsregisteredField;
            }
            set
            {
                this.tpsregisteredField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string main
        {
            get
            {
                return this.mainField;
            }
            set
            {
                this.mainField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyBaseinformationFaxnumbersFaxnumber
    {

        private string faxField;

        private string fpsregisteredField;

        private string mainField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string fax
        {
            get
            {
                return this.faxField;
            }
            set
            {
                this.faxField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string fpsregistered
        {
            get
            {
                return this.fpsregisteredField;
            }
            set
            {
                this.fpsregisteredField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string main
        {
            get
            {
                return this.mainField;
            }
            set
            {
                this.mainField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyRatingsRatingdetail
    {

        private string dateField;

        private string scoreField;

        private string descriptionField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyLimitsLimitdetail
    {

        private string limitField;

        private string dateField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string limit
        {
            get
            {
                return this.limitField;
            }
            set
            {
                this.limitField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyMatchedccjsummary
    {

        private string valueField;

        private string numberField;

        private string datefromField;

        private string datetoField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string datefrom
        {
            get
            {
                return this.datefromField;
            }
            set
            {
                this.datefromField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string dateto
        {
            get
            {
                return this.datetoField;
            }
            set
            {
                this.datetoField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyPossiblematchedccjsummary
    {

        private string valueField;

        private string numberField;

        private string datefromField;

        private string datetoField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string datefrom
        {
            get
            {
                return this.datefromField;
            }
            set
            {
                this.datefromField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string dateto
        {
            get
            {
                return this.datetoField;
            }
            set
            {
                this.datetoField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanySeniorexecutive
    {

        private string nameField;

        private string positionField;

        private string emailField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string email
        {
            get
            {
                return this.emailField;
            }
            set
            {
                this.emailField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyBodyCompaniesCompanyEventsEventdetail
    {

        private string dateField;

        private string textField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string date
        {
            get
            {
                return this.dateField;
            }
            set
            {
                this.dateField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyHeader
    {

        private xmlresponsecompanyHeaderReportinformation[] reportinformationField;

        /// <remarks/>
        [XmlElement("reportinformation", Form = XmlSchemaForm.Unqualified)]
        public xmlresponsecompanyHeaderReportinformation[] reportinformation
        {
            get
            {
                return this.reportinformationField;
            }
            set
            {
                this.reportinformationField = value;
            }
        }
    }

    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.33440")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public partial class xmlresponsecompanyHeaderReportinformation
    {

        private string timeField;

        private string reporttypeField;

        private string countryField;

        private string versionField;

        private string providerField;

        private string chargereferenceField;

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string reporttype
        {
            get
            {
                return this.reporttypeField;
            }
            set
            {
                this.reporttypeField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string country
        {
            get
            {
                return this.countryField;
            }
            set
            {
                this.countryField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }

        /// <remarks/>
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string chargereference
        {
            get
            {
                return this.chargereferenceField;
            }
            set
            {
                this.chargereferenceField = value;
            }
        }
    }
}
