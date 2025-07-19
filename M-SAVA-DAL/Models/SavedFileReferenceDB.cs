using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Models
{
    public class SavedFileReferenceDB : IIdentifiableDB
    {
        public Guid Id { get; set; }
        public required byte[] FileHash { get; set; }
        public required FileExtensionType FileExtension { get; set; }

        // permissions
        public required bool PublicDownload { get; set; } = false;
        public required bool PublicViewing { get; set; } = false;
        public AccessGroupDB? AccessGroup { get; set; }
        public Guid? AccessGroupId { get; set; }
    }
}
