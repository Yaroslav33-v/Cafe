namespace CafeWeb.Models
{
    public class Cart
    {
        //Корзина заказа
        public string PozitionName { get; set; } = null!;
        public int PozitionCount { get; set; }
        public decimal PozitionPrice { get; set; }

    }



    // Модель отдельной позиции в корзине
    public class CartItem
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }

            public CartItem(string name, decimal price, int quantity)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Название позиции не может быть пустым", nameof(name));

                if (price < 0)
                    throw new ArgumentOutOfRangeException(nameof(price), "Цена не может быть отрицательной");

                if (quantity <= 0)
                    throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть положительным");

                Name = name;
                Price = price;
                Quantity = quantity;
            }

            // Метод для расчёта стоимости данной позиции
            public decimal GetTotalPrice()
            {
                return Price * Quantity;
            }
        }

        // Модель корзины заказов
        public class ShoppingCart
        {
            private readonly List<CartItem> _items = new List<CartItem>();

            // Добавление позиции в корзину
            public void AddItem(string name, decimal price, int quantity)
            {
                var item = new CartItem(name, price, quantity);
                _items.Add(item);
            }

            // Добавление уже созданного объекта CartItem
            public void AddItem(CartItem item)
            {
                _items.Add(item);
            }

            // Удаление позиции по названию
            public bool RemoveItem(string name)
            {
                var item = _items.Find(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    _items.Remove(item);
                    return true;
                }
                return false;
            }

            // Получение всех позиций
            public IReadOnlyCollection<CartItem> GetItems()
            {
                return _items.AsReadOnly();
            }

            // Расчёт общей стоимости корзины
            public decimal GetTotalCartPrice()
            {
                decimal total = 0;
                foreach (var item in _items)
                {
                    total += item.GetTotalPrice();
                }
                return total;
            }

            // Получение количества позиций в корзине
            public int GetItemCount()
            {
                return _items.Count;
            }

            // Проверка, пуста ли корзина
            public bool IsEmpty()
            {
                return _items.Count == 0;
            }
        }

}
