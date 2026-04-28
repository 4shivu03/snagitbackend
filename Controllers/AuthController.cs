using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using backend.Services;
using Microsoft.EntityFrameworkCore;
using backend.DTOs.Auth;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
         private readonly EmailHelper _emailHelper;        
        private readonly AppDbContext _context;
        private readonly JwtService _jwt;
        public AuthController(AppDbContext context, JwtService jwt, EmailHelper emailHelper)
        {
            _context = context;
            _jwt = jwt;
            _emailHelper = emailHelper;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequest request)
        {
         try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _context.TblUserLogins.AnyAsync(x => x.Email == request.Email))
                return BadRequest("Email already exists");
            if (await _context.TblUserLogins.AnyAsync(x => x.Mobile == request.Mobile))
                return BadRequest("Mobile already exists");
            var hasher = new PasswordHasher<TblUserLogin>();
            var user = new TblUserLogin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Mobile = request.Mobile,
                Role = request.Role,
                IsActive = request.IsActive,
                Status = "A",
                CreatedBy = request.Email
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);
            _context.TblUserLogins.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedBy = user.Email
            });
            }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                inner = ex.InnerException?.Message
            });
        }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _context.TblUserLogins
                .FirstOrDefaultAsync(x => x.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email");
            var hasher = new PasswordHasher<TblUserLogin>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid password");
            if (user.Status != "A")
            {
                if (user.Status == "P")
                    return Unauthorized("Account pending approval");
                if (user.Status == "R")
                    return Unauthorized("Account rejected by admin");
            }
            if (user.IsActive == false)
                return Unauthorized("User is deactivated");
            user.LastLoginDate = DateTime.UtcNow;
            user.UpdatedBy = user.Id;
            user.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            var jwtToken = _jwt.GenerateToken(user.Email, user.Role);
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id,
                CreatedBy = user.Id
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return Ok(new LoginResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                User = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Mobile = user.Mobile,
                    Role = user.Role
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var token = await _context.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
            if (token == null || token.IsRevoked || token.ExpiryDate < DateTime.Now)
                return Unauthorized("Invalid refresh token");
            var newJwt = _jwt.GenerateToken(token.User.Email, token.User.Role);
            token.IsRevoked = true;
            var newRefreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = token.UserId,
                CreatedBy = token.UserId
            };
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();
            return Ok(new LoginResponse
            {
                Token = newJwt,
                RefreshToken = newRefreshToken.Token,
                User = new UserResponse
                {
                    Id = token.User.Id,
                    FirstName = token.User.FirstName,
                    LastName = token.User.LastName,
                    Email = token.User.Email,
                    Mobile = token.User.Mobile,
                    Role = token.User.Role
                }
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
            if (token == null)
                return Ok("Already logged out");
            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return Ok("Logged out");
        }

        [HttpPost("seller")]
        public async Task<IActionResult> CreateSeller(SellerCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hasher = new PasswordHasher<TblUserLogin>();
                var user = new TblUserLogin
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Mobile = request.Mobile,
                    Role = "S",
                    Status = "P",
                    PasswordHash = hasher.HashPassword(null, request.Password),
                    CreatedBy = request.Email,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = false
                };
                _context.TblUserLogins.Add(user);
                await _context.SaveChangesAsync();
                var seller = new TblSellerDetailMaster
                {
                    UserId = user.Id,
                    BusinessName = request.BusinessName,
                    GSTNumber = request.GSTNumber,
                    PANNumber = request.PANNumber,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    Pincode = request.Pincode,
                    AccountHolderName = request.AccountHolderName,
                    AccountNumber = request.AccountNumber,
                    IFSCCode = request.IFSCCode,
                    BankName = request.BankName,
                    Status = "P",
                    CreatedBy = request.Email,
                    CreatedDate = DateTime.UtcNow
                };
                _context.TblSellerDetailMasters.Add(seller);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok("Seller created successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
       
        [HttpPost("sendotp")]
        public async Task<IActionResult> SendOtp([FromBody] EmailDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await using var transaction = await _context.Database.BeginTransactionAsync();
             try
            {
            var user = await _context.TblUserLogins
                .FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
                return NotFound("User not found");
            var otp = new Random().Next(100000, 999999).ToString();
            var otpEntry = new TblOtp
            {
                Email = dto.Email,
                Otp = otp,
                CreatedBy= dto.Email,
                CreatedDate= DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10)
            };
            _context.TblOtps.Add(otpEntry);
            await _context.SaveChangesAsync();
            await _emailHelper.SendOtpEmail(dto.Email, otp);
            await transaction.CommitAsync();
            return Ok();
             }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var otpEntry = await _context.TblOtps
                .Where(x => x.Email == dto.Email && x.Otp == dto.Otp && !x.IsUsed)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();
            if (otpEntry == null)
                return BadRequest("Invalid OTP");
            if (otpEntry.ExpiryTime < DateTime.UtcNow)
                return BadRequest("OTP expired");
            var user = await _context.TblUserLogins
                .FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null)
                return NotFound();
            user.PasswordHash = dto.NewPassword;
            otpEntry.IsUsed = true;
             otpEntry.UpdatedBy = dto.Email;
             otpEntry.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok();
        }

    }
}
