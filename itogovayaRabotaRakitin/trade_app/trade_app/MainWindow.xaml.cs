using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TradeApp.Models;

namespace TradeApp
{
	/// <summary>
	/// Главное окно приложения, реализующее интерфейс терминала самообслуживания.
	/// </summary>
	public partial class MainWindow : Window
    {
		private List<Product> _allProducts = [];
		private int _totalCount = 0;

		public static Dictionary<string, int> CurrentCart { get; private set; } = new();
		public static User? CurrentUser { get; private set; }

		/// <summary>
		/// Инициализирует компоненты окна и запускает первичную загрузку данных.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			LoadDataFromDatabase();
		}

		/// <summary>
		/// Выполняет чтение списков книг и производителей из локальной базы данных SQLite.
		/// </summary>
		private void LoadDataFromDatabase()
		{
			try
			{
				using var db = new AppDbContext();

				_allProducts = [.. db.Products.Include(p => p.Manufacturer)];
				_totalCount = _allProducts.Count;

				var manufacturersList = db.Manufacturers.OrderBy(m => m.Name).ToList();
				manufacturersList.Insert(0, new Manufacturer { Id = 0, Name = "Все производители" });

				CbManufacturers.ItemsSource = manufacturersList;
				CbManufacturers.SelectedIndex = 0;
				CbSorting.SelectedIndex = 0;

				ApplyRealTimeFilters();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка инициализации данных из БД: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Производит динамическую фильтрацию и сортировку списка книг в реальном времени.
		/// </summary>
		private void ApplyRealTimeFilters()
		{
			if (_allProducts == null) return;

			var filteredData = _allProducts.AsEnumerable();

			string searchText = TbSearch.Text.Trim().ToLower();
			if (!string.IsNullOrEmpty(searchText))
			{
				filteredData = filteredData.Where(p => p.Name.ToLower().Contains(searchText));
			}

			if (CbManufacturers.SelectedItem is Manufacturer selectedMfg && selectedMfg.Id > 0)
			{
				filteredData = filteredData.Where(p => p.ManufacturerId == selectedMfg.Id);
			}

			if (decimal.TryParse(TbMinPrice.Text.Trim(), out decimal minPrice))
			{
				filteredData = filteredData.Where(p => p.Price >= minPrice);
			}
			if (decimal.TryParse(TbMaxPrice.Text.Trim(), out decimal maxPrice))
			{
				filteredData = filteredData.Where(p => p.Price <= maxPrice);
			}

			if (CbSorting.SelectedIndex == 1) filteredData = filteredData.OrderBy(p => p.Price);
			else if (CbSorting.SelectedIndex == 2) filteredData = filteredData.OrderByDescending(p => p.Price);
			else if (CbSorting.SelectedIndex == 3) filteredData = filteredData.OrderBy(p => p.Name);
			else if (CbSorting.SelectedIndex == 4) filteredData = filteredData.OrderByDescending(p => p.Name);

			var result = filteredData.ToList();
			LvProducts.ItemsSource = result;

			TxtCountInfo.Text = $"{result.Count} из {_totalCount}";
		}

		private void FilterChanged(object sender, TextChangedEventArgs e) => ApplyRealTimeFilters();
		private void FilterChanged(object sender, SelectionChangedEventArgs e) => ApplyRealTimeFilters();

		private void BtnOrder_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button orderButton && orderButton.Tag != null)
			{
				string id = orderButton.Tag.ToString()!;

				if (CurrentCart.TryGetValue(id, out int value))
					CurrentCart[id] = ++value;
				else
					CurrentCart[id] = 1;

				BtnViewOrder.Visibility = Visibility.Visible;

				MessageBox.Show($"Книга с артикулом [{id}] успешно добавлена в текущий заказ.", "Корзина терминала", MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

		private void BtnAuthAction_Click(object sender, RoutedEventArgs e)
		{
			if (BtnAuthAction.Content.ToString() == "Выйти")
			{
				TxtUserFIO.Text = "Гость";
				BtnAuthAction.Content = "Войти";
				MessageBox.Show("Вы успешно вышли из системы.", "Выход", MessageBoxButton.OK, MessageBoxImage.Information);
				return;
			}

			LoginWindow loginWin = new() { Owner = this };
			if (loginWin.ShowDialog() == true && loginWin.AuthenticatedUser != null)
			{
				CurrentUser = loginWin.AuthenticatedUser;

				TxtUserFIO.Text = loginWin.AuthenticatedUser.FIO;
				BtnAuthAction.Content = "Выйти";
			}
		}

        private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
        {
			OrderWindow orderWin = new() { Owner = this };
			if (orderWin.ShowDialog() == true)
			{
				BtnViewOrder.Visibility = Visibility.Collapsed;
			}
		}
    }
}