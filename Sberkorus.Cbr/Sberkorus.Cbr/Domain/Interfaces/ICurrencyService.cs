using System;
using System.Threading.Tasks;
using Sberkorus.Cbr.Domain.Models;

namespace Sberkorus.Cbr.Domain.Interfaces
{
    public interface ICurrencyService
    {
        Task<CurrencyResponse> GetCurrencyRatesAsync(DateTime date, int? currencyCode = null);
    }
}