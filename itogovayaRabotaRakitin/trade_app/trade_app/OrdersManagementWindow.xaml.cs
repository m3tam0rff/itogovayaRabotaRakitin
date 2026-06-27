using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TradeApp.Models;

namespace TradeApp
{
    public partial class OrdersManagementWindow : Window
    {
        private List<OrderViewModel> _orders = new();

        public OrdersManagementWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            using var db = new AppDbContext();
            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .ToList();

            _orders = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                CustomerName = o.User != null ? o.User.FIO : "Гость",
                TotalSum = o.OrderProducts.Sum(op => op.Quantity * op.Product.GetPriceWithDiscount())
            }).ToList();

            DgOrders.ItemsSource = _orders;
        }

        private void TbSearchOrderId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbSearchOrderId.Text))
                DgOrders.ItemsSource = _orders;
            else if (int.TryParse(TbSearchOrderId.Text, out int id))
                DgOrders.ItemsSource = _orders.Where(o => o.Id == id).ToList();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            TbSearchOrderId.Text = "";
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new AppDbContext();
                var dbOrders = db.Orders.ToList();
                foreach (var vm in _orders)
                {
                    var order = dbOrders.FirstOrDefault(o => o.Id == vm.Id);
                    if (order != null)
                    {
                        order.DeliveryDate = vm.DeliveryDate;
                        order.Status = vm.Status;
                    }
                }
                db.SaveChanges();
                MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Status { get; set; } = "Новый";
        public string? CustomerName { get; set; }
        public decimal TotalSum { get; set; }
        public List<string> StatusList => new() { "Новый", "В обработке", "Завершен" };
    }
}
