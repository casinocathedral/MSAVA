using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class AccessGroupDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public IEnumerable<UserDB> Users { get; set; }
        public IEnumerable<AccessGroupDB> SubGroups { get; set; }
        public UserDB Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public AccessCodeDB AccessCode { get; set; }
    }
}
