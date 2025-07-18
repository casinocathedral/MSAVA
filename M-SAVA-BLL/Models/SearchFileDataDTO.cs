using System;
using System.Collections.Generic;
using System.Text.Json;

namespace M_SAVA_BLL.Models
{
    public class SearchFileDataDTO
    {
        public Guid DataId { get; set; }
        public Guid RefId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
        public string FileExtension { get; set; }
        public string[] Tags { get; set; }
        public string[] Categories { get; set; }
        public ulong SizeInBytes { get; set; }
        public string Checksum { get; set; }
        public JsonDocument Metadata { get; set; }
        public bool PublicViewing { get; set; }
        public uint DownloadCount { get; set; }
        public DateTime SavedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public Guid OwnerId { get; set; }
        public Guid LastModifiedById { get; set; }
    }
}
