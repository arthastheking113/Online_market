﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Online_market.Services
{
    public interface IUserDetector
    {
        public string GetUserIpAdress();

        public string GetUserConnectionId();
    }
}
