using Microsoft.EntityFrameworkCore;
using SocialMedia.Core.Entities;
using System.Threading.Tasks;
using SocialMedia.Infraestructure.Data;
using SocialMedia.Core.Interfaces;

namespace SocialMedia.Infraestructure.Repositories
{
    public class SecurityRepository : BaseRepository<Security>, ISecurityRepository
    {
        public SecurityRepository(SocialMediaContext context) : base(context) { }

        public async Task<Security> GetLoginByCredentials(UserLogin login)
        {
            return await _entities.FirstOrDefaultAsync(x => x.User == login.User);
        }
    }
}
