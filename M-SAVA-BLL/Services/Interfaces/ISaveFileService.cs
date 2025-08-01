﻿using M_SAVA_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_BLL.Services.Interfaces
{
    public interface ISaveFileService
    {
        Task<Guid> CreateFileFromStreamAsync(SaveFileFromStreamDTO dto, CancellationToken cancellationToken = default);
        Task<Guid> CreateFileFromURLAsync(
            SaveFileFromUrlDTO dto,
            CancellationToken cancellationToken = default);
        Task<Guid> CreateFileFromFormFileAsync(
            SaveFileFromFormFileDTO dto,
            CancellationToken cancellationToken = default);
    }
}