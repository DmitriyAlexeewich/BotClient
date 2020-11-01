﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Models.Role
{
    public class RoleMissionConnectorModel
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int RoleId { get; set; }
        public int MissionId { get; set; }
        public bool isActive { get; set; }
    }
}
