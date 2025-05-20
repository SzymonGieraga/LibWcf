using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using LibraryService.DataContracts;
using LibraryService.ServiceContracts;

namespace LibraryService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class LibraryServiceImplementation : ILibraryService
    {
        private readonly Book[] books;

        public LibraryServiceImplementation()
        {
            try
            {
                books = LoadBooks("C:\\Users\\MOBILE-TECHNOLOGY\\Desktop\\Data.txt");
                Console.WriteLine("[INFO] Book data loaded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] FAILED TO LOAD BOOK DATA: " + ex.Message.ToUpper());
                books = Array.Empty<Book>();
            }
        }

        public int[] findBooksByKeyword(string keyword)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received findBooksByKeyword query for keyword: '{keyword}'");

            if (string.IsNullOrWhiteSpace(keyword))
                return Array.Empty<int>();

            return books
                .Where(book => !string.IsNullOrWhiteSpace(book.title) &&
                               book.title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(book => book.id)
                .ToArray();
        }

        public Book getBookById(int id)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Received getBookById query for ID: {id}");

            var book = books.FirstOrDefault(b => b.id == id);

            if (book != null)
            {
                return book;
            }

            Console.WriteLine($"[ERROR] BOOK WITH ID {id} NOT FOUND.");
            throw new FaultException<BookNotFound>(
                new BookNotFound(id),
                new FaultReason("NO BOOK WITH THE GIVEN IDENTIFIER.")
            );
        }

        private Book[] LoadBooks(string path)
        {
            var lines = File.ReadAllLines(path);
            var authorCache = new Dictionary<string, Author>(StringComparer.OrdinalIgnoreCase);
            var bookTitleSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Author currentAuthor = null;

            // Pierwsze przejście — zlicz unikalne tytuły książek
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("a."))
                {
                    string authorFullName = trimmedLine.Substring(2).Trim().TrimEnd(':');
                    if (!authorCache.ContainsKey(authorFullName))
                    {
                        var nameParts = authorFullName.Split(' ');
                        authorCache[authorFullName] = new Author
                        {
                            firstName = nameParts.FirstOrDefault() ?? "",
                            lastName = string.Join(" ", nameParts.Skip(1))
                        };
                    }
                    currentAuthor = authorCache[authorFullName];
                }
                else if (trimmedLine.StartsWith("b.") && currentAuthor != null)
                {
                    string bookTitle = trimmedLine.Substring(2).Trim();
                    bookTitleSet.Add(bookTitle); // unikaty tytułów
                }
            }

            // Przygotuj tablicę o odpowiednim rozmiarze
            Book[] books = new Book[bookTitleSet.Count];
            Dictionary<string, Book> bookDict = new Dictionary<string, Book>(StringComparer.OrdinalIgnoreCase);
            int index = 0;
            authorCache.Clear(); // wyczyść cache, by ponownie przypisać autorów
            currentAuthor = null;

            // Drugie przejście — uzupełnij książki
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("a."))
                {
                    string authorFullName = trimmedLine.Substring(2).Trim().TrimEnd(':');
                    if (!authorCache.TryGetValue(authorFullName, out var cachedAuthor))
                    {
                        var nameParts = authorFullName.Split(' ');
                        cachedAuthor = new Author
                        {
                            firstName = nameParts.FirstOrDefault() ?? "",
                            lastName = string.Join(" ", nameParts.Skip(1))
                        };
                        authorCache[authorFullName] = cachedAuthor;
                    }
                    currentAuthor = cachedAuthor;
                }
                else if (trimmedLine.StartsWith("b.") && currentAuthor != null)
                {
                    string bookTitle = trimmedLine.Substring(2).Trim();

                    if (!bookDict.TryGetValue(bookTitle, out var book))
                    {
                        book = new Book
                        {
                            id = index,
                            title = bookTitle,
                            authors = new[] { currentAuthor }
                        };
                        books[index++] = book;
                        bookDict[bookTitle] = book;
                    }
                    else
                    {
                        if (!book.authors.Any(a => a.firstName == currentAuthor.firstName &&
                                                   a.lastName == currentAuthor.lastName))
                        {
                            var authors = book.authors.ToList();
                            authors.Add(currentAuthor);
                            book.authors = authors.ToArray();
                        }
                    }
                }
            }

            return books;
        }
    }
}
