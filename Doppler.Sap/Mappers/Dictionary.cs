using System.Collections.Generic;

namespace Doppler.Sap.Mappers
{
    public static class Dictionary
    {
        public static readonly Dictionary<int, string> UserPlanTypesDictionary = new Dictionary<int, string>
        {
            {0, "CM"},
            {1, "CD"},
            {2, "CD"},
            {3, "CD"},
            {4, "CD"},
            {5, "CR"}
        };

        public static readonly Dictionary<int?, string> CurrencyDictionary = new Dictionary<int?, string>
        {
            {0, "USD"},
            {1, "ARS"},
            {2, "CLP"},
            {3, "MXN"}
        };

        public static readonly Dictionary<int?, string> PeriodicityDictionary = new Dictionary<int?, string>
        {
            {0, "Mensual"},
            {1, "Trimestral"},
            {2, "Semestral"},
            {3, "Anual"}
        };
    }
}
