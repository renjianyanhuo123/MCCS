using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MCCS.Core.WorkflowSettings
{
    internal class Class1
    {

        public void TRR()
        {
            var services = new ServiceCollection();
            services.AddWorkflow();
        }
    }
}
