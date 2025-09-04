using Core.Application.Abstractions.BusinessLogic.Identity;
using Core.Application.Common.Results;
using Core.Domain.BoundedContext.Identity.Repositories;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Application.BusinessLogic.Identity;

public class RoleService : IRoleService
{
    
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> UpdateRolesByEmail(IdGuid id, string[] userRoles, CancellationToken cancellationToken)
    {
        var countUpdated = await _roleRepository.UpdateRoles(id,userRoles, cancellationToken);

        if (countUpdated != userRoles.Length)
        {
            return Result.Fail(new Error("Count roles updated not equal original count roles"));
        }
        return Result.Ok();
    }
}