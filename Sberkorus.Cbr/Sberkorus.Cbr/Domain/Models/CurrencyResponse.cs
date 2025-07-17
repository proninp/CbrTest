using System;
using System.Collections.Generic;

namespace Sberkorus.Cbr.Domain.Models
{
    public class CurrencyResponse
    {
        public DateTime Date { get; set; }
        public List<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();
    }
}