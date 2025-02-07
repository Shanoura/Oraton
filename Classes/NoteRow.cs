using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oraton.Classes
{
    public class NoteRow
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
    }
}
