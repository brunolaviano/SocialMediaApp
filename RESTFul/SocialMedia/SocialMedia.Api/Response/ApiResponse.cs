using SocialMedia.Core.CustomEntities;

namespace SocialMedia.Api.Response
{
    public class ApiResponse<T>
    {
        public ApiResponse(T data)
        {
            Data = data;
        }    

        public T Data { get; set; }

        public Metadata Meta { get; set; }
    }
}
