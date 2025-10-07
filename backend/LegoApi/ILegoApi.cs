using Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegoApi {
    public interface ILegoApi {
        public Task<IEnumerable<LegoSet>> SearchLegoSetFromCode(string code);
        public Task<IEnumerable<LegoPiece>> GetAllPieceFromSet(string apiId);
    }
}
