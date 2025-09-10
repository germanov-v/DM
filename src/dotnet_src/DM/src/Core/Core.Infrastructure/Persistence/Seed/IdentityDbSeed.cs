

using Core.Application.Abstractions;
using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Abstractions.Handlers;
using Core.Application.Common.Results;
using Core.Application.Dto.Identity;
using Core.Domain.BoundedContext.Identity.Entities;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.Constants;
using Core.Domain.SharedKernel.ValueObjects;
using Microsoft.Extensions.Configuration;

namespace Core.Infrastructure.Persistence.Seed;

public class IdentityDbSeed(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IConfiguration configuration,
    IEmailPasswordUserProvider emailPasswordUserProvider,
    IRoleService roleService,
    IRoleRepository roleRepository
    
    // IUserService userService,
    // IModeratorProfileRepository moderatorProfileRepository,
    // ICandidateProfileRepository candidateProfileRepository,
    // ICompanyProfileRepository  companyProfileRepository,
    // IManageBlogService manageBlogService,
    // IQueryBlogRepository queryBlogRepository,
    // ICommandBlogHandler commandBlogHandler
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
        var email = configuration["SeedData:ModeratorTestAccountEmail"]!;
        var password = configuration["SeedData:ModeratorTestAccountPassword"]!;
        var resultCreate = await AddUser(email,
            password,
            userRoles,
            cancellationToken, true);

        if (resultCreate.IsFailure)
        {
            await unitOfWork.RollbackTransaction(cancellationToken);
            throw new InvalidOperationException(
                $"Creation seed user was failing! {configuration["SeedData:ModeratorTestAccountEmail"]}");
        }
       
        var resultUpdateRole = await roleService.UpdateRolesByEmail(resultCreate.Value, userRoles, cancellationToken);

        if (resultUpdateRole.IsFailure)
        {
            throw new InvalidOperationException(resultUpdateRole.Error.Message);
        }
        

      

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
            var guid = Guid.NewGuid();
            _ = await roleRepository.Create(guid, alias, alias, cancellationToken);
        }
    }


    public async Task<Result<IdGuid>> AddUser(string email,
        string password,
        string[] roleAliases, CancellationToken cancellationToken
        , bool allowUserBlog = false)
    {
        var user = await userRepository.GetByEmail(email, cancellationToken);

        if (user == null)
        {
        
            var roles = await roleRepository.GetListByAliases(roleAliases, cancellationToken);
            var id = await emailPasswordUserProvider.Create(email, password, email, roleAliases, cancellationToken, true);

            return id;

        }

        return Result<IdGuid>.Ok(user.Id);
    }
}