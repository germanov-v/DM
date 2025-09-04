using Core.Application.Abstractions.Services;
using Core.Application.Common.Results;
using Core.Domain.SharedKernel.ValueObjects;

namespace Core.Application.Abstractions.BusinessLogic.Identity;

public interface IRoleService : IApplicationService
{
    Task<Result> UpdateRolesByEmail(IdGuid id, string[] userRoles, CancellationToken cancellationToken);
}