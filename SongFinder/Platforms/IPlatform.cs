using SongFinder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongFinder.Platforms
{
    public interface IPlatform
    {
        IEnumerable<SongFinderResponse> Search(string query);
        Task<SongFinderResponse> ExtractAndSave(string url);
    }
}
