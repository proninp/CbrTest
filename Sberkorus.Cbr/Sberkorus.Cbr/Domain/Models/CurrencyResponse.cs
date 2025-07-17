using System;
using System.Collections.Generic;

namespace Sberkorus.Cbr.Domain.Models
{
    /// <summary>
    /// Ответ с курсами валют на определенную дату
    /// </summary>
    public class CurrencyResponse
    {
        /// <summary>
        /// Дата, на которую установлены курсы
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Список курсов валют
        /// </summary>
        public List<CurrencyRate> CurrencyRates { get; set; } = new List<CurrencyRate>();
    }
}