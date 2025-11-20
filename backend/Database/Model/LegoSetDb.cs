using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Model {
    public class LegoSetDb {
        public int Id { get; set; }
        public string ApiId { get; set; }
        public string LegoCode { get; set; }
        public string Name { get; set; }
        public int? Year { get; set; }
        public string? Url { get; set; }
        public int? ThemeId { get; set; }
        public LegoSetState State { get; set; }
        public LegoThemeDb? Theme { get; set; }
        public ICollection<LegoSetPieceDb> Pieces { get; set; }
    }

    public enum LegoSetState {
        /**
         * Purchased new, still sealed in the box.
         */
        NEW_SEALED,

        /**
         * Assembled and on display.
         */
        ASSEMBLED,

        /**
         * Disassembled, with all parts and the original box.
         */
        DISASSEMBLED_WITH_BOX,

        /**
         * Disassembled, with all parts, but without the original box.
         */
        DISASSEMBLED_WITHOUT_BOX,

        /**
         * Missing parts, but replacements have been purchased and are awaited.
         */
        REPLACEMENT_PARTS_ON_ORDER
    }
}
