using KurguWebsite.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Services
{
    public class AppEnvironment : IAppEnvironment
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AppEnvironment(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string WebRootPath => _webHostEnvironment.WebRootPath;

        public string ContentRootPath => _webHostEnvironment.ContentRootPath;
    }
}
