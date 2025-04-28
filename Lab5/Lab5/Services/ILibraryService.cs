using Lab5.Models;
using System.Threading.Tasks;

namespace Lab5.Services
{
    public interface ILibraryService
    {
        List<Book> Books { get; }
        List<User> Users { get; }

        Task<List<Book>> ReadBooks();  
        Task<List<User>> ReadUsers();
        Task InitializeBooksAsync();

        Task AddBook(Book book);      
        Task EditBook(int bookId, string newTitle, string newAuthor, string ISBN); 
        Task DeleteBook(int bookId);     

        Task AddUser(User user);       
        Task EditUser(int userId, string newName, string newEmail); 
        Task DeleteUser(int userId);

        Task<string> BorrowBook(int bookId, int userId);
        Task<string> ReturnBook(int userId, int bookIndex);
        public List<Book> GetBorrowedBooks(int userId);
    }
}
