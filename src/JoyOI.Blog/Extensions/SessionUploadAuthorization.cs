using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.AspNetCore.Extensions.BlobStorage;
using JoyOI.Blog.Extensions;

namespace JoyOI.Blog.Extensions
{
    public class SessionUploadAuthorization : IBlobUploadAuthorizationProvider
    {
        private IServiceProvider services;

        public SessionUploadAuthorization(IServiceProvider provider)
        {
            services = provider;
        }

        public bool IsAbleToUpload()
        {
            return true;
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SignedUserUploadAuthorizationProviderServiceCollectionExtensions
    {
        public static IBlobStorageBuilder AddSessionUploadAuthorization(this IBlobStorageBuilder self)
        {
            self.Services.AddSingleton<IBlobUploadAuthorizationProvider, SessionUploadAuthorization>();
            return self;
        }
    }
}