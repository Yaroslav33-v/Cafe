namespace CafeWeb.Static
{
    public static class OrderNumberGenerator
    {
        private static string _currentPrefix = "AA";
        private static int _currentNumber = 1;
        private static readonly object _lock = new();

        public static void InitializeFromLastOrderNumber(string lastOrderNumber)
        {
            if (string.IsNullOrEmpty(lastOrderNumber))
                return;

            if (lastOrderNumber.Length >= 5)
            {
                string prefix = lastOrderNumber.Substring(0, 2);
                string numberPart = lastOrderNumber.Substring(2);

                if (int.TryParse(numberPart, out int number))
                {
                    _currentPrefix = prefix;
                    _currentNumber = number;
                }
            }
        }

        public static string GenerateOrderNumber()
        { 
            lock (_lock)
            {
                _currentNumber++;

                // Если номер превысил 999, переходим к следующему префиксу
                if (_currentNumber > 999)
                {
                    _currentNumber = 1;
                    _currentPrefix = GetNextPrefix(_currentPrefix);
                }

                return $"{_currentPrefix}{_currentNumber:D3}";
            }
        }

        private static string GetNextPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix) || prefix.Length != 2)
                return "AA";

            char first = prefix[0];
            char second = prefix[1];

            // Увеличиваем вторую букву
            if (second < 'Z')
                second++;
            else
            {
                // Если вторая буква Z, сбрасываем её на A и увеличиваем первую
                second = 'A';
                if (first < 'Z')
                    first++;
                else
                    first = 'A';
            }

            return $"{first}{second}";
        }
    }
}