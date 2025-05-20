using System;
using System.Configuration;
using System.ServiceModel;
using LibraryService;
using LibraryService.ServiceContracts;

namespace LibraryServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Uri serviceUri;

            if (!TryBuildServiceUri(out serviceUri))
            {
                Console.WriteLine("INVALID CONFIGURATION PARAMETERS.");
                return;
            }

            Console.WriteLine($"Service URI: {serviceUri}");

            using (ServiceHost host = new ServiceHost(typeof(LibraryServiceImplementation), serviceUri))
            {
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                host.AddServiceEndpoint(typeof(ILibraryService), binding, string.Empty);

                host.Opened += OnHostOpened;
                host.Closed += OnHostClosed;

                try
                {
                    host.Open();
                    Console.WriteLine("Type 'q' and press Enter to stop the server.");

                    string input;
                    do
                    {
                        input = Console.ReadLine()?.ToLower();
                    }
                    while (input != "q");

                    host.Close();
                }
                catch (AddressAlreadyInUseException)
                {
                    Console.WriteLine("ERROR: THE SPECIFIED URI IS ALREADY IN USE.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: AN EXCEPTION OCCURRED DURING SERVER SETUP.");
                    Console.WriteLine(ex.ToString().ToUpper());
                }
            }
        }

        private static bool TryBuildServiceUri(out Uri uri)
        {
            uri = null;

            string protocol = ConfigurationManager.AppSettings["Protocol"] ?? "";
            string address = ConfigurationManager.AppSettings["Address"] ?? "";
            string port = ConfigurationManager.AppSettings["Port"] ?? "";
            string serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "";

            string uriString = $"{protocol}://{address}:{port}/{serviceName}";

            try
            {
                uri = new Uri(uriString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void OnHostOpened(object sender, EventArgs e)
        {
            Console.WriteLine("Library service is now running.");
        }

        private static void OnHostClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Library service has been stopped.");
        }
    }
}
