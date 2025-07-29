using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class AccessLogDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public UserDB? User { get; set; }
        public Guid FileRefId { get; set; }
        public SavedFileReferenceDB? FileRef { get; set; }
        public AccessLogActions Action { get; set; }

    }
}
