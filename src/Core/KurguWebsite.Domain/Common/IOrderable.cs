using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Common
{
    public interface IOrderable
    {
        int DisplayOrder { get; }
        void SetDisplayOrder(int order);
    }
}
