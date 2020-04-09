using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementWebApp.Models
{
    public class Audit
    {
        public Audit()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        [StringLength(256)]
        public string TableName { get; set; }

        public DateTime DateTime { get; set; }

        public string KeyValues { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }
    }
}
