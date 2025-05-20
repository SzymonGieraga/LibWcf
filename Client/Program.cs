using System;
using System.Configuration;
using System.ServiceModel;
using LibraryService.DataContracts;
using LibraryService.ServiceContracts;

namespace LibraryClient
{
    internal class Program
    {
        private static void Main()
        {
            ILibraryService proxy = null;

            try
            {
                string protocol = ConfigurationManager.AppSettings["Protocol"] ?? "";
                string address = ConfigurationManager.AppSettings["ServiceAddress"] ?? "";
                string port = ConfigurationManager.AppSettings["ServicePort"] ?? "";
                string serviceName = ConfigurationManager.AppSettings["ServiceName"] ?? "";

                string uriString = $"{protocol}://{address}:{port}/{serviceName}";
                Uri uri = new Uri(uriString);

                Console.WriteLine("USING URI: " + uri);

                NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
                ChannelFactory<ILibraryService> factory = new ChannelFactory<ILibraryService>(binding);
                EndpointAddress endpoint = new EndpointAddress(uri);

                proxy = factory.CreateChannel(endpoint);
            }
            catch (UriFormatException)
            {
                Console.WriteLine("ERROR: PROVIDED CONFIGURATION PARAMETERS ARE INVALID.");
                return;
            }
            catch (CommunicationException)
            {
                Console.WriteLine("ERROR: FAILED TO CONNECT WITH SERVICE.");
                return;
            }
            catch (TimeoutException)
            {
                Console.WriteLine("ERROR: CONNECTION TO SERVICE TIMED OUT.");
                return;
            }

            Console.WriteLine("CONNECTED TO SERVICE SUCCESSFULLY. PROXY CREATED.");

            bool running = true;
            do
            {
                Console.WriteLine();
                Console.WriteLine("SELECT AN OPTION:");
                Console.WriteLine("1 - SEARCH BOOKS BY KEYWORD");
                Console.WriteLine("2 - GET BOOK BY ID");
                Console.WriteLine("Q - EXIT");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        // Search books by keyword
                        Console.Write("ENTER KEYWORD: ");
                        string keyword = Console.ReadLine();

                        int[] ids = proxy.findBooksByKeyword(keyword);

                        if (ids == null || ids.Length == 0)
                        {
                            Console.WriteLine("NO BOOKS FOUND.");
                        }
                        else
                        {
                            Console.WriteLine("FOUND BOOK IDS: " + string.Join(", ", ids));
                        }
                        break;

                    case "2":
                        // Get book by ID
                        Console.Write("ENTER BOOK ID: ");
                        string input = Console.ReadLine();

                        if (!int.TryParse(input, out int id))
                        {
                            Console.WriteLine("ERROR: INVALID ID FORMAT.");
                            break;
                        }

                        try
                        {
                            Book book = proxy.getBookById(id);

                            Console.WriteLine("BOOK DETAILS:");
                            Console.WriteLine("TITLE: " + book.title);

                            for (int i = 0; i < book.authors.Length; i++)
                            {
                                Console.WriteLine($"AUTHOR {i + 1}: {book.authors[i].firstName} {book.authors[i].lastName}");
                            }
                        }
                        catch (FaultException<BookNotFound>)
                        {
                            Console.WriteLine("BOOK WITH THE GIVEN ID WAS NOT FOUND.");
                        }
                        break;

                    case "q":
                    case "Q":
                        running = false;
                        Console.WriteLine("EXITING PROGRAM.");
                        break;

                    default:
                        Console.WriteLine("ERROR: UNKNOWN OPERATION.");
                        break;
                }
            }
            while (running);

            // Close connection
            if (proxy is ICommunicationObject commObject)
            {
                try
                {
                    commObject.Close();
                }
                catch
                {
                    commObject.Abort();
                }
            }
        }
    }
}