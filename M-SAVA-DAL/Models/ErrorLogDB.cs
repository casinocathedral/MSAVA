using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class ErrorLogDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public required string Message { get; set; }
        public required string StackTrace { get; set; }
        public required DateTime Timestamp { get; set; }
        public required Guid? UserId { get; set; }
        public UserDB? User { get; set; }
    }
}
