using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Helpers
{
    public static class SeletectListHelper
    {
        public static IEnumerable<SelectListItem> GetEnumSelectList<T>() where T : Enum
        {
            var list = new List<SelectListItem>();

            foreach (var option in Enum.GetValues(typeof(T)))
            {
                list.Add(new SelectListItem
                {
                    Text = Enum.GetName(typeof(T), option),
                    Value = option.ToString()
                });
            }

            return list;
        }
    }
}
