using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

namespace EventFoto.API.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    
    private const int SaltSize = 16;
    private const int KeySize = 32; 
    private const int Iterations = 100_000;

    public AuthService(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }
    
    public async Task<ServiceResult<string>> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetUserWithCredentialsAsync(email);
        if (user is null) return ServiceResult<string>.Fail(AppError.UserNotFound);

        var userCredential = user.Credentials.FirstOrDefault(x => x.Type == CredentialType.Password);
        if (userCredential is null) return ServiceResult<string>.Fail(AppError.NoCredentials);

        var isValid = VerifyPassword(password, userCredential.HashedPassword);
        if (!isValid) return ServiceResult<string>.Fail(AppError.InvalidCredentials);
        
        var token = GenerateToken(user);
        return ServiceResult<string>.Ok(token);

    }
    
    private string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, "User") // Optional: Assign role
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(2),
            signingCredentials: credentials
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private bool VerifyPassword(string plainPassword, string hash)
    {
        var hashSalt = hash.Split(':');
        var decodedSalt = Convert.FromBase64String(hashSalt[0]);
        var decodedPasswordHash = Convert.FromBase64String(hashSalt[1]);

        var newHash = KeyDerivation.Pbkdf2(
            password: plainPassword,
            salt: decodedSalt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize
        );
        
        return CryptographicOperations.FixedTimeEquals(newHash, decodedPasswordHash);
    }
}
