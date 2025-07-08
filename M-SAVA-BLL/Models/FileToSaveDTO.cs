using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Models
{
    public class FileToSaveDTO
    {
        public Guid? Id { get; set; }
        public required string FileName { get; set; }
        public required string FileExtension { get; set; }
        public Stream? Stream { get; set; } = null!;
        public List<string>? Tags { get; set; } = null!;
        public List<string>? Categories { get; set; } = null!;
        public Guid? AccessGroup { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public bool Restricted { get; set; } = false;
        public bool PublicDownload { get; set; } = false;
    }
}
