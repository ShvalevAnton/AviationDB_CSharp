using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace AviationDB_CSharp
{
    public static class AppServices
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static T GetService<T>() where T : class
        {
            return ServiceProvider?.GetService<T>();
        }

        public static T GetRequiredService<T>() where T : class
        {
            return ServiceProvider?.GetRequiredService<T>();
        }
    }
}
