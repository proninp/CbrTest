using System;

namespace Sberkorus.Cbr.Domain.Models
{
    /// <summary>
    /// Курс валюты на определенную дату
    /// </summary>
    public class CurrencyRate
    {
        /// <summary>
        /// Название валюты
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Номинал валюты
        /// </summary>
        public decimal Nominal { get; set; }
        /// <summary>
        /// Курс валюты в рублях
        /// </summary>
        public decimal Rate { get; set; }
        /// <summary>
        /// Цифровой код валюты (ISO 4217)
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// Символьный код валюты (ISO 4217)
        /// </summary>
        public string CharCode { get; set; }
        /// <summary>
        /// Курс за единицу валюты
        /// </summary>
        public double UnitRate { get; set; }
        /// <summary>
        /// Дата курса
        /// </summary>
        public DateTime Date { get; set; }
    }
}