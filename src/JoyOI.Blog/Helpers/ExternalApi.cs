using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using JoyOI.Blog.Helpers;

namespace JoyOI.Blog.Helpers
{
    public class ExternalApi
    {
        private IConfiguration _config;

        public ExternalApi(IConfiguration config)
        {
            _config = config;
        }

        private class ApiResult<T>
        {
            public int code { get; set; }

            public string msg { get; set; }

            public T data { get; set; }
        }

        private class OnlineJudgeUser
        {
            public IEnumerable<string> passedProblems { get; set; }
        }

        public async Task<Dictionary<string, string>> GetAcceptedProblemsAsync(string username)
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(_config["JoyOI:OnlineJudgeUrl"]) })
            {
                var response = await client.GetAsync("/api/user/" + username);
                var user = JsonConvert.DeserializeObject<ApiResult<OnlineJudgeUser>>(await response.Content.ReadAsStringAsync());
                var problemIdsString = string.Join(',', user.data.passedProblems);
                var response2 = await client.GetAsync("/api/problem/title?problemIds=" + WebUtility.UrlEncode(problemIdsString));
                var result = JsonConvert.DeserializeObject<ApiResult<Dictionary<string, string>>>(await response2.Content.ReadAsStringAsync());
                return result.data;
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ExternalApiExtensions
    {
        public static IServiceCollection AddExternalApi(this IServiceCollection self)
        {
            return self.AddSingleton<ExternalApi>();
        }
    }
}