using M_SAVA_BLL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace M_SAVA_BLL.Services
{
    public interface IReturnFileService
    {
        StreamReturnFileDTO GetFileStreamById(Guid id);
        PhysicalFileResult GetFileByPath(string filePath);
        PhysicalReturnFileDTO GetPhysicalFileReturnData(string filePath);
        PhysicalReturnFileDTO GetPhysicalFileReturnDataById(Guid id);
    }
}
