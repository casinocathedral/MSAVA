﻿using System;
using System.Collections.Generic;

namespace M_SAVA_Core.Models
{
    public class AccessGroupDTO
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccessGroupDTO? SubGroups { get; set; }
    }
}
