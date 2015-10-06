using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniAlltid.Language.API.Models
{
    public class Translation
    {
        public int Id { get; set; }
        public string KeyId { get; set; }
        public string Lang { get; set; }
        public string Value { get; set; }
        public string Customer { get; set; }
        public string DefaultValue { get; set; }
    }

    public class NewTranslation : NewSingleTranslation
    {
        public string ValueEnglish { get; set; }
    }

    public class NewSingleTranslation
    {
        public string KeyId { get; set; }
        public string Value { get; set; }
        public string Language { get; set; }
    }

    public class CsvTranslation
    {
        public string KeyId { get; set; }
        public string Norwegian { get; set; }
        public string English { get; set; }
        public string Customer { get; set; }
    }
}