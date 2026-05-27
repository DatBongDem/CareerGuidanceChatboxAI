# Tổng quan dự án CareerGuidanceChatboxAI (4S_BE)

Đây là tài liệu tổng quan về cấu trúc, chức năng và các thành phần chính của dự án backend `CareerGuidanceChatboxAI`.

## 1. Cấu trúc dự án

Dự án được xây dựng theo kiến trúc 3 lớp (3-Tier Architecture) rõ ràng, giúp tách biệt các mối quan tâm và tăng khả năng bảo trì, mở rộng:

-   **`WebAPI` (Presentation Layer):** Lớp giao diện, chịu trách nhiệm tiếp nhận các yêu cầu HTTP từ client, gọi các service tương ứng ở lớp BusinessLogic và trả về kết quả. Lớp này chứa các `Controllers`.
-   **`BusinessLogic` (Business Logic Layer):** Lớp nghiệp vụ, chứa toàn bộ logic xử lý cốt lõi của ứng dụng. Lớp này bao gồm các `Services`, `DTOs` (Data Transfer Objects), `Interfaces` (định nghĩa các hợp đồng cho service), và `Mapping Profiles` (cấu hình AutoMapper).
-   **`DataAccess` (Data Access Layer):** Lớp truy cập dữ liệu, chịu trách nhiệm tương tác với cơ sở dữ liệu (PostgreSQL). Lớp này sử dụng Entity Framework Core và triển khai các mẫu thiết kế `Repository` và `Unit of Work` để trừu tượng hóa việc truy cập dữ liệu.

## 2. Phân tích chi tiết các chức năng

Dưới đây là phân tích chi tiết các chức năng chính của hệ thống, dựa trên các controllers trong `WebAPI`.

---

### 2.1. Xác thực & Quản lý phiên đăng nhập (`AuthController`, `RegisterController`)

Đây là nhóm chức năng quan trọng nhất, quản lý toàn bộ quy trình từ đăng ký, đăng nhập đến quản lý thông tin cá nhân của người dùng.

#### a. Đăng ký nhiều bước (Multi-step Registration)

-   **Endpoint:** `POST /api/auth/register-step1`
-   **Mô tả:** Người dùng cung cấp thông tin cơ bản (email, họ tên, v.v.). Hệ thống kiểm tra email tồn tại, tạo mã OTP, lưu thông tin tạm thời và gửi OTP qua email.
-   **DTO:** `RegisterStep1RequestDto.cs`
-   **Logic chính:** `AuthService.RegisterStep1()`

```csharp
// BusinessLogic/Services/AuthService.cs
public async Task<bool> RegisterStep1(RegisterStep1RequestDto request)
{
    // 1. Check if email already exists
    var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
    if (existingUser != null)
    {
        throw new ApplicationException("Email already registered.");
    }

    // 2. Generate OTP
    string otp = GenerateOtp();

    // 3. Store user's basic info and OTP temporarily
    var emailVerification = new EmailVerification
    {
        Email = request.Email,
        Otp = otp,
        ExpiredAt = DateTime.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
        TemporaryUserData = JsonSerializer.Serialize(request), // Store step1 data
        IsUsed = false
    };
    await _unitOfWork.EmailVerificationRepository.AddAsync(emailVerification);
    await _unitOfWork.SaveAsync();

    // 4. Send OTP to user's email
    string subject = "Verify your email";
    string message = _templateService.GetRegisterOtpTemplate(request.Email, otp);
    await _emailService.SendEmailAsync(request.Email, subject, message);

    return true;
}
```

-   **Endpoint:** `POST /api/auth/verify-otp`
-   **Mô tả:** Xác thực OTP người dùng nhập. Nếu hợp lệ, tạo ra một `verifyToken` để sử dụng cho bước cuối cùng.
-   **DTO:** `RegisterStep2RequestDto.cs`
-   **Logic chính:** `AuthService.VerifyOtp()`

-   **Endpoint:** `POST /api/auth/register-step3`
-   **Mô tả:** Người dùng gửi `verifyToken` và mật khẩu. Hệ thống xác thực token, lấy lại thông tin từ bước 1, hash mật khẩu và tạo người dùng mới trong CSDL.
-   **DTO:** `RegisterStep3RequestDto.cs`
-   **Logic chính:** `AuthService.RegisterStep3()`

#### b. Đăng nhập

-   **Endpoint:** `POST /api/auth/login`
-   **Mô tả:** Xác thực email và mật khẩu. Nếu thành công, tạo ra một `AccessToken` (JWT, 15 phút) và một `RefreshToken` (7 ngày). Refresh token được lưu vào cookie (HttpOnly, Secure) và access token được trả về cho client.
-   **DTO:** `LoginDto.cs`, `LoginResponseDto.cs`
-   **Logic chính:** `AuthService.Login()`

```csharp
// WebAPI/Controllers/AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login(LoginDto loginDto)
{
    try
    {
        var result = await _authService.Login(loginDto);

        Response.Cookies.Append(
            "refreshToken",
            result.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });

        return Ok(new { accessToken = result.AccessToken });
    }
    // ... error handling
}
```

#### c. Lấy thông tin người dùng hiện tại (`/me`)

-   **Endpoint:** `GET /api/auth/me`
-   **Mô tả:** Yêu cầu `Authorize`. Lấy thông tin chi tiết của người dùng đang đăng nhập dựa trên `UserId` từ JWT.
-   **DTO:** `MeResponseDto.cs`
-   **Logic chính:** `AuthService.GetMe()`

#### d. Cập nhật thông tin cá nhân

-   **Endpoint:** `PUT /api/auth/profile`
-   **Mô tả:** Yêu cầu `Authorize`. Cho phép người dùng tự cập nhật thông tin cá nhân của mình (tên, địa chỉ, SĐT, v.v.).
-   **DTO:** `UpdateProfileDto.cs`
-   **Logic chính:** `AuthService.UpdateProfileAsync()`

#### e. Tải lên Avatar

-   **Endpoint:** `POST /api/auth/upload`
-   **Mô tả:** Yêu cầu `Authorize`. Cho phép người dùng tải lên file ảnh avatar. Ảnh được upload lên dịch vụ Cloudinary và URL được lưu vào CSDL.
-   **Logic chính:** `AvatarService.UploadAvatarAsync()`

#### f. Đăng xuất và Làm mới Token

-   **Endpoint:** `POST /api/auth/logout`
-   **Mô tả:** Yêu cầu `Authorize`. Vô hiệu hóa (revoke) refresh token hiện tại.
-   **Endpoint:** `POST /api/auth/refresh-token`
-   **Mô tả:** Sử dụng `refreshToken` từ cookie để tạo một cặp `AccessToken` và `RefreshToken` mới.

---

### 2.2. Quản lý Người dùng (`UsersController`)

Cung cấp các API cho quản trị viên để thực hiện các thao tác CRUD (Create, Read, Update, Delete) trên tài khoản người dùng.

-   `GET /api/Users`: Lấy danh sách tất cả người dùng.
-   `GET /api/Users/{id}`: Lấy thông tin chi tiết một người dùng theo ID.
-   `POST /api/Users`: Tạo một người dùng mới (thường dành cho admin).
-   `PUT /api/Users/{id}`: Cập nhật thông tin một người dùng.
-   `DELETE /api/Users/{id}`: Xóa một người dùng.

**Logic chính:** `UserService.cs`

```csharp
// BusinessLogic/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    // ... constructor

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
        if (user == null) return false;

        _mapper.Map(updateUserDto, user);
        user.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.UserRepository.UpdateAsync(user);
        await _unitOfWork.SaveAsync();
        return true;
    }
    // ... other methods
}
```

---

### 2.3. Quản lý Vai trò (`RolesController`)

Cung cấp các API cho quản trị viên để thực hiện các thao tác CRUD trên các vai trò (ví dụ: ADMIN, STUDENT).

-   `GET /api/Roles`: Lấy danh sách tất cả vai trò và số lượng người dùng trong mỗi vai trò.
-   `POST /api/Roles`: Tạo một vai trò mới.
-   `PUT /api/Roles/{id}`: Cập nhật thông tin một vai trò.
-   `DELETE /api/Roles/{id}`: Xóa một vai trò.

**Logic chính:** `RoleService.cs`

---

### 2.4. Quản lý Gói dịch vụ (`PlansController`)

Quản lý các gói dịch vụ (ví dụ: FREE, VIP) và lịch sử đăng ký của người dùng.

-   `GET /api/Plans`: Lấy danh sách các gói dịch vụ hiện có.
-   `GET /api/Plans/history`: (Yêu cầu `Authorize`) Lấy lịch sử đăng ký gói của người dùng đang đăng nhập.
-   `POST /api/Plans/register-vip`: (Yêu cầu `Authorize`) Cho phép người dùng đăng ký gói VIP.

**Logic chính:** `PlanService.cs`

---

## 3. Mô hình Dữ liệu (`DataAccess/Entities`)

Các thực thể chính định nghĩa cấu trúc của cơ sở dữ liệu.

-   **`User.cs`**: Lưu trữ thông tin người dùng, bao gồm thông tin cá nhân, mật khẩu đã hash, `RoleId`, và các mối quan hệ.
-   **`Role.cs`**: Định nghĩa các vai trò trong hệ thống.
-   **`Plan.cs`**: Định nghĩa các gói dịch vụ.
-   **`PlanHistory.cs`**: Ghi lại lịch sử khi người dùng đăng ký một gói dịch vụ.
-   **`RefreshToken.cs`**: Lưu trữ các refresh token đã được cấp cho người dùng.
-   **`EmailVerification.cs`**: Lưu trữ thông tin tạm thời cho quá trình đăng ký qua email (OTP, token xác thực).

```csharp
// DataAccess/Entities/User.cs
public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Gender  { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DOB { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginTime { get; set; }

    [ForeignKey("Role")]
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PlanHistory> PlanHistories { get; set; } = new List<PlanHistory>();
}
```

## 4. Các công nghệ và thư viện chính

-   **Framework:** ASP.NET Core 8
-   **ORM:** Entity Framework Core 8
-   **Database:** PostgreSQL
-   **Authentication:** JWT (JSON Web Tokens)
-   **Password Hashing:** BCrypt.Net
-   **Mapping:** AutoMapper
-   **Email:** MailKit
-   **File Storage:** Cloudinary
-   **API Documentation:** Swashbuckle (Swagger)
