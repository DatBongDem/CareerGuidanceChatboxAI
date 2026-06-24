using BusinessLogic.DTOs.Edu;
using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BusinessLogic.Services
{
    public class EduService : IEduService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayOSService _payOSService;

        public EduService(IUnitOfWork unitOfWork, IPayOSService payOSService)
        {
            _unitOfWork = unitOfWork;
            _payOSService = payOSService;
        }

        public async Task<EduRegistrationResponseDto> RegisterEduAsync(CreateEduRegistrationDto dto)
        {
            var plan = await _unitOfWork.PlanRepository.GetByIdAsync(dto.PlanId);
            if (plan == null)
            {
                throw new ApplicationException("Không tìm thấy gói Plan này.");
            }

            var registration = new EduRegistration
            {
                Id = Guid.NewGuid(),
                SchoolName = dto.SchoolName,
                ContactName = dto.ContactName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                StudentCount = dto.StudentCount,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                PlanId = dto.PlanId
            };

            await _unitOfWork.EduRegistrationRepository.AddAsync(registration);
            await _unitOfWork.SaveAsync();

            return new EduRegistrationResponseDto
            {
                Id = registration.Id,
                SchoolName = registration.SchoolName,
                ContactName = registration.ContactName,
                Email = registration.Email,
                PhoneNumber = registration.PhoneNumber,
                StudentCount = registration.StudentCount,
                Notes = registration.Notes,
                CreatedAt = registration.CreatedAt,
                Status = registration.Status,
                PlanId = registration.PlanId,
                PlanName = plan.Name,
                TransactionCode = registration.TransactionCode
            };
        }

        public async Task<IEnumerable<EduRegistrationResponseDto>> GetEduRegistrationsAsync()
        {
            var registrations = await _unitOfWork.EduRegistrationRepository.GetAllAsync();
            return registrations.Select(r => new EduRegistrationResponseDto
            {
                Id = r.Id,
                SchoolName = r.SchoolName,
                ContactName = r.ContactName,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                StudentCount = r.StudentCount,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                PlanId = r.PlanId,
                PlanName = r.Plan?.Name,
                TransactionCode = r.TransactionCode
            });
        }

        public async Task<CreatePaymentResponseDto> CreateEduPaymentLinkAsync(Guid registrationId)
        {
            var registration = await _unitOfWork.EduRegistrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
            {
                throw new ApplicationException("Không tìm thấy thông tin đăng ký trường học.");
            }

            if (registration.Status != "Pending")
            {
                throw new ApplicationException("Yêu cầu này đã được thanh toán hoặc đã bị hủy.");
            }

            if (registration.Plan == null)
            {
                throw new ApplicationException("Không tìm thấy gói dịch vụ tương ứng.");
            }

            decimal totalAmount = registration.Plan.Price * registration.StudentCount;

            string transactionCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            registration.TransactionCode = transactionCode;
            await _unitOfWork.EduRegistrationRepository.UpdateAsync(registration);
            await _unitOfWork.SaveAsync();

            var paymentLinkResult = await _payOSService.CreatePaymentLinkAsync(
                transactionCode,
                $"{registration.SchoolName} (EDU)",
                totalAmount
            );

            string qrImageUrl = $"https://img.vietqr.io/image/{paymentLinkResult.Bin}-{paymentLinkResult.AccountNumber}-qr_only.png?amount={(long)totalAmount}&addInfo={Uri.EscapeDataString(paymentLinkResult.Description)}&accountName={Uri.EscapeDataString(paymentLinkResult.AccountName)}";

            return new CreatePaymentResponseDto
            {
                QrCode = paymentLinkResult.QrCode,
                Bin = paymentLinkResult.Bin,
                AccountNumber = paymentLinkResult.AccountNumber,
                AccountName = paymentLinkResult.AccountName,
                Description = paymentLinkResult.Description,
                TransactionCode = transactionCode,
                Amount = totalAmount,
                PlanName = registration.Plan.Name,
                CheckoutUrl = paymentLinkResult.CheckoutUrl,
                QrImageUrl = qrImageUrl
            };
        }

        public async Task ConfirmEduPaymentAsync(string transactionCode)
        {
            var registration = await _unitOfWork.EduRegistrationRepository.GetByTransactionCodeAsync(transactionCode);
            if (registration == null)
            {
                throw new ApplicationException("Không tìm thấy giao dịch đăng ký.");
            }

            if (registration.Status == "Paid" || registration.Status == "Completed")
            {
                throw new ApplicationException("Giao dịch đã được xác nhận thanh toán trước đó.");
            }

            registration.Status = "Paid";
            await _unitOfWork.EduRegistrationRepository.UpdateAsync(registration);
            await _unitOfWork.SaveAsync();
        }

        public async Task CancelEduPaymentAsync(string transactionCode)
        {
            var registration = await _unitOfWork.EduRegistrationRepository.GetByTransactionCodeAsync(transactionCode);
            if (registration == null)
            {
                throw new ApplicationException("Không tìm thấy giao dịch đăng ký.");
            }

            if (registration.Status == "Paid" || registration.Status == "Completed")
            {
                throw new ApplicationException("Không thể hủy giao dịch đã thanh toán thành công.");
            }

            registration.Status = "Cancelled";
            await _unitOfWork.EduRegistrationRepository.UpdateAsync(registration);
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<EduActivationKeyResponseDto>> ImportEduKeysAsync(Guid registrationId, IFormFile file)
        {
            var registration = await _unitOfWork.EduRegistrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
            {
                throw new ApplicationException("Không tìm thấy thông tin đăng ký trường học.");
            }

            if (registration.Status != "Paid" && registration.Status != "Completed")
            {
                throw new ApplicationException("Đăng ký trường học này chưa được thanh toán thành công.");
            }

            if (file == null || file.Length == 0)
            {
                throw new ApplicationException("File trống hoặc không hợp lệ.");
            }

            var emails = new List<string>();

            try
            {
                using (var stream = file.OpenReadStream())
                using (var archive = new ZipArchive(stream))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry == null)
                    {
                        throw new ApplicationException("File Word không đúng định dạng OpenXML (.docx).");
                    }

                    using (var entryStream = entry.Open())
                    {
                        var doc = XDocument.Load(entryStream);
                        var textElements = doc.Descendants().Where(e => e.Name.LocalName == "t");
                        var sb = new System.Text.StringBuilder();
                        foreach (var el in textElements)
                        {
                            sb.Append(el.Value + " ");
                        }
                        var text = sb.ToString();

                        var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
                        var matches = emailRegex.Matches(text);
                        foreach (Match match in matches)
                        {
                            var email = match.Value.Trim().ToLower();
                            if (!string.IsNullOrEmpty(email))
                            {
                                emails.Add(email);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (!(ex is ApplicationException))
            {
                throw new ApplicationException("Có lỗi xảy ra khi đọc tệp Word. Vui lòng đảm bảo tệp .docx không bị lỗi.", ex);
            }

            var distinctEmails = emails.Distinct().ToList();
            if (!distinctEmails.Any())
            {
                throw new ApplicationException("Không tìm thấy địa chỉ email học sinh nào trong tệp Word.");
            }

            var existingKeys = await _unitOfWork.EduActivationKeyRepository.GetAsync(filter: k => k.RegistrationId == registrationId);
            var totalKeysCount = existingKeys.Count() + distinctEmails.Count(e => !existingKeys.Any(ek => ek.Email == e));
            if (totalKeysCount > registration.StudentCount)
            {
                throw new ApplicationException($"Tổng số học sinh sau khi import ({totalKeysCount}) sẽ vượt quá số học sinh đăng ký ({registration.StudentCount}).");
            }

            var resultKeys = new List<EduActivationKey>();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            foreach (var email in distinctEmails)
            {
                var existingKey = existingKeys.FirstOrDefault(k => k.Email == email);
                if (existingKey != null)
                {
                    resultKeys.Add(existingKey);
                    continue;
                }

                string key = string.Empty;
                bool isKeyDuplicate = true;
                while (isKeyDuplicate)
                {
                    var resultChars = new char[8];
                    for (int i = 0; i < 8; i++)
                    {
                        resultChars[i] = chars[random.Next(chars.Length)];
                    }
                    key = "EDU-" + new string(resultChars);
                    
                    var existingKeyGlobal = await _unitOfWork.EduActivationKeyRepository.GetByKeyAsync(key);
                    if (existingKeyGlobal == null)
                    {
                        isKeyDuplicate = false;
                    }
                }

                var newKey = new EduActivationKey
                {
                    Id = Guid.NewGuid(),
                    RegistrationId = registrationId,
                    Email = email,
                    ActivationKey = key,
                    IsUsed = false
                };

                await _unitOfWork.EduActivationKeyRepository.AddAsync(newKey);
                resultKeys.Add(newKey);
            }

            registration.Status = "Completed";
            await _unitOfWork.EduRegistrationRepository.UpdateAsync(registration);
            await _unitOfWork.SaveAsync();

            return resultKeys.Select(rk => new EduActivationKeyResponseDto
            {
                Id = rk.Id,
                RegistrationId = rk.RegistrationId,
                Email = rk.Email,
                ActivationKey = rk.ActivationKey,
                IsUsed = rk.IsUsed,
                UsedByUserId = rk.UsedByUserId,
                ActivatedAt = rk.ActivatedAt
            });
        }

        public async Task ActivateEduKeyAsync(Guid userId, string activationKey)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("Không tìm thấy thông tin tài khoản người dùng.");
            }

            var keyEntity = await _unitOfWork.EduActivationKeyRepository.GetByKeyAsync(activationKey);
            if (keyEntity == null || keyEntity.IsUsed)
            {
                throw new ApplicationException("Mã kích hoạt không hợp lệ hoặc đã được sử dụng trước đó.");
            }

            if (!string.Equals(keyEntity.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("Mã kích hoạt này không dành cho email tài khoản của bạn.");
            }

            if (keyEntity.Registration == null)
            {
                throw new ApplicationException("Giao dịch gốc của mã kích hoạt này không khả dụng.");
            }

            // Deactivate any currently active plans of this user
            var activePlans = await _unitOfWork.PlanHistoryRepository.GetAsync(
                filter: ph => ph.UserId == userId && ph.IsActive && ph.ExpiryDate > DateTime.UtcNow
            );

            foreach (var activePlan in activePlans)
            {
                activePlan.IsActive = false;
                await _unitOfWork.PlanHistoryRepository.UpdateAsync(activePlan);
            }

            // Create a dummy transaction for the user to map database constraints cleanly
            var transactionId = Guid.NewGuid();
            var dummyTransaction = new PaymentTransaction
            {
                TransactionId = transactionId,
                UserId = userId,
                PlanId = keyEntity.Registration.PlanId,
                Amount = 0,
                PaymentMethod = "EDU_ACTIVATION_KEY",
                TransactionCode = keyEntity.ActivationKey,
                CreatedAt = DateTime.UtcNow,
                PaidAt = DateTime.UtcNow
            };

            await _unitOfWork.PaymentTransactionRepository.AddAsync(dummyTransaction);

            // Create PlanHistory
            var planHistory = new PlanHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PlanId = keyEntity.Registration.PlanId,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                TransactionId = transactionId
            };

            await _unitOfWork.PlanHistoryRepository.AddAsync(planHistory);

            // Update Key Status
            keyEntity.IsUsed = true;
            keyEntity.UsedByUserId = userId;
            keyEntity.ActivatedAt = DateTime.UtcNow;
            await _unitOfWork.EduActivationKeyRepository.UpdateAsync(keyEntity);

            await _unitOfWork.SaveAsync();
        }

        public async Task<EduRegistrationResponseDto?> GetEduRegistrationByTransactionCodeAsync(string transactionCode)
        {
            var r = await _unitOfWork.EduRegistrationRepository.GetByTransactionCodeAsync(transactionCode);
            if (r == null) return null;

            return new EduRegistrationResponseDto
            {
                Id = r.Id,
                SchoolName = r.SchoolName,
                ContactName = r.ContactName,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                StudentCount = r.StudentCount,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                PlanId = r.PlanId,
                PlanName = r.Plan?.Name,
                TransactionCode = r.TransactionCode
            };
        }

        public async Task<EduRegistrationResponseDto> UpdateEduRegistrationStatusAsync(Guid registrationId, string status)
        {
            var r = await _unitOfWork.EduRegistrationRepository.GetByIdAsync(registrationId);
            if (r == null)
            {
                throw new ApplicationException("Không tìm thấy thông tin đăng ký trường học.");
            }

            r.Status = status;
            await _unitOfWork.EduRegistrationRepository.UpdateAsync(r);
            await _unitOfWork.SaveAsync();

            return new EduRegistrationResponseDto
            {
                Id = r.Id,
                SchoolName = r.SchoolName,
                ContactName = r.ContactName,
                Email = r.Email,
                PhoneNumber = r.PhoneNumber,
                StudentCount = r.StudentCount,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                PlanId = r.PlanId,
                PlanName = r.Plan?.Name,
                TransactionCode = r.TransactionCode
            };
        }
    }
}
