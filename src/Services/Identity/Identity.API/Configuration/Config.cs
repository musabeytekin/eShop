// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using IdentityServer4;

namespace Identity.API.Configuration
{
    public static class Config
    {
        public static IEnumerable<ApiResource> Apis =>
            new ApiResource[]
            {
                new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
            };

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile()
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientName = "interactive",
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { IdentityServerConstants.LocalApi.ScopeName }
                },

                new Client()
                {
                    ClientName = "User",
                    ClientId = "foruser",
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        IdentityServerConstants.LocalApi.ScopeName
                    },
                    AccessTokenLifetime = 1 * 60 * 60, // 1 hour
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    AbsoluteRefreshTokenLifetime = 1 * 60 * 60 * 24 * 30, // 30 days
                    RefreshTokenUsage = TokenUsage.ReUse
                }
            };
    }
}