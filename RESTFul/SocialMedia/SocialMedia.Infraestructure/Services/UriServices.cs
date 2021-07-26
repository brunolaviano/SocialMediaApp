using SocialMedia.Core.QueryFilters;
using SocialMedia.Infraestructure.Interfaces;
using System;

namespace SocialMedia.Infraestructure.Services
{
    public class UriServices : IUriService
    {
        private readonly string _baseUri;

        public UriServices(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Uri GetPostPaginationUri(PostQueryFilter filter, string actionUrl)
        {
            string baseUrl = $"{_baseUri}{actionUrl}";
            return new Uri(baseUrl);
        }
    }
}
