﻿using System.Web;
using System.Web.Mvc;

namespace UniAlltid.Language.API
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
