﻿using System;
using System.Collections;
using System.Collections.Generic;
using API.Extensions;
using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppUser: IdentityUser<int>
    {
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string Introduction { get; set; }
        public string Gender { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Photo> Photos { get; set; }
        
        public ICollection<UserLike> LikedUsers { get; set; }
        public ICollection<UserLike> LikedByUsers { get; set; }
        
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> RecievedMessages { get; set; }
        
        public ICollection<AppUserRole> UserRoles { get; set; }
    }
}