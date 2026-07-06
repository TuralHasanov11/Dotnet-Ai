using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgentFrameworkQuickStart.Workflows
{
    public class Email
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string Content { get; set; } = string.Empty;
    }
}