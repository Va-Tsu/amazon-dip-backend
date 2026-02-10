using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AmazonClone.Application.Dtos;
using AmazonClone.Application.Interfaces;
using AmazonClone.Domain.Entities;
using AmazonClone.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AmazonClone.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    readonly ApplicationDbContext _dbContext;
    readonly UserManager<IdentityUser> _userManager;
    readonly SignInManager<IdentityUser> _signInManager;
    readonly IConfiguration _config;
    readonly IEmailService _emailService;

    public UserController(ApplicationDbContext context, 
        UserManager<IdentityUser> userManager, 
        SignInManager<IdentityUser> signInManager,
        IConfiguration config, IEmailService emailService)
    {
        _dbContext = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _emailService = emailService;
        
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var identityUser = new IdentityUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(identityUser,registerDto.Password );
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email,
            IdentityUserId = identityUser.Id
        };

        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);

        var link = Url.Action("ConfirmEmail", "User",
            new { id = identityUser.Id, token },
            Request.Scheme);

        // TODO: send link via email
        return Ok(new { message = "User created. Please confirm email.", confirmLink = link });
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string id, string token)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return BadRequest("Invalid user");
        
       
        var result = await _userManager.ConfirmEmailAsync(user, token);
    
        if (!result.Succeeded)
            return BadRequest("Invalid token");
    
        return Ok("Email confirmed!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        if (!user.EmailConfirmed)
            return Unauthorized("Email not confirmed");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid credentials");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var jwt = _config.GetSection("Jwt");
        var getKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(getKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpireMinutes"])),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return BadRequest("No token provided");

        var token = authHeader.Substring("Bearer ".Length).Trim();

        _dbContext.RevokedTokens.Add(new RevokedToken
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddMinutes(30) // same as JWT expiry
        });

        await _dbContext.SaveChangesAsync();

        return Ok("Logged out successfully. Token revoked.");
    }
    
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Ok(); // don't reveal if user exists

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var link = Url.Action(
            "ResetPassword",
            "User",
            new { email = user.Email, token },
            Request.Scheme);

        // send email
        // await _emailService.SendAsync(
        //     user.Email!,
        //     "Reset password",
        //     $"Click here: {link}"
        // );

        return Ok(new {message = "Reset link sent",
                resetLink = link,
                //generatedToken = token
        });
    }
    
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        string email,
        string token,
        string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return BadRequest("Invalid user");

        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            newPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password changed successfully");
    }
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        var user = await _userManager.GetUserAsync(User);

        var result = await _userManager.ChangePasswordAsync(
            user,
            oldPassword,
            newPassword);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok("Password changed");
    }
    
    [Authorize]
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = _dbContext.Users.ToList();
        return Ok(users);
    } 
}