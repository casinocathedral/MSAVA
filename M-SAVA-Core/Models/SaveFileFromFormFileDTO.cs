﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace M_SAVA_Core.Models
{
    public class SaveFileFromFormFileDTO
    {
        public required string FileName { get; set; }
        public required string FileExtension { get; set; }
        public required IFormFile FormFile { get; set; } = null!;
        public required Guid AccessGroupId { get; set; }
        public List<string>? Tags { get; set; } = [];
        public List<string>? Categories { get; set; } = [];
        public string? Description { get; set; } = string.Empty;
        public bool PublicViewing { get; set; } = false;
        public bool PublicDownload { get; set; } = false;
    }
}
