namespace EzBobModels.Customer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobModels.Enums;

    public partial class Customer
    {
        /// <summary>
        /// Represents personal information of customer model
        /// </summary>
        public class CustomerPersonalInfo
        {
            private readonly Customer customer;
            private string fullName;
            private Gender? gender;
            private MaritalStatus? maritalStatus;
            private TypeOfBusiness? typeOfBusiness;
            private IndustryType? industryType;

            /// <summary>
            /// Initializes a new instance of the <see cref="CustomerPersonalInfo"/> class.
            /// </summary>
            /// <param name="customer">The customer.</param>
            public CustomerPersonalInfo(Customer customer)
            {
                this.customer = customer;
            }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName
            {
                get { return this.customer.FirstName; }
                set { this.customer.FirstName = value; }
            }

            /// <summary>
            /// Gets or sets the middle initial.
            /// </summary>
            /// <value>
            /// The middle initial.
            /// </value>
            public string MiddleInitial
            {
                get { return this.customer.MiddleInitial; }
                set { this.customer.MiddleInitial = value; }
            }

            /// <summary>
            /// Gets or sets the surname.
            /// </summary>
            /// <value>
            /// The surname.
            /// </value>
            public string Surname
            {
                get { return this.customer.Surname; }
                set { this.customer.Surname = value; }
            }

            /// <summary>
            /// Gets the full name.
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string Fullname //TODO: should be in view model
            {
                get
                {
                    if (this.fullName == null)
                    {
                        const string Space = " ";

                        var namesList = new List<string>(3) {
                            (FirstName ?? string.Empty).Trim(),
                            (MiddleInitial ?? string.Empty).Trim(),
                            (Surname ?? string.Empty).Trim(),
                        };

                        this.fullName = string.Join(Space, namesList.Where(x => !string.IsNullOrWhiteSpace(x)));
                    }

                    return this.fullName;
                }

            }

            /// <summary>
            /// Gets or sets the date of birth.
            /// </summary>
            /// <value>
            /// The date of birth.
            /// </value>
            public DateTime? DateOfBirth
            {
                get { return this.customer.DateOfBirth; }
                set { this.customer.DateOfBirth = value; }
            }

            /// <summary>
            /// Gets the age.
            /// </summary>
            /// <value>
            /// The age.
            /// </value>
            public int Age
            {
                get { return DateOfBirth == null ? 0 : (int)(DateTime.Today - DateOfBirth.Value).TotalDays / 365; }
            }

            /// <summary>
            /// Gets or sets the time at address.
            /// </summary>
            /// <value>
            /// The time at address.
            /// </value>
            public int? TimeAtAddress
            {
                get { return this.customer.TimeAtAddress; }
                set { this.customer.TimeAtAddress = value; }
            }

            /// <summary>
            /// Gets or sets the property status.
            /// </summary>
            /// <value>
            /// The property status.
            /// </value>
            public int PropertyStatus
            {
                get { return this.customer.PropertyStatusId; }
                set { this.customer.PropertyStatusId = value; }
            }

            /// <summary>
            /// Gets or sets the gender.
            /// </summary>
            /// <value>
            /// The gender.
            /// </value>
            public Gender? Gender
            {
                get
                {
                    if (this.gender.HasValue)
                    {
                        return this.gender;
                    }

                    if (this.customer.Gender.HasValue)
                    {
                        Gender g;
                        if (Enum.TryParse(this.customer.Gender.ToString(), out g))
                        {
                            this.gender = g;
                        }
                    }

                    return this.gender;
                }
                set
                {
                    this.gender = value;
                    this.customer.Gender = value.HasValue ? (char?)value.ToString()[0] : null;
                }
            }

            /// <summary>
            /// Gets the name of the gender.
            /// </summary>
            /// <value>
            /// The name of the gender.
            /// </value>
            public string GenderName
            {
                get { return Gender.ToString(); }
            } //TODO: should be in view model

            /// <summary>
            /// Gets or sets the marital status.
            /// </summary>
            /// <value>
            /// The marital status.
            /// </value>
            public MaritalStatus? MaritalStatus
            {
                get
                {
                    if (this.maritalStatus.HasValue)
                    {
                        return this.maritalStatus;
                    }

                    if (this.customer.MaritalStatus != null)
                    {
                        MaritalStatus ms;
                        if (Enum.TryParse(this.customer.MaritalStatus, out ms))
                        {
                            this.maritalStatus = ms;
                        }
                    }

                    return this.maritalStatus;
                }
                set
                {
                    this.maritalStatus = value;
                    this.customer.MaritalStatus = value.HasValue ? value.ToString() : null;
                }
            }

            /// <summary>
            /// Gets or sets the type of business.
            /// </summary>
            /// <value>
            /// The type of business.
            /// </value>
            public TypeOfBusiness? TypeOfBusiness
            {
                get
                {
                    if (this.typeOfBusiness.HasValue)
                    {
                        return this.typeOfBusiness;
                    }

                    if (this.customer.TypeOfBusiness != null)
                    {
                        TypeOfBusiness t;
                        if (Enum.TryParse(this.customer.TypeOfBusiness, out t))
                        {
                            this.typeOfBusiness = t;
                        }
                    }

                    return this.typeOfBusiness;
                }
                set
                {
                    this.typeOfBusiness = value;
                    this.customer.TypeOfBusiness = value.HasValue ? value.ToString() : null;
                }
            }

            /// <summary>
            /// Gets the name of the type of business.
            /// </summary>
            /// <value>
            /// The name of the type of business.
            /// </value>
            public string TypeOfBusinessName
            {
                get { return TypeOfBusiness.ToString(); }
            } //TODO: should be in view model

            /// <summary>
            /// Gets the type of business description.
            /// </summary>
            /// <value>
            /// The type of business description.
            /// </value>
            public string TypeOfBusinessDescription
            {
                get { return TypeOfBusiness.GetDescriptionAttribute(); }
            } //TODO: should be in view model

            /// <summary>
            /// Gets the type of the industry.
            /// </summary>
            /// <value>
            /// The type of the industry.
            /// </value>
            public IndustryType? IndustryType
            {
                get
                {
                    if (this.industryType.HasValue)
                    {
                        return this.industryType;
                    }

                    if (this.customer.IndustryType != null)
                    {
                        IndustryType industry;
                        if (Enum.TryParse(this.customer.IndustryType, out industry))
                        {
                            this.industryType = industry;
                        }
                    }

                    return this.industryType;
                }
                set
                {
                    this.industryType = value;
                    this.customer.IndustryType = value.HasValue ? value.ToString() : null;
                }
            }

            /// <summary>
            /// Gets the industry type description.
            /// </summary>
            /// <value>
            /// The industry type description.
            /// </value>
            public string IndustryTypeDescription
            {
                get { return IndustryType == null ? null : IndustryType.ToString(); }
            } //TODO: should be in view model

            /// <summary>
            /// Gets or sets the daytime phone.
            /// </summary>
            /// <value>
            /// The daytime phone.
            /// </value>
            public string DaytimePhone
            {
                get { return this.customer.DaytimePhone; }
                set { this.customer.DaytimePhone = value; }
            }

            /// <summary>
            /// Gets or sets the mobile phone.
            /// </summary>
            /// <value>
            /// The mobile phone.
            /// </value>
            public string MobilePhone
            {
                get { return this.customer.MobilePhone; }
                set { this.customer.MobilePhone = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether [mobile phone verified].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [mobile phone verified]; otherwise, <c>false</c>.
            /// </value>
            public bool MobilePhoneVerified
            {
                get { return this.customer.MobilePhoneVerified; }
                set { this.customer.MobilePhoneVerified = value; }
            }

            /// <summary>
            /// Gets or sets the overall turn over.
            /// </summary>
            /// <value>
            /// The overall turn over.
            /// </value>
            public decimal? OverallTurnOver
            {
                get { return this.customer.OverallTurnOver; }
                set { this.customer.OverallTurnOver = value; }
            }

            /// <summary>
            /// Gets or sets the web site turn over.
            /// </summary>
            /// <value>
            /// The web site turn over.
            /// </value>
            public decimal? WebSiteTurnOver
            {
                get { return this.customer.WebSiteTurnOver; }
                set { this.customer.WebSiteTurnOver = value; }
            }

            /// <summary>
            /// Gets or sets a value indicating whether [consent to search].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [consent to search]; otherwise, <c>false</c>.
            /// </value>
            public bool? ConsentToSearch
            {
                get { return this.customer.ConsentToSearch; }
                set { this.customer.ConsentToSearch = value; }
            }
        }
    }
}
