using BusinessObjects.Models;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISoundBoardRepository:IGenericRepository<SoundBoard>
    {
        Task<List<SoundBoard>> GetAllServerSoundAsync(Guid serverId);
    }
}
