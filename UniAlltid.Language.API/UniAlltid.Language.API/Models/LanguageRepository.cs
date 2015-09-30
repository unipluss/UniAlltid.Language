﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Dapper;
using Newtonsoft.Json;

namespace UniAlltid.Language.API.Models
{
    public class LanguageRepository
    {
        private readonly IDbConnection _connection;

        public LanguageRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        internal IEnumerable<Translation> Retrieve(string customer, string language)
        {
            if (String.IsNullOrEmpty(customer) && String.IsNullOrEmpty(language))
                return GetDefaultValues();

            else
                return GetFilteredValues(customer, language);
        }

        private IEnumerable<Translation> GetDefaultValues()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_language where isNull(customer, '') = ''");

            return _connection.Query<Translation>(sql.ToString()).ToList();
        }

         private IEnumerable<Translation> GetFilteredValues(string customer, string language)
        {
            StringBuilder sql = new StringBuilder();

            if (String.IsNullOrEmpty(customer))
            {
                // Get by language   
                sql.AppendLine("select * from");
                sql.AppendLine("(Select * from t_language where lang = @language and isNull(customer, '') = ''");
                sql.AppendLine("union select * from t_language where lang = @language");
                sql.AppendLine("and keyid not in (select keyid from t_language where lang = @language)) inni");
            }

            else if (String.IsNullOrEmpty(language))
            {
                // Get by customer
                sql.AppendLine("select l0.*, l2.value as DefaultValue from t_language l0");
                sql.AppendLine("left join t_language l2  on l0.keyid = l2.keyid and l0.lang = l2.lang  and not isNull(l0.customer, '') = '' and isNull(l2.customer, '') = ''");
                sql.AppendLine("where not l0.id in (");
                sql.AppendLine("select l.id from t_language l");
                sql.AppendLine("left join t_language l1 on l.keyid = l1.keyid and l.lang = l1.lang and l1.customer = @customer");
                sql.AppendLine("where not l.id = l1.id )");
            }

            else
            {
                // Get by both
                sql.AppendLine("select l0.*, l2.value as DefaultValue from t_language l0");
                sql.AppendLine("left join t_language l2  on l0.keyid = l2.keyid and l0.lang = l2.lang  and not isNull(l0.customer, '') = '' and isNull(l2.customer, '') = ''");
                sql.AppendLine("where not l0.id in (");
                sql.AppendLine("select l.id from t_language l");
                sql.AppendLine("left join t_language l1 on l.keyid = l1.keyid and l.lang = l1.lang and l1.customer = @customer");
                sql.AppendLine("where not l.id = l1.id )");
                sql.AppendLine("and l0.lang = @language");
            }

            return _connection.Query<Translation>(sql.ToString(), new { customer, language }).ToList();
        }

    }
}