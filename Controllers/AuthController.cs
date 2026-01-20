using AutoMapper;
using JWTPractice.Data;
using JWTPractice.DTOs;
using JWTPractice.Helper;
using JWTPractice.Models;
using JWTPractice.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTPractice.Controllers
{ 

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;

        public AuthController(
            AppDbContext context,
            TokenService tokenService,
            IConfiguration config,
            IMapper mapper)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                return BadRequest("User already exists");

            var user = _mapper.Map<User>(dto);

            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var user = await _context.Users
                .Include(x => x.RefreshTokens)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var accessToken = _tokenService.CreateAccessToken(user);
            var refreshToken = _tokenService.CreateRefreshToken();

            var refreshEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpiryDays"])
                ),
                UserId = user.Id
            };

            _context.RefreshTokens.Add(refreshEntity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                accessToken,
                refreshToken
            });
        }
    }

}
