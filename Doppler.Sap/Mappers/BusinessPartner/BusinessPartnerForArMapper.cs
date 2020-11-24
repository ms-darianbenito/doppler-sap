using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Doppler.Sap.Mappers.BusinessPartner
{
    public class BusinessPartnerForArMapper : IBusinessPartnerMapper
    {
        private const string countryCodeSupported = "AR";
        private readonly Dictionary<int, string> states = new Dictionary<int, string>
        {
            {2189,"01"}, // Buenos Aires
            {2190,"02"}, // Catamarca
            {2191,"16"}, // Chaco
            {2192,"17"}, // Chubut
            {2193,"00"}, // Ciudad Autónoma de Buenos Aires
            {2194,"04"}, // Corrientes
            {2195,"03"}, // Córdoba
            {2196,"05"}, // Entre Ríos
            {2197,"18"}, // Formosa
            {2198,"06"}, // Jujuy
            {2199,"21"}, // La Pampa
            {2200,"08"}, // La Rioja
            {2201,"07"}, // Mendoza
            {2202,"19"}, // Misiones
            {2203,"20"}, // Neuquén
            {2204,"22"}, // Río Negro
            {2205,"09"}, // Salta
            {2206,"10"}, // San Juan
            {2207,"11"}, // San Luis
            {2208,"12"}, // Santa Cruz
            {2209,"13"}, // Santa Fe
            {2210,"14"}, // Santiago del Estero
            {2211,"24"}, // Tierra del Fuego
            {2212,"15"}, // Tucumán
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
                PayTermsGrpCode = dopplerUser.PaymentMethod == (int)PaymentMethodEnum.MP ? (int)PayTermsGroupEnum.MP : (int)PayTermsGroupEnum.DEFAULT,
                ContactPerson = new MailAddress((dopplerUser.BillingEmails != null && dopplerUser.BillingEmails[0] != String.Empty) ?
                dopplerUser.BillingEmails[0].ToLower() :
                    dopplerUser.Email.ToLower()).User,
                EmailAddress = dopplerUser.Email.ToLower(),
                Phone1 = dopplerUser.PhoneNumber ?? "",
                FederalTaxID = dopplerUser.FederalTaxID.Replace("-", ""),
                U_B1SYS_VATCtg = dopplerUser.IdConsumerType.HasValue ?
                            (Dictionary.ConsumerTypesDictionary.TryGetValue(dopplerUser.IdConsumerType, out string consumerType) ? consumerType : "CF")
                            : "CF",
                Currency = fatherBusinessPartner?.Currency ?? "##",
                AliasName = dopplerUser.Email.ToLower(),
                U_B1SYS_FiscIdType = dopplerUser.FederalTaxType == "DNI" ? "96" : (dopplerUser.FederalTaxType == "CUIT" ? "80" : "99"),
                CardType = "C",
                U_DPL_CANCELED = dopplerUser.Cancelated ? "Y" : "N",
                U_DPL_SUSPENDED = dopplerUser.Blocked ? "Y" : "N",
                SalesPersonCode = (dopplerUser.IsInbound.HasValue ? (dopplerUser.IsInbound.GetValueOrDefault() ? 1 : 2) : 3),
                Indicator = "FC",
                DunningTerm = "ReclamoVto",
                FatherCard = fatherBusinessPartner?.CardCode,
                ContactEmployees = (dopplerUser.BillingEmails != null && dopplerUser.BillingEmails[0] != String.Empty) ?
                dopplerUser.BillingEmails
                    .Select(x => new SapContactEmployee
                    {
                        Name = new MailAddress(x.ToLower()).User,
                        E_Mail = x.ToLower(),
                        CardCode = cardCode,
                        Active = "tYES",
                        EmailGroupCode = "Facturacion"
                    })
                    .Append(new SapContactEmployee
                    {
                        Name = new MailAddress(dopplerUser.Email.ToLower()).User,
                        E_Mail = dopplerUser.Email.ToLower(),
                        CardCode = cardCode,
                        Active = "tYES",
                        EmailGroupCode = "Facturacion"
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
                                    EmailGroupCode = "Facturacion"
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
                                    (dopplerUser.BillingStateId.HasValue ? (states.TryGetValue(dopplerUser.BillingStateId.Value, out var sapBillStateId) ? sapBillStateId : "99") : "99"),
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
                                    (dopplerUser.BillingStateId.HasValue ? (states.TryGetValue(dopplerUser.BillingStateId.Value, out var sapShipStateId) ? sapShipStateId : "99") : "99"),
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