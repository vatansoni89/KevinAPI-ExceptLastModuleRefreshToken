using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Marvin.IDP
{
    public static class Config
    {
        //How to get Claims in Identity Token.
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Frank address"),
                        new Claim("role", "FreeUser"),
                        new Claim("country","nl"),
                        new Claim("subscriptionlevel","FreeUser")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "Claire Address"),
                        new Claim("role", "PayingUser"),
                        new Claim("country","be"),
                        new Claim("subscriptionlevel","PayingUser")
                    }
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(), // SubjectId
                new IdentityResources.Profile(), // given_name, family_name
                new IdentityResources.Address(),
                new IdentityResource("roles","Your Role(s)",new List<string>{ "role"}),
                new IdentityResource("country","The country you are living in",new List<string>{ "country"}),
                new IdentityResource("subscriptionlevel","Your subscription level",new List<string>{ "subscriptionlevel"})
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            //Global scope
            return new List<ApiResource>
            {
                new ApiResource("imagegalleryapi","Image Gallery API" /*Dislay Name*/,new List<string> { "role"}/* List of claims returned when requesting the imagegalleryapi*/)
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>()
            {
                //Adding website application
                new Client()
                {
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:44365/signin-oidc" //Web application URL
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:44365/signout-callback-oidc" //Web application URL
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile, // This is must now. should be same as Clien
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi", //Local scope for web app.
                        "subscriptionlevel",
                        "country"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                }
            };
        }
    }
}