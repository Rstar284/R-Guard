using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RGuard.Database.Models
{
    public class UserModel
    {
        public ulong Id { get; set; }
        [Key]
        public ulong DatabaseId { get; set; }
        public Guild Guild { get; set; }
        public List<UserWarnModel> Infractions { get; set; } = new();
    }
}
