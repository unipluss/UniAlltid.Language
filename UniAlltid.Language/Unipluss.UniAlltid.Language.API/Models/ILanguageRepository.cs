using System.Collections.Generic;
using System.Net.Http;

namespace UniAlltid.Language.API.Models
{
    public interface ILanguageRepository
    {
        IEnumerable<Translation> Retrieve(string customer, string language);
        Dictionary<string, string> RetrieveDictionary(string language, string customer);
        void Create(NewTranslation translation);
        void CreateOrUpdateSingle(NewSingleTranslation translation);
        void Update(Translation translation, string selectedCustomer);
        void UpdateKey(int id, string keyId);
        void Delete(int id);
        IEnumerable<Customer> RetrieveCustomers();
        void CreateCustomer(Customer customer);
        HttpResponseMessage ExportCSV();
        void UpdateCustomerKeys(IEnumerable<ExternalTranslation> translations, string customer, string updatedBy);
        IEnumerable<ExternalTranslation> RetrieveExternalTranslations(IEnumerable<string> keyIds, string customer = "");
        IEnumerable<Log> GetLogs();
    }
}