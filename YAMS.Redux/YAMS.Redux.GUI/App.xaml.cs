using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace YAMS.Redux.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public IConfiguration Config { get; set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appconfig.json", optional: false, reloadOnChange: true)
             .AddEnvironmentVariables();

            Config = builder.Build();

            //var tmp = Configuration["ConnectionString"]["YAMS"];


        }


    }

}
