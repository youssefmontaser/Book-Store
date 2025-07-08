using System.Collections.Concurrent;
using Book_Store;

namespace Book_Store
{
    public static class ISBNGenerator
    {
        private static readonly string CounterFile = "isbn_counter.txt";
        private static readonly ConcurrentDictionary<string, int> Counters = new();

        static ISBNGenerator()
        {
            LoadCounters();
        }

        private static void LoadCounters()
        {
            if (!File.Exists(CounterFile)) return;

            foreach (var line in File.ReadAllLines(CounterFile))
            {
                var parts = line.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[1], out var count))
                    Counters[parts[0]] = count;
            }
        }

        private static void SaveCounters()
        {
            var lines = new List<string>();
            foreach (var pair in Counters)
                lines.Add($"{pair.Key}:{pair.Value}");

            File.WriteAllLines(CounterFile, lines);
        }

        public static string GenerateISBN(string prefix)
        {
            var count = Counters.AddOrUpdate(prefix, 1000, (_, oldValue) => oldValue + 1);
            SaveCounters();
            return $"{prefix}-{count}";
        }
    }
    public abstract class Book
    {
        public string ISBN { get; private set; }
        public string Title { get; set; }
        public DateTime PublishedYear { get; set; }
        public decimal Price { get; set; }
        public string Author { get; set; }
        public Book(string title, DateTime publishedYear, decimal price, string author,string prefix)
        {
            ISBN = ISBNGenerator.GenerateISBN(prefix);
            Title = title;
            PublishedYear = publishedYear;
            Price = price;
            Author = author;
        }

        public abstract void Buy(string email, string address, int quantity);
        public abstract bool IsAvailable(int quantity);
    }
    public class PaperBook:Book
    {
        int Stock {  get; set; }
        bool Shippable {  get; set; }
        public PaperBook(string title, DateTime publishedYear, decimal price, string author, int stock, bool shippable) : base( title, publishedYear, price, author, "PB")
        {
           Stock = stock;
            Shippable = shippable;
        }
        public override bool IsAvailable(int quantity) => Stock >= quantity;
        public override void Buy(string email, string address, int quantity)
        {
            if (!IsAvailable(quantity))
                throw new Exception($"Only {quantity} of {Title} is available.");
            Stock -= quantity;
            Console.WriteLine($"Quantum book store: Paper book '{Title}' purchased for {Price * quantity:C}");
            if (Shippable)
            {
                Console.WriteLine($"Quantum book store: Shipping to {address}");
            }
            else
            {
                Console.WriteLine($"Quantum book store: Wait You Mr {email} to Take Your Order in The Time of Work Thanks for your Time");
            }
        }
    }
    public class EBook : Book
    {
        public string FileType { get; set; }

        public EBook(string title, DateTime publishedYear, decimal price, string author, string fileType)
            : base(title, publishedYear, price, author, "EB")
        {
            FileType = fileType;
        }

        public override bool IsAvailable(int quantity) => quantity == 1;

        public override void Buy(string email, string address, int quantity)
        {
            if (!IsAvailable(quantity))
                throw new Exception("Quantum book store: Only one ebook may be purchased per transaction.");

            Console.WriteLine($"Quantum book store: EBook '{Title}' purchased for {Price:C}");
            Console.WriteLine($"Quantum book store: Sending {FileType} file to {email}...");
        }
    }
    public class DemoBook : Book
    {
        public DemoBook(string title, DateTime publishedYear, string author)
            : base(title, publishedYear, 0, author,"DB")
        { }

        public override bool IsAvailable(int quantity) => false;

        public override void Buy(string email, string address, int quantity)
        {
            throw new InvalidOperationException("Quantum book store: Demo books are not for sale.");
        }
    }
    public class BookManagement
    {
        private readonly Dictionary<string, Book> books = new();

        public void AddBook(Book book)
        {
            books[book.ISBN] = book;
            Console.WriteLine($"Quantum book store: Added '{book.Title}' to inventory.");
        }

        public void RemoveOutdatedBooks(DateTime YearOdOldBooks)
        {
            var outdated = books.Values
                .Where(book =>  book.PublishedYear < YearOdOldBooks)
                .Select(book => book.ISBN)
                .ToList();

            foreach (var isbn in outdated)
            {
                books.Remove(isbn);
                Console.WriteLine($"Quantum book store: Removed outdated book with ISBN {isbn}.");
            }
        }
        public decimal BuyBook(string isbn, int quantity, string email, string address)
        {
            if (!books.ContainsKey(isbn))
                throw new KeyNotFoundException("Quantum book store: Book not found.");

            var book = books[isbn];

            book.Buy(email, address, quantity);
            return book.Price * quantity;
        }
    }
}
internal class Program
{
    static void Main(string[] args)
    {
        var inventory = new BookManagement();

        var paper = new PaperBook("The art of indifference", new DateTime(2018), 400.00m, "Einstein", 10,true);
        var ebook1 = new EBook(" The art of reading minds", new DateTime(2021), 224.00m, "Turing", "PDF");
        var ebook2 = new EBook("Your psychological complexes are your eternal prison", new DateTime(2021), 180.00m, "Turing", "PDF");
        var demo = new DemoBook("Quantum Showcase", new DateTime(2010), "Bohr");

        inventory.AddBook(paper);
        inventory.AddBook(ebook1);
        inventory.AddBook(ebook2);
        inventory.AddBook(demo);

        try
        {
            var amount = inventory.BuyBook("PB001", 2, "youssefmontaser@gmail.com", "Mania");
            Console.WriteLine($"Quantum book store: Amount paid: {amount:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        try
        {
            var amount = inventory.BuyBook("EB001", 1, "Ahmed@gmail.com", "");
            Console.WriteLine($"Quantum book store: Amount paid: {amount:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            inventory.BuyBook("DB001", 1, "Abdo@gmail.com", "Port Said");
        }
        catch (Exception ex)
        {
            Console.WriteLine( ex.Message);
        }

        inventory.RemoveOutdatedBooks(new DateTime(2010));
    }
}
