using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Interfaces
{
    public interface IAvatarService
    {
        Task<string> UploadAvatarAsync(IFormFile file, Guid userId);
    }
}
