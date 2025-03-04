// sqlite.razor.cs
using BlazorWasmPortfolioGhAction.Data;
using BlazorWasmPortfolioGhAction.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;

namespace BlazorWasmPortfolioGhAction.Pages
{
    public partial class Sqlite
    {
        public const string SqliteDbFilename = "DemoData.db";
        private string _version = "unknown";
        private string _alertMessage = "";
        private string _alertType = "success";
        private bool _showAlert = false;
        private readonly List<Car> _cars = new();

        [Inject] private IJSRuntime _js { get; set; }
        [Inject] private IDbContextFactory<ClientSideDbContext> _dbContextFactory { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("browser")))
            {
                var module = await _js.InvokeAsync<IJSObjectReference>("import", "./sqlite/dbstorage.js");
                await module.InvokeVoidAsync("synchronizeFileWithIndexedDb", SqliteDbFilename);
            }

            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Database.EnsureCreatedAsync();

            if (!db.Cars.Any())
            {
                var cars = new[]
                {
                    new Car { Brand = "Audi", Price = 21000 },
                    new Car { Brand = "Volvo", Price = 11000 },
                    new Car { Brand = "Range Rover", Price = 135000 },
                    new Car { Brand = "Ford", Price = 8995 }
                };
                await db.Cars.AddRangeAsync(cars);
            }
            await Update(db);
            await base.OnInitializedAsync();
        }

        private async Task SQLiteVersion()
        {
            await using var db = new SqliteConnection($"Data Source={SqliteDbFilename}");
            await db.OpenAsync();
            await using var cmd = new SqliteCommand("SELECT SQLITE_VERSION()", db);
            _version = (await cmd.ExecuteScalarAsync())?.ToString();
        }

        private async Task Create(Car upCar)
        {
            try
            {
                var db = await _dbContextFactory.CreateDbContextAsync();
                await db.Cars.AddAsync(upCar);
                await Update(db);
                ShowAlert("Car added successfully!", "success");
            }
            catch
            {
                ShowAlert("Error adding car!", "danger");
            }
        }

        private async Task Update(Car upCar)
        {
            try
            {
                var db = await _dbContextFactory.CreateDbContextAsync();
                var car = await db.Cars.FindAsync(upCar.Id);
                if (car != null)
                {
                    car.Brand = upCar.Brand;
                    car.Price = upCar.Price;
                    db.Cars.Update(car);
                    await Update(db);
                    ShowAlert("Car updated successfully!", "success");
                }
            }
            catch
            {
                ShowAlert("Error updating car!", "danger");
            }
        }

        private async Task Delete(int id)
        {
            try
            {
                var db = await _dbContextFactory.CreateDbContextAsync();
                var car = await db.Cars.FindAsync(id);
                if (car != null)
                {
                    db.Cars.Remove(car);
                    await Update(db);
                    ShowAlert("Car deleted successfully!", "warning");
                }
            }
            catch
            {
                ShowAlert("Error deleting car!", "danger");
            }
        }

        private async Task Update(ClientSideDbContext db)
        {
            await db.SaveChangesAsync();
            _cars.Clear();
            _cars.AddRange(db.Cars);
            StateHasChanged();
        }

        private void ShowAlert(string message, string type)
        {
            _alertMessage = message;
            _alertType = type;
            _showAlert = true;
        }
    }
}
