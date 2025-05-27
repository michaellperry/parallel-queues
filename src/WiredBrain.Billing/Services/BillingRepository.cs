using System.Text.Json;

namespace WiredBrain.Billing.Services;

public class BillingRepository
{
    private readonly string _filePath;
    private readonly object _lock = new object();
    private decimal _totalAmount;
    private int _transactionCount;
    private DateTime _lastUpdated;
    private readonly ILogger<BillingRepository> _logger;

    public BillingRepository(IConfiguration configuration, ILogger<BillingRepository> logger)
    {
        _logger = logger;
        // Get file path from configuration
        _filePath = configuration["Billing:StorageFilePath"] ?? "/data/billing.json";
        LoadFromFile();
    }

    public void AddCharge(decimal amount)
    {
        lock (_lock)
        {
            _totalAmount += amount;
            _transactionCount++;
            _lastUpdated = DateTime.UtcNow;
            SaveToFile();
        }
    }

    public (decimal TotalAmount, int TransactionCount, DateTime LastUpdated) GetTotals()
    {
        lock (_lock)
        {
            return (_totalAmount, _transactionCount, _lastUpdated);
        }
    }

    private void LoadFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonSerializer.Deserialize<BillingData>(json);
                if (data != null)
                {
                    _totalAmount = data.TotalAmount;
                    _transactionCount = data.TransactionCount;
                    _lastUpdated = data.LastUpdated;
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but continue with default values
            _logger.LogError(ex, "Error loading billing data from {FilePath}", _filePath);
        }
    }

    private void SaveToFile()
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = new BillingData
            {
                TotalAmount = _totalAmount,
                TransactionCount = _transactionCount,
                LastUpdated = _lastUpdated
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            // Log error but continue
            _logger.LogError(ex, "Error saving billing data to {FilePath}", _filePath);
        }
    }

    private class BillingData
    {
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}