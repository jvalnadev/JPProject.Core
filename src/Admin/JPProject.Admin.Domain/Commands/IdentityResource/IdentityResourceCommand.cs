using System;
using JPProject.Domain.Core.Commands;

namespace JPProject.Admin.Domain.Commands.IdentityResource
{
    public abstract class IdentityResourceCommand : Command
    {
        public IdentityServer4.Models.IdentityResource Resource { get; protected set; }
        
        public string OldIdentityResourceName { get; protected set; }

    }
}