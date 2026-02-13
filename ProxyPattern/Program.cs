using System;
using System.Collections.Generic;
using System.Threading;

namespace ProxyPattern
{
    
    public interface IDatabase
    {
        string GetData(string key);
        void SaveData(string key, string value);
    }

    
    public class RealDatabase : IDatabase
    {
        private string _connectionString;
        private Dictionary<string, string> _storage = new Dictionary<string, string>();

        public RealDatabase(string connectionString)
        {
            _connectionString = connectionString;
            Console.WriteLine($"[RealDB] Подключение к БД: {connectionString}");
            Thread.Sleep(2000); 
            Console.WriteLine("[RealDB] Подключение установлено");
        }

        public string GetData(string key)
        {
            Console.WriteLine($"[RealDB] Выполнение запроса: SELECT * FROM data WHERE key='{key}'");
            Thread.Sleep(1000); 

            if (_storage.ContainsKey(key))
                return _storage[key];
            else
                return null;
        }

        public void SaveData(string key, string value)
        {
            Console.WriteLine($"[RealDB] Выполнение запроса: INSERT INTO data (key, value) VALUES ('{key}', '{value}')");
            Thread.Sleep(1000); 
            _storage[key] = value;
        }
    }

    
    public class DatabaseProxy : IDatabase
    {
        private RealDatabase _realDatabase;
        private string _connectionString;
        private Dictionary<string, string> _cache = new Dictionary<string, string>();
        private bool _isConnected = false;

        public DatabaseProxy(string connectionString)
        {
            _connectionString = connectionString;
            Console.WriteLine("[Proxy] Создан прокси для БД");
        }

        private void Connect()
        {
            if (!_isConnected)
            {
                _realDatabase = new RealDatabase(_connectionString);
                _isConnected = true;
            }
        }

        public string GetData(string key)
        {
            Console.WriteLine($"[Proxy] Запрос данных для ключа '{key}'");

            
            if (key.Contains("admin") || key.Contains("secret"))
            {
                Console.WriteLine("[Proxy] ДОСТУП ЗАПРЕЩЕН: нет прав на чтение секретных данных");
                return "ACCESS_DENIED";
            }

            
            if (_cache.ContainsKey(key))
            {
                Console.WriteLine($"[Proxy] Данные из кеша для '{key}': {_cache[key]}");
                return _cache[key];
            }

            
            Connect();

            
            string data = _realDatabase.GetData(key);

            
            if (data != null)
            {
                _cache[key] = data;
                Console.WriteLine($"[Proxy] Данные закешированы для '{key}'");
            }

            return data;
        }

        public void SaveData(string key, string value)
        {
            Console.WriteLine($"[Proxy] Запрос на сохранение '{key}'='{value}'");

            
            if (key.Contains("admin") || key.Contains("secret"))
            {
                Console.WriteLine("[Proxy] ДОСТУП ЗАПРЕЩЕН: нет прав на запись секретных данных");
                return;
            }

            
            Connect();

            
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
                Console.WriteLine($"[Proxy] Кеш для '{key}' инвалидирован");
            }

            
            _realDatabase.SaveData(key, value);

            
            _cache[key] = value;
            Console.WriteLine($"[Proxy] Данные закешированы для '{key}'");
        }

        
        public void ShowCacheStats()
        {
            Console.WriteLine($"[Proxy] Статистика кеша: {_cache.Count} записей");
        }
    }

    
    public interface IImage
    {
        void Display();
        string GetInfo();
    }

    public class HighResImage : IImage
    {
        private string _filename;

        public HighResImage(string filename)
        {
            _filename = filename;
            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            Console.WriteLine($"[Image] Загрузка {_filename} с диска...");
            Thread.Sleep(1500); 
            Console.WriteLine($"[Image] {_filename} загружено");
        }

        public void Display()
        {
            Console.WriteLine($"[Image] Отображение {_filename}");
        }

        public string GetInfo() => $"{_filename} (High Resolution)";
    }

    public class ImageProxy : IImage
    {
        private string _filename;
        private HighResImage _realImage;

        public ImageProxy(string filename)
        {
            _filename = filename;
            Console.WriteLine($"[Proxy] Создан прокси для {filename}");
        }

        public void Display()
        {
            
            if (_realImage == null)
            {
                Console.WriteLine("[Proxy] Первый запрос отображения - инициализация реального изображения");
                _realImage = new HighResImage(_filename);
            }

            _realImage.Display();
        }

        public string GetInfo()
        {
            
            return $"{_filename} (Proxy, не загружено)";
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("ПАТТЕРН ПРОКСИ");

            
            Console.WriteLine("1. Прокси с кешированием и ленивой инициализацией:");

            IDatabase db = new DatabaseProxy("server=localhost;db=test");

            
            Console.WriteLine("Первый запрос (инициализация БД)");
            var result1 = db.GetData("users");
            Console.WriteLine($"Результат: {result1 ?? "NULL"}");

            
            Console.WriteLine("Сохранение данных");
            db.SaveData("users", "Ivan, Petr, Maria");
            Console.WriteLine();

            
            Console.WriteLine("Повторный запрос (кеш)");
            var result2 = db.GetData("users");
            Console.WriteLine($"Результат: {result2}");

            
            Console.WriteLine("Попытка доступа к секретным данным ");
            var secretData = db.GetData("admin_password");
            Console.WriteLine($"Результат: {secretData}");

            
            if (db is DatabaseProxy proxy)
                proxy.ShowCacheStats();

            Console.WriteLine("2. Прокси с ленивой загрузкой изображений:");

            
            IImage img1 = new ImageProxy("family_photo.jpg");
            IImage img2 = new ImageProxy("vacation.png");
            IImage img3 = new ImageProxy("avatar.jpg");

            
            Console.WriteLine("Информация об изображениях (без загрузки):");
            Console.WriteLine($"  {img1.GetInfo()}");
            Console.WriteLine($"  {img2.GetInfo()}");
            Console.WriteLine($"  {img3.GetInfo()}");

            
            Console.WriteLine("Отображение первого изображения:");
            img1.Display();

            Console.WriteLine("Отображение первого изображения повторно (уже загружено):");
            img1.Display(); 

            Console.WriteLine("Отображение второго изображения (загружается сейчас):");
            img2.Display();

            Console.WriteLine();
            Console.ReadKey();
        }
    }
}