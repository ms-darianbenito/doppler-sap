using Doppler.Sap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Doppler.Sap.Mappers.BusinessPartner
{
    public class BusinessPartnerForUsMapper : IBusinessPartnerMapper
    {
        private const string countryCodeSupported = "US";
        private readonly Dictionary<int, string> states = new Dictionary<int, string>
        {
            {4828,"AL"}, // Alabama
            {4829,"AK"},  // Alaska
            {4830,"AS"}, // American Samoa
            {4831,"AZ"}, // Arizona
            {4832,"AR"},  // Arkansas
            {4833,"CA"},  // California
            {4834,"CO"},  // Colorado
            {4835,"CT"},  // Connecticut
            {4836,"DE"}, // Delaware
            {4838,"FL"},  // Florida
            {4839,"GA"}, // Georgia
            {4840,"GU"},  // Guam
            {4841,"HI"},  // Hawaii
            {4842,"ID"}, // Idaho
            {4843,"IL"}, // Illinois
            {4844,"IN"}, // Indiana
            {4845,"IA"},  // Iowa
            {4846,"KS"}, // Kansas
            {4847,"KY"}, // Kentucky
            {4848,"LA"}, // Louisiana
            {4849,"ME"}, // Maine
            {4850,"MD"}, // Maryland
            {4851,"MA"}, // Massachusetts
            {4852,"MI"}, // Michigan
            {4853,"AL"}, // Minnesota
            {4854,"MN"},  // Mississippi
            {4855,"MO"}, // Missouri
            {4856,"MT"}, // Montana
            {4857,"NE"},  // Nebraska
            {4858,"NV"},  // Nevada
            {4859,"NH"},  // New Hampshire
            {4860,"NJ"},  // New Jersey
            {4861,"NM"}, // New Mexico
            {4862,"NY"},  // New York
            {4863,"NC"}, // North Carolina
            {4864,"ND"},  // North Dakota
            {4865,"MP"},  // Northern Mariana Islands
            {4866,"OH"}, // Ohio
            {4867,"OK"}, // Oklahoma
            {4868,"OR"}, // Oregon
            {4869,"PA"},  // Pennsylvania
            {4870,"PR"}, // Puerto Rico
            {4871,"RI"}, // Rhode Island
            {4872,"SC"}, // South Carolina
            {4873,"SD"}, // South Dakota
            {4874,"TN"}, // Tennessee
            {4875,"TX"}, // Texas
            {4876,"--"}, // United States Minor Outlying Islands
            {4877,"UT"},  // Utah
            {4878,"VT"}, // Vermont
            {4879,"VI"}, // Virgin Islands, U.S.
            {4880,"VA"},  // Virginia
            {4881,"WA"},  // Washington
            {4882,"WV"},  // West Virginia
            {4883,"WI"},  // Wisconsin
            {4884,"WY"},  // Wyoming
        };

        public bool CanMapCountry(string countryCode)
        {
            return countryCodeSupported == countryCode;
        }

        public string MapDopplerUserIdToSapBusinessPartnerId(int id, int planType)
        {
            var planTypeCode = Dictionary.UserPlanTypesDictionary.TryGetValue(planType, out var code) ? code
                : throw new ArgumentException("Parameter does not match with any plan type.", $"userPlanTypeId");

            return $"{planTypeCode}{id:00000000000}.";
        }

        public SapBusinessPartner MapDopplerUserToSapBusinessPartner(DopplerUserDto dopplerUser, string cardCode, SapBusinessPartner fatherBusinessPartner)
        {
            var newBusinessPartner = new SapBusinessPartner
            {
                CardCode = cardCode,
                CardName = $"{dopplerUser.FirstName} {dopplerUser.LastName}".ToUpper(),
                GroupCode = dopplerUser.GroupCode,
                PayTermsGrpCode = -1,
                ContactPerson = new MailAddress((dopplerUser.BillingEmails != null && dopplerUser.BillingEmails[0] != String.Empty) ?
                dopplerUser.BillingEmails[0].ToLower() :
                    dopplerUser.Email.ToLower()).User,
                EmailAddress = dopplerUser.Email.ToLower(),
                Phone1 = dopplerUser.PhoneNumber ?? "",
                FederalTaxID = dopplerUser.FederalTaxID,
                Currency = "$",
                AliasName = dopplerUser.Email.ToLower(),
                CardType = "C",
                U_DPL_CANCELED = dopplerUser.Cancelated ? "Y" : "N",
                U_DPL_SUSPENDED = dopplerUser.Blocked ? "Y" : "N",
                SalesPersonCode = (dopplerUser.IsInbound.HasValue ? (dopplerUser.IsInbound.GetValueOrDefault() ? 1 : 2) : 3),
                FatherCard = fatherBusinessPartner?.CardCode,
                ContactEmployees = (dopplerUser.BillingEmails != null && dopplerUser.BillingEmails[0] != String.Empty) ?
                dopplerUser.BillingEmails
                    .Select(x => new SapContactEmployee
                    {
                        Name = new MailAddress(x.ToLower()).User,
                        E_Mail = x.ToLower(),
                        CardCode = cardCode,
                        Active = "tYES",
                        EmailGroupCode = "Billing"
                    })
                    .Append(new SapContactEmployee
                    {
                        Name = new MailAddress(dopplerUser.Email.ToLower()).User,
                        E_Mail = dopplerUser.Email.ToLower(),
                        CardCode = cardCode,
                        Active = "tYES",
                        EmailGroupCode = "Billing"
                    })
                    .GroupBy(y => y.E_Mail)
                    .Select(z => z.First())
                    .ToList()
                    : new List<SapContactEmployee>
                        {
                            new SapContactEmployee
                                {
                                    Name = new MailAddress(dopplerUser.Email.ToLower()).User,
                                    E_Mail = dopplerUser.Email.ToLower(),
                                        CardCode = cardCode,
                                    Active = "tYES",
                                    EmailGroupCode = "Billing"
                                }
                            },
                BPAddresses = new List<Address>
                {
                    new Address
                        {
                            AddressName = "Bill to",
                            Street = dopplerUser.BillingAddress != null ? dopplerUser.BillingAddress.ToUpper() : "",
                            ZipCode = dopplerUser.BillingZip != null ? dopplerUser.BillingZip.ToUpper() : "",
                            City = dopplerUser.BillingCity != null ? dopplerUser.BillingCity.ToUpper() : "",
                            Country = dopplerUser.BillingCountryCode != null ? dopplerUser.BillingCountryCode.ToUpper() : "",
                            State = dopplerUser.GroupCode == 115 ? dopplerUser.BillingStateId.ToString() :
                                    (states.TryGetValue(dopplerUser.BillingStateId.Value, out var sapBillStateId) ? sapBillStateId : "99"),
                            AddressType = "bo_BillTo",
                                BPCode =  cardCode,
                            RowNum = 0
                        },
                    new Address
                        {
                            AddressName = "Ship to",
                            Street = dopplerUser.BillingAddress != null ? dopplerUser.BillingAddress.ToUpper() : "",
                            ZipCode = dopplerUser.BillingZip != null ? dopplerUser.BillingZip.ToUpper() : "",
                            City = dopplerUser.BillingCity != null ? dopplerUser.BillingCity.ToUpper() : "",
                            Country = dopplerUser.BillingCountryCode != null ? dopplerUser.BillingCountryCode.ToUpper() : "",
                            State = dopplerUser.GroupCode == 115 ? dopplerUser.BillingStateId.ToString() :
                                    (states.TryGetValue(dopplerUser.BillingStateId.Value, out var sapShipStateId) ? sapShipStateId : "99"),
                            AddressType = "bo_ShipTo",
                                BPCode =  cardCode,
                            RowNum = 1
                        }
                    }
            };

            if (dopplerUser.SAPProperties != null)
            {
                newBusinessPartner.Properties1 = dopplerUser.SAPProperties.ContractCurrency ? "tYES" : "tNO";
                newBusinessPartner.Properties2 = dopplerUser.SAPProperties.GovernmentAccount ? "tYES" : "tNO";
                newBusinessPartner.Properties3 = dopplerUser.SAPProperties.Premium ? "tYES" : "tNO";
                newBusinessPartner.Properties4 = dopplerUser.SAPProperties.Plus ? "tYES" : "tNO";
                newBusinessPartner.Properties5 = dopplerUser.SAPProperties.ComercialPartner ? "tYES" : "tNO";
                newBusinessPartner.Properties6 = dopplerUser.SAPProperties.MarketingPartner ? "tYES" : "tNO";
                newBusinessPartner.Properties7 = dopplerUser.SAPProperties.OnBoarding ? "tYES" : "tNO";
                newBusinessPartner.Properties8 = dopplerUser.SAPProperties.Layout ? "tYES" : "tNO";
                newBusinessPartner.Properties9 = dopplerUser.SAPProperties.Datahub ? "tYES" : "tNO";
                newBusinessPartner.Properties10 = dopplerUser.SAPProperties.PushNotification ? "tYES" : "tNO";
                newBusinessPartner.Properties11 = dopplerUser.SAPProperties.ExclusiveIp ? "tYES" : "tNO";
                newBusinessPartner.Properties12 = dopplerUser.SAPProperties.Advisory ? "tYES" : "tNO";
                newBusinessPartner.Properties13 = dopplerUser.SAPProperties.Reports ? "tYES" : "tNO";
                newBusinessPartner.Properties14 = dopplerUser.SAPProperties.SMS ? "tYES" : "tNO";
            }

            return newBusinessPartner;
        }
    }
}
