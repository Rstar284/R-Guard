using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGuard.Database.Models
{
    class UserWarnModel
    {
        public int Id { get; set; }
        public ulong UserId {get; set;}
        public ulong GuildId { get; set; }
        public string Reason { get; set; }
        public ulong WarnerId { get; set; }
    }
}
