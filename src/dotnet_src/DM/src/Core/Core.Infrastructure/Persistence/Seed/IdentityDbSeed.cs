

using Core.Application.Abstractions;
using Core.Application.Abstractions.Handlers;
using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.Constants;

namespace Core.Infrastructure.Persistence.Seed;

public class IdentityDbSeed(
    IUnitOfWork unitOfWork,
    IIdentityHandler identityService,
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IUserService userService,
    IModeratorProfileRepository moderatorProfileRepository,
    ICandidateProfileRepository candidateProfileRepository,
    ICompanyProfileRepository  companyProfileRepository,
    IConfiguration configuration,
    IManageBlogService manageBlogService,
    IQueryBlogRepository queryBlogRepository,
    ICommandBlogHandler commandBlogHandler
    // , IProfileAccountService accountService
) 
{
    private string[] _roles = new[]
    {
        RoleConstants.Moderator,
    };


    public async Task SeedData(CancellationToken cancellationToken)
    {
        await unitOfWork.StartTransaction(cancellationToken);
        foreach (var role in _roles)
        {
            await AddRole(role, cancellationToken);
        }

        string[] userRoles =
            [RoleConstants.Moderator, ];
        var user = await AddUser(configuration["SeedData:ModeratorTestAccountEmail"]!,
            configuration["SeedData:ModeratorTestAccountPassword"]!,
            userRoles,
            cancellationToken, true);

        if (user.Id == 0)
        {
            await unitOfWork.RollbackTransaction(cancellationToken);
            throw new InvalidOperationException(
                $"Creation seed user was failing! {configuration["SeedData:ModeratorTestAccountEmail"]}");
        }
       
        await userService.UpdateRolesByEmail(user.Email, userRoles, cancellationToken);
        
        

        var moderator = await moderatorProfileRepository.GetByUserId(user.Id,
            cancellationToken);

        if (moderator == null)
        {
            moderator = new Moderator()
            {
                UserId = user.Id,
                Email = configuration["SeedData:ModeratorTestAccountEmail"]!,
                Name = configuration["SeedData:ModeratorTestAccountEmail"]!,
                GuidId = Guid.NewGuid() // = Guid.CreateVersion7()
            };
            moderator = await moderatorProfileRepository.Create(moderator, cancellationToken);

            if (moderator.Id == 0)
                throw new InvalidOperationException(
                    $"Creation seed moderator was failing! {configuration["SeedData:ModeratorTestAccountEmail"]}");
            // await manageBlogService.UserBlogSetAllow(moderator.UserId, cancellationToken,
            //     needTransaction: false);
        }

        
        var candidate = await candidateProfileRepository.GetByUserId(user.Id,
            cancellationToken);
        
        if (candidate == null)
        {
            candidate = new Candidate
            {
                UserId = user.Id,
                Email = configuration["SeedData:ModeratorTestAccountEmail"]!,
                Name = configuration["SeedData:ModeratorTestAccountEmail"]!,
                GuidId = Guid.NewGuid() // = Guid.CreateVersion7()
            };
            candidate = await candidateProfileRepository.Create(candidate, cancellationToken);
        
            if (candidate.Id == 0)
                throw new InvalidOperationException(
                    $"Creation seed candidate was failing! {configuration["SeedData:ModeratorTestAccountEmail"]}");
            // await manageBlogService.UserBlogSetAllow(moderator.UserId, cancellationToken,
            //     needTransaction: false);
        }
        
        
        var company = await companyProfileRepository.GetByUserId(user.Id,
            cancellationToken);
        
        if (company == null)
        {
            company = new Company()
            {
                UserId = user.Id,
                Email = configuration["SeedData:ModeratorTestAccountEmail"]!,
                Name = configuration["SeedData:ModeratorTestAccountEmail"]!,
                GuidId = Guid.NewGuid() // = Guid.CreateVersion7()
            };
            company = await companyProfileRepository.Create(company, cancellationToken);
        
            if (company.Id == 0)
                throw new InvalidOperationException(
                    $"Creation seed candidate was failing! {configuration["SeedData:ModeratorTestAccountEmail"]}");
            // await manageBlogService.UserBlogSetAllow(moderator.UserId, cancellationToken,
            //     needTransaction: false);
        }

        var blog = await queryBlogRepository.GetBlogByUserId(moderator.UserId, cancellationToken);

        if (blog == null)
        {
            blog = await commandBlogHandler.UserBlogSetAllow(moderator.UserId,
                cancellationToken);
        }


        if (blog is null)
            throw new InvalidOperationException("Creation blog was failing!");

        await unitOfWork.CommitTransaction(cancellationToken);
    }

    public async Task AddRoles(CancellationToken cancellationToken)
    {
        await unitOfWork.StartTransaction(cancellationToken);
        foreach (var role in _roles)
        {
            await AddRole(role, cancellationToken);
        }

        await unitOfWork.CommitTransaction(cancellationToken);
    }


    public async Task AddRole(string alias, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByAlias(alias, cancellationToken);
        if (role == null)
        {
            role = new()
            {
                Alias = alias,
                Title = alias
            };
            await roleRepository.Create(role, cancellationToken);
        }
    }


    public async Task<User> AddUser(string email, string password,
        string[] aliases, CancellationToken cancellationToken
        , bool allowUserBlog = false)
    {
        var user = await userRepository.GetByEmail(email, cancellationToken);

        if (user == null)
        {
            var dto = new LoginEmailRequestDto(email, password);

            user = await userService.CreateUserByEmail(dto,
                aliases,
                cancellationToken,
                new User()
                {
                    EmailConfirmedStatus = true,
                    EmailConfirmedDateChanged = DateTimeApplication.GetCurrentDate(),
                    AllowedToCreateBlogStatus = allowUserBlog,
                    AllowedToCreateBlogDate = DateTimeApplication.GetCurrentDate(),
                });
        }

        return user;
    }
}