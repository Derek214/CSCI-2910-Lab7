using Lab5.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.Services
{
    public class LibraryService : ILibraryService
    {
        private const string BooksFile = "./Data/Books.csv";
        private const string UsersFile = "./Data/Users.csv";
        private const string BorrowedBooksFile = "./Data/BorrowedBooks.json";


        public List<Book> Books { get; private set; }
        public List<User> Users { get; private set; }
        public Dictionary<int, List<Book>> BorrowedBooks { get; private set; } = new Dictionary<int, List<Book>>();

        public LibraryService()
        {
            Books = new List<Book>();
            Users = new List<User>();
        }

        public async Task InitializeBooksAsync()
        {
            Books = await ReadBooks();
            Users = await ReadUsers();
            LoadBorrowedBooks();
        }

        public async Task<List<Book>> ReadBooks()
        {
            List<Book> books = new List<Book>();
            try
            {
                await foreach (string line in File.ReadLinesAsync(BooksFile))
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 100)
                    {
                        Book book = new Book
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Title = fields[1].Trim(),
                            Author = fields[2].Trim(),
                            ISBN = fields[3].Trim()
                        };

                        books.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return books;
        }

        public async Task<List<User>> ReadUsers()
        {
            List<User> users = new List<User>();
            try
            {
                await foreach (string line in File.ReadLinesAsync(UsersFile))
                {
                    string[] fields = line.Split(',');

                    if (fields.Length >= 3)
                    {
                        User user = new User
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Name = fields[1].Trim(),
                            Email = fields[2].Trim()
                        };

                        users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return users;
        }

        public async Task AddBook(Book book)
        {
            Books = await ReadBooks();
            Books.Add(book);
            await WriteBooks();
        }

        public async Task EditBook(int bookId, string newTitle, string newAuthor, string ISBN)
        {
            Book book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book != null)
            {
                book.Title = newTitle;
                book.Author = newAuthor;
                book.ISBN = ISBN;
                await WriteBooks();
            }
        }

        public async Task DeleteBook(int bookId)
        {
            Books.RemoveAll(b => b.Id == bookId);
            await WriteBooks();
        }

        public async Task AddUser(User user)
        {
            Users = await ReadUsers();
            Users.Add(user);
            await WriteUsers();
        }

        public async Task EditUser(int userId, string newName, string newEmail)
        {
            User user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.Name = newName;
                user.Email = newEmail;
                await WriteUsers();
            }
        }

        public async Task DeleteUser(int userId)
        {
            Users.RemoveAll(u => u.Id == userId);
            await WriteUsers();
        }

        public async Task<string> BorrowBook(int bookId, int userId)
        {
            Book book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null) return "Book not found!";

            var userExists = Users.Any(u => u.Id == userId);
            if (!userExists) return "User not found!";

            if (!BorrowedBooks.ContainsKey(userId))
                BorrowedBooks[userId] = new List<Book>();

            BorrowedBooks[userId].Add(book);
            Books.Remove(book);

            await WriteBooks();
            await SaveBorrowedBooks(); 

            return "Book borrowed successfully!";
        }

        private async Task SaveBorrowedBooks()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(BorrowedBooks);
                await File.WriteAllTextAsync(BorrowedBooksFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving borrowed books: {ex.Message}");
            }
        }


        public async Task<string> ReturnBook(int userId, int bookIndex)
        {
            if (!BorrowedBooks.ContainsKey(userId) || bookIndex < 0 || bookIndex >= BorrowedBooks[userId].Count)
                return "Invalid selection!";

            var book = BorrowedBooks[userId][bookIndex];
            BorrowedBooks[userId].RemoveAt(bookIndex);
            Books.Add(book);

            if (BorrowedBooks[userId].Count == 0)
                BorrowedBooks.Remove(userId);

            await WriteBooks();
            await SaveBorrowedBooks(); 

            return "Book returned successfully!";
        }

        public List<Book> GetBorrowedBooks(int userId)
        {
            LoadBorrowedBooks();

            return BorrowedBooks.ContainsKey(userId) ? BorrowedBooks[userId] : new List<Book>();
        }


        private void LoadBorrowedBooks()
        {
            try
            {
                if (File.Exists(BorrowedBooksFile))
                {
                    string json = File.ReadAllText(BorrowedBooksFile);
                    BorrowedBooks = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, List<Book>>>(json)
                                    ?? new Dictionary<int, List<Book>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowed books: {ex.Message}");
                BorrowedBooks = new Dictionary<int, List<Book>>(); 
            }
        }


        private async Task WriteBooks()
        {
            List<string> bookData = Books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}").ToList();

            await File.WriteAllLinesAsync(BooksFile, bookData);
        }

        private async Task WriteUsers()
        {
            List<string> userData = Users.Select(u => $"{u.Id},{u.Name},{u.Email}").ToList();

            await File.WriteAllLinesAsync(UsersFile, userData);
        }

    }
}
