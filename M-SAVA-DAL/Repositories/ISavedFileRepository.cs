using M_SAVA_DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Repositories
{
    public interface ISavedFileRepository : IIdentifiableRepository<SavedFileReferenceDB>
    {
        string GetFilePath(SavedFileReferenceDB file);
        byte[] GetFileBytes(SavedFileReferenceDB file);
        FileStream GetFileStream(SavedFileReferenceDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read);
        Task<FileStream> GetFileStreamAsync(SavedFileReferenceDB file, FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, CancellationToken cancellationToken = default);
        bool FileExists(SavedFileReferenceDB file);

        public Guid GetFileIdByHashAndExtension(byte[] fileHash, FileExtensionType extension);
        public bool FileExistsByHashAndExtension(byte[] fileHash, FileExtensionType extension);

        void SaveFileFromBytes(SavedFileReferenceDB file, byte[] content, bool overwrite = false);
        void SaveFileFromStream(SavedFileReferenceDB file, Stream content, bool overwrite = false);
        Task SaveFileFromStreamAsync(SavedFileReferenceDB file, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
        Task SaveFileFromFormFileAsync(SavedFileReferenceDB file, IFormFile formFile, bool overwrite = false, CancellationToken cancellationToken = default);
    }
}
