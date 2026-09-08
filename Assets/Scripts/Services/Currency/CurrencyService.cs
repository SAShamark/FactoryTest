using System;
using System.Collections.Generic;
using Services.Storage;

namespace Services.Currency
{
    public class CurrencyService : IDisposable
    {
        private readonly StorageService _storageService;
        private readonly Dictionary<string, int> _savedCurrencies;
        private readonly Dictionary<CurrencyType, IBank> _currencyBanks = new();
        private readonly Dictionary<CurrencyType, Action<int>> _saveHandlers = new();

        public event Action<CurrencyType, int> OnNotEnough;
        public CurrencyCollection CurrencyCollection { get; }

        public CurrencyService(StorageService storageService, CurrencyCollection currencyCollection)
        {
            _storageService = storageService;
            CurrencyCollection = currencyCollection;
            _savedCurrencies = _storageService.LoadData(
                    StorageConstants.CURRENCIES,
                    new Dictionary<string, int>())
                ?? new Dictionary<string, int>();
            AddAllCurrencyBanks();
        }

        private void AddAllCurrencyBanks()
        {
            foreach (CurrencyType currencyType in Enum.GetValues(typeof(CurrencyType)))
            {
                int initialCurrency = _savedCurrencies.GetValueOrDefault(currencyType.ToString(), 0);
                AddCurrencyBank(currencyType, initialCurrency);
            }
        }

        private void AddCurrencyBank(CurrencyType currencyType, int initialCurrency)
        {
            var bank = new CurrencyBank(currencyType, initialCurrency);
            Action<int> saveHandler = value => SaveCurrency(currencyType, value);

            bank.OnCurrencyChanged += saveHandler;
            bank.OnNotEnough += NotEnoughCurrency;
            _currencyBanks[currencyType] = bank;
            _saveHandlers[currencyType] = saveHandler;
        }

        private void NotEnoughCurrency(CurrencyType type, int value) => OnNotEnough?.Invoke(type, value);


        private void SaveCurrency(CurrencyType currencyType, int value)
        {
            _savedCurrencies[currencyType.ToString()] = value;
            _storageService.SaveData(StorageConstants.CURRENCIES, _savedCurrencies);
        }

        public IBank GetCurrencyByType(CurrencyType currencyType)
        {
            if (_currencyBanks.TryGetValue(currencyType, out IBank bank))
            {
                return bank;
            }

            throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, null);
        }

        public void Dispose()
        {
            foreach (KeyValuePair<CurrencyType, IBank> pair in _currencyBanks)
            {
                CurrencyType currencyType = pair.Key;
                IBank bank = pair.Value;
                bank.OnNotEnough -= NotEnoughCurrency;

                if (_saveHandlers.TryGetValue(currencyType, out Action<int> saveHandler))
                    bank.OnCurrencyChanged -= saveHandler;
            }

            _saveHandlers.Clear();
            _currencyBanks.Clear();
        }
    }
}
