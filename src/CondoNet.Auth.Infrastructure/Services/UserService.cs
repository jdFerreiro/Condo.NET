using CondoNet.Auth.Core.DTOs;
using CondoNet.Auth.Core.Entities;
using CondoNet.Auth.Core.Interfaces;
using CondoNet.Auth.Infrastructure.Persistence;
using CondoNet.Shared;
using CondoNet.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Auth.Infrastructure.Services
{
    public class UserService(AuthDbContext db, IIdentityService identityService, IPublishEndpoint publishEndpoint) : IUserService
    {
        private readonly AuthDbContext _db = db;
        private readonly IIdentityService _identityService = identityService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request)
        {
            // 1. Validaciones de negocio
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return Result<UserResponse>.Failure("El correo ya está registrado.");

            // 2. Creación de entidades
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim().ToLower(),
                FullName = request.FullName.Trim(),
                PasswordHash = _identityService.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // 3. Persistencia
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new UserCreatedEvent
            {
                UserId = newUser.Id,
                Email = newUser.Email,
                FullName = newUser.FullName
            });


            return Result<UserResponse>.Success(
                new UserResponse(newUser.Id, newUser.Email, newUser.FullName, newUser.IsActive));
        }

        public async Task<Result<List<UserResponse>>> GetUsersByOrganizationAsync(Guid orgId)
        {
            var users = await _db.UserContexts
                .Where(c => c.OrganizationId == orgId)
                .Select(c => new UserResponse(c.User.Id, c.User.Email, c.User.FullName, c.User.IsActive))
                .ToListAsync();

            return Result<List<UserResponse>>.Success(users);
        }
    }
}
