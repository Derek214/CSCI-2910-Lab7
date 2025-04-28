using Microsoft.VisualStudio.TestTools.UnitTesting;

using Lab5.Services;
using Lab5.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryTests
{
    [TestClass]
    [DoNotParallelize]
    public class LibraryServiceTests
    {
        private LibraryService _service;

        [TestInitialize]
        public void Setup()
        {
            // Setup dummy files
            if (!Directory.Exists("./Data"))
                Directory.CreateDirectory("./Data");

            File.WriteAllText("./Data/Books.csv", "1,Book One,Author One,ISBN1\n2,Book Two,Author Two,ISBN2");
            File.WriteAllText("./Data/Users.csv", "1,Alice,alice@example.com\n2,Bob,bob@example.com");

            _service = new LibraryService();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists("./Data/Books.csv")) File.Delete("./Data/Books.csv");
            if (File.Exists("./Data/Users.csv")) File.Delete("./Data/Users.csv");
            if (File.Exists("./Data/BorrowedBooks.json")) File.Delete("./Data/BorrowedBooks.json");
        }

        [TestMethod]
        public async Task AddBook_ShouldAddBookToList()
        {
            // Arrange
            var newBook = new Book
            {
                Id = 3,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "123456789"
            };

            // Act
            await _service.AddBook(newBook);
            var books = await _service.ReadBooks();

            // Assert
            Assert.IsTrue(books.Any(b => b.Id == 3 && b.Title == "Test Book"));
        }

        [TestMethod]
        public async Task EditBook_ShouldUpdateBookInfo()
        {
            // Arrange
            await _service.InitializeBooksAsync();

            // Act
            await _service.EditBook(1, "Updated Title", "Updated Author", "UpdatedISBN");

            // Assert
            var updatedBook = _service.Books.FirstOrDefault(b => b.Id == 1);
            Assert.IsNotNull(updatedBook);
            Assert.AreEqual("Updated Title", updatedBook.Title);
            Assert.AreEqual("Updated Author", updatedBook.Author);
            Assert.AreEqual("UpdatedISBN", updatedBook.ISBN);
        }

        [TestMethod]
        public async Task DeleteBook_ShouldRemoveBook()
        {
            // Arrange
            await _service.InitializeBooksAsync();

            // Act
            await _service.DeleteBook(2);

            // Assert
            var book = _service.Books.FirstOrDefault(b => b.Id == 2);
            Assert.IsNull(book);
        }

        [TestMethod]
        public async Task ReadBooks_ShouldReturnBooks()
        {
            // Arrange
            // Files created in Setup

            // Act
            var books = await _service.ReadBooks();

            // Assert
            Assert.IsNotNull(books);
            Assert.IsTrue(books.Count >= 2);
        }

        [TestMethod]
        public async Task AddUser_ShouldAddNewUser()
        {
            // Arrange
            var newUser = new User
            {
                Id = 3,
                Name = "Charlie",
                Email = "charlie@example.com"
            };

            // Act
            await _service.AddUser(newUser);
            var users = await _service.ReadUsers();

            // Assert
            Assert.IsTrue(users.Any(u => u.Id == 3 && u.Name == "Charlie"));
        }

        [TestMethod]
        public async Task DeleteUser_ShouldRemoveUser()
        {
            // Arrange
            await _service.InitializeBooksAsync();

            // Act
            await _service.DeleteUser(1);
            var users = await _service.ReadUsers();

            // Assert
            Assert.IsFalse(users.Any(u => u.Id == 1));
        }

        [TestMethod]
        public async Task BorrowBook_ShouldSucceedForValidUserAndBook()
        {
            await _service.InitializeBooksAsync();

            var result = await _service.BorrowBook(1, 1);

            Assert.AreEqual("Book borrowed successfully!", result);
            Assert.IsFalse(_service.Books.Any(b => b.Id == 1));
            Assert.IsTrue(_service.GetBorrowedBooks(1).Any(b => b.Id == 1));
        }

        [TestMethod]
        public async Task BorrowBook_ShouldFailForInvalidBook()
        {
            await _service.InitializeBooksAsync();

            var result = await _service.BorrowBook(99, 1);

            Assert.AreEqual("Book not found!", result);
        }

        [TestMethod]
        public async Task BorrowBook_ShouldFailForInvalidUser()
        {
            await _service.InitializeBooksAsync();

            var result = await _service.BorrowBook(1, 99);

            Assert.AreEqual("User not found!", result);
        }

        [TestMethod]
        public async Task ReturnBook_ShouldSucceed()
        {
            await _service.InitializeBooksAsync();
            await _service.BorrowBook(1, 1);

            var result = await _service.ReturnBook(1, 0);

            Assert.AreEqual("Book returned successfully!", result);
            Assert.IsTrue(_service.Books.Any(b => b.Id == 1));
            Assert.AreEqual(0, _service.GetBorrowedBooks(1).Count);
        }

        [TestMethod]
        public async Task ReturnBook_ShouldFailForInvalidIndex()
        {
            await _service.InitializeBooksAsync();
            await _service.BorrowBook(1, 1);

            var result = await _service.ReturnBook(1, 5);

            Assert.AreEqual("Invalid selection!", result);
        }

        [TestMethod]
        public async Task ReturnBook_ShouldFailForInvalidUser()
        {
            var result = await _service.ReturnBook(999, 0);

            Assert.AreEqual("Invalid selection!", result);
        }

        [TestMethod]
        public async Task GetBorrowedBooks_ShouldReturnBooks()
        {
            await _service.InitializeBooksAsync();
            await _service.BorrowBook(1, 1);

            var borrowed = _service.GetBorrowedBooks(1);

            Assert.IsNotNull(borrowed);
            Assert.AreEqual(1, borrowed.Count);
            Assert.AreEqual(1, borrowed[0].Id);
        }

        [TestMethod]
        public async Task GetBorrowedBooks_ShouldReturnEmptyListIfUserHasNone()
        {
            await _service.InitializeBooksAsync();

            var borrowed = _service.GetBorrowedBooks(2);

            Assert.IsNotNull(borrowed);
            Assert.AreEqual(0, borrowed.Count);
        }

        [TestMethod]
        public async Task EditUser_ShouldUpdateUser()
        {
            await _service.InitializeBooksAsync();
            await _service.EditUser(1, "New Name", "new@email.com");

            var user = _service.Users.FirstOrDefault(u => u.Id == 1);

            Assert.IsNotNull(user);
            Assert.AreEqual("New Name", user.Name);
            Assert.AreEqual("new@email.com", user.Email);
        }

        [TestMethod]
        public async Task EditUser_ShouldDoNothingForInvalidUser()
        {
            await _service.InitializeBooksAsync();
            await _service.EditUser(999, "Ghost", "ghost@email.com");

            var user = _service.Users.FirstOrDefault(u => u.Id == 999);
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task InitializeBooksAsync_ShouldLoadBooksUsersAndBorrowed()
        {
            // Arrange
            await _service.AddBook(new Book { Id = 99, Title = "Temp", Author = "Author", ISBN = "999" });
            await _service.AddUser(new User { Id = 42, Name = "Test User", Email = "test@user.com" });
            await _service.BorrowBook(99, 42);

            // Act
            var newService = new LibraryService();
            await newService.InitializeBooksAsync();

            // Assert
            Assert.IsTrue(newService.Users.Any(u => u.Id == 42));
            Assert.IsTrue(newService.GetBorrowedBooks(42).Any(b => b.Id == 99));
        }

    }
}

