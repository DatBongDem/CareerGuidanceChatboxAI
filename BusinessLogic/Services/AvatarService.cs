using BusinessLogic.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using Npgsql.BackendMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class AvatarService : IAvatarService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IUnitOfWork _unitOfWork;

        public AvatarService(Cloudinary cloudinary, IUnitOfWork unitOfWork)
        {
            _cloudinary = cloudinary;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> UploadAvatarAsync(IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("File is empty");
            }

            var allowed = new[]
            {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

            if (!allowed.Contains(file.ContentType))
            {
                throw new Exception("Invalid file type");
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                throw new Exception("File too large");
            }

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "avatars"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new Exception(result.Error.Message);
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.AvatarUrl = result.SecureUrl.ToString();
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            return user.AvatarUrl;
        }
    }
}
