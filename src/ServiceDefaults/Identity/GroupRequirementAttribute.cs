using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ServiceDefaults.Identity;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class GroupRequirementAttribute :
    AuthorizeAttribute,
    IAuthorizationRequirement,
    IAuthorizationRequirementData
{
    public GroupRequirementAttribute(params string[] groups)
    {
        ArgumentNullException.ThrowIfNull(groups);
        Groups = groups;
    }

    public string[] Groups { get; }

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
