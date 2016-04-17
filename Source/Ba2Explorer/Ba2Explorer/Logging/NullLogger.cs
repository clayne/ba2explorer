﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Logging
{
    internal class NullLogger : ILogger
    {
        public void Log(string message, LogPriority priority)
        {
        }

        public void Log(string format, LogPriority priority, params object[] args)
        {
        }

        public void Dispose()
        {
        }
    }
}
