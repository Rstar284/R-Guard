using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RGuard.Database.Models
{
    public class Guild
    {
        [Key]
        public ulong Id { get; set; }
        [Display(Name = "Blacklist Enabled")]
        public bool BlacklistWords { get; set; }
        public bool GreetMembers { get; set; }
        public bool LogRoleChange { get; set; }
        [Required]
        [StringLength(2)]
        public string Prefix { get; set; }

        public string InfractionFormat { get; set; }

        public ulong MuteRoleId { get; set; }
        public ulong MessageEditChannel { get; set; }

        public ulong GeneralLoggingChannel { get; set; }
        public ulong GreetingChannel { get; set; }
        public List<Ban> Bans { get; set; } = new List<Ban>();
        public List<UserModel> Users { get; set; } = new List<UserModel>();

    }
}
