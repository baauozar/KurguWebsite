using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Domain.Common
{
    public interface ISeoEntity
    {
        string Slug { get; }
        string? MetaTitle { get; }
        string? MetaDescription { get; }
        string? MetaKeywords { get; }
    }
}
