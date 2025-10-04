using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Common
{
    public interface IActivatable
    {
        bool IsActive { get; }     // read-only outside
        void Activate();           // control via methods
        void Deactivate();
    }
}
