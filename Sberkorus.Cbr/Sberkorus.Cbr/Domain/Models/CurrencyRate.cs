using System;

namespace Sberkorus.Cbr.Domain.Models
{
    public class CurrencyRate
    {
        public string Name { get; set; }
        public decimal Nominal { get; set; }
        public decimal Rate { get; set; }
        public int Code { get; set; }
        public string CharCode { get; set; }
        public double UnitRate { get; set; }
        public DateTime Date { get; set; }
    }
}