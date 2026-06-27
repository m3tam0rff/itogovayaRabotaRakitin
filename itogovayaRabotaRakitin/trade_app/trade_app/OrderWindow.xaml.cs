using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using TradeApp.Models;

namespace TradeApp
{
    /// <summary>
    /// Окно просмотра корзины и оформления заказа.
    /// </summary>
    public partial class OrderWindow : Window
    {
        public class OrderItemView
        {
            public Product Product { get; set; } = null!;
            public int Quantity { get; set; }
        }

        private List<OrderItemView> _items = [];
        private decimal _totalSum = 0;
        private string _generatedCode = "";

        public OrderWindow()
        {
            InitializeComponent();
            InitOrderData();
        }

        private void InitOrderData()
        {
            if (MainWindow.CurrentUser != null)
                TxtClientInfo.Text = $"Заказчик: {MainWindow.CurrentUser.FIO}";
            else
                TxtClientInfo.Text = "Заказчик: Неавторизованный клиент (Гость)";

            _generatedCode = new Random().Next(100, 1000).ToString();
            TxtPickupCode.Text = $"Код получения: {_generatedCode}";

            RefreshCart();
        }

        private void RefreshCart()
        {
            using var db = new AppDbContext();
            _items.Clear();
            _totalSum = 0;

            foreach (var pair in MainWindow.CurrentCart)
            {
                var product = db.Products.Find(pair.Key);
                if (product != null)
                {
                    _items.Add(new OrderItemView { Product = product, Quantity = pair.Value });
                    _totalSum += product.GetPriceWithDiscount() * pair.Value;
                }
            }

            LvOrderItems.ItemsSource = null;
            LvOrderItems.ItemsSource = _items;
            TxtTotalSum.Text = $"Итоговая сумма: {_totalSum:N2} руб.";
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag != null)
            {
                string id = btn.Tag.ToString()!;
                MainWindow.CurrentCart.Remove(id);
                RefreshCart();
            }
        }

        private void BtnSubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new AppDbContext();

                var newOrder = new Order
                {
                    OrderDate = DateTime.Now,
                    Status = "новый",
                    PickupCode = _generatedCode,
                    UserId = MainWindow.CurrentUser?.Id
                };

                db.Orders.Add(newOrder);
                db.SaveChanges();

                foreach (var item in _items)
                {
                    db.OrderProducts.Add(new OrderProduct
                    {
                        OrderId = newOrder.Id,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity
                    });
                }
                db.SaveChanges();

                MessageBox.Show($"Заказ №{newOrder.Id} успешно зарегистрирован в системе!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                SaveFileDialog sfd = new()
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt",
                    FileName = $"Талон_Заказа_{newOrder.Id}.txt"
                };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new();
                    sb.AppendLine($"ТАЛОН ЗАКАЗА №{newOrder.Id}");
                    sb.AppendLine($"Дата оформления: {newOrder.OrderDate:dd.MM.yyyy HH:mm}");
                    sb.AppendLine($"Статус заказа: {newOrder.Status}");
                    sb.AppendLine($"Код получения в ПВЗ: {newOrder.PickupCode}");
                    sb.AppendLine("Состав покупки:");
                    foreach (var item in _items)
                    {
                        decimal priceWithDiscount = item.Product.GetPriceWithDiscount();
                        sb.AppendLine($"- {item.Product.Name} x{item.Quantity} шт. ({priceWithDiscount * item.Quantity:N2} руб.)");
                    }
                    sb.AppendLine(" ");
                    sb.AppendLine($"ИТОГО К ОПЛАТЕ: {_totalSum:N2} руб.");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Текстовый талон успешно сохранен на диск!", "Экспорт данных", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                MainWindow.CurrentCart.Clear();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}