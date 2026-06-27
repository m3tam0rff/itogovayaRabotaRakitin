using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeApp.Models;

namespace TradeApp.Services
{
    public static class DataImporter
    {
        public static void ImportAll(string folderPath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Chebupeli");

            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            if (db.Products.Any())
                return;

            ImportProducts(Path.Combine(folderPath, "Tovar.xlsx"), db);
            ImportUsers(Path.Combine(folderPath, "user_import.xlsx"), db);
            ImportOrders(Path.Combine(folderPath, "Заказ_import.xlsx"), db);
        }
        private static void ImportProducts(string file, AppDbContext db)
        {
            if (!File.Exists(file)) return;
            using var p = new ExcelPackage(new FileInfo(file));
            var sheet = p.Workbook.Worksheets[0];
            int row = 2;
            while (!string.IsNullOrEmpty(sheet.Cells[row, 1].Text))
            {
                var prod = new Product
                {
                    Id = sheet.Cells[row, 1].Text.Trim(),
                    Name = sheet.Cells[row, 2].Text.Trim(),
                    Price = decimal.TryParse(sheet.Cells[row, 4].Text, out var pr) ? pr : 0,
                    Author = sheet.Cells[row, 5].Text.Trim(),
                    ManufacturerId = GetOrCreateManufacturer(sheet.Cells[row, 6].Text.Trim(), db),
                    Category = sheet.Cells[row, 7].Text.Trim(),
                    Discount = int.TryParse(sheet.Cells[row, 8].Text, out var d) ? d : 0,
                    InStock = int.TryParse(sheet.Cells[row, 9].Text, out var s) ? s : 0,
                    Description = sheet.Cells[row, 10].Text.Trim()
                };
                db.Products.Add(prod);
                row++;
            }
            db.SaveChanges();
        }

        private static int GetOrCreateManufacturer(string name, AppDbContext db)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Неизвестный";
            var m = db.Manufacturers.FirstOrDefault(x => x.Name == name);
            if (m != null) return m.Id;
            var newM = new Manufacturer { Name = name };
            db.Manufacturers.Add(newM);
            db.SaveChanges();
            return newM.Id;
        }

        private static void ImportUsers(string file, AppDbContext db)
        {
            if (!File.Exists(file)) return;
            using var p = new ExcelPackage(new FileInfo(file));
            var sheet = p.Workbook.Worksheets[0];
            int row = 2;
            while (!string.IsNullOrEmpty(sheet.Cells[row, 1].Text))
            {
                var roleName = sheet.Cells[row, 1].Text.Trim();
                var role = db.Roles.FirstOrDefault(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName };
                    db.Roles.Add(role);
                    db.SaveChanges();
                }
                var user = new User
                {
                    FIO = sheet.Cells[row, 2].Text.Trim(),
                    Login = sheet.Cells[row, 3].Text.Trim(),
                    Password = sheet.Cells[row, 4].Text.Trim(),
                    RoleId = role.Id
                };
                db.Users.Add(user);
                row++;
            }
            db.SaveChanges();
        }

        private static void ImportOrders(string file, AppDbContext db)
        {
            if (!File.Exists(file)) return;
            using var p = new ExcelPackage(new FileInfo(file));
            var sheet = p.Workbook.Worksheets[0];
            int row = 2;
            while (!string.IsNullOrEmpty(sheet.Cells[row, 1].Text))
            {
                var order = new Order
                {
                    OrderDate = DateTime.TryParse(sheet.Cells[row, 3].Text, out var od) ? od : DateTime.Now,
                    DeliveryDate = DateTime.TryParse(sheet.Cells[row, 4].Text, out var dd) ? dd : (DateTime?)null,
                    Status = sheet.Cells[row, 7].Text.Trim(),
                    PickupCode = sheet.Cells[row, 6].Text.Trim()
                };
                var fio = sheet.Cells[row, 5].Text.Trim();
                if (!string.IsNullOrEmpty(fio))
                {
                    var user = db.Users.FirstOrDefault(u => u.FIO == fio);
                    if (user != null) order.UserId = user.Id;
                }
                db.Orders.Add(order);
                db.SaveChanges();

                var parts = sheet.Cells[row, 2].Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i += 2)
                {
                    var prodId = parts[i].Trim();
                    var qty = int.TryParse(parts[i + 1].Trim(), out var q) ? q : 1;
                    var product = db.Products.FirstOrDefault(p => p.Id == prodId);
                    if (product != null)
                    {
                        db.OrderProducts.Add(new OrderProduct
                        {
                            OrderId = order.Id,
                            ProductId = prodId,
                            Quantity = qty
                        });
                    }
                }
                db.SaveChanges();
                row++;
            }
        }
    }
}