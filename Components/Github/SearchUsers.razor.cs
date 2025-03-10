using BlazorWasmPortfolioGhAction.Store.Services;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.ObjectModel;

namespace BlazorWasmPortfolioGhAction.Components.Github
{
    public partial class SearchUsers
    {
        private string AccessToken { get; set; } = "";
        private string SearchText { get; set; } = "";
        private readonly List<UserViewModel> _usersTotal = new();
        private ObservableCollection<UserViewModel> _users = new();
        private int _totalPageCount = 1;
        private int _currentPage = 1;
        private const int PageSize = 12;

        private async Task OnKeyDownAsync(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
            {
                await StartSearchAsync();
            }
        }

        private async Task StartSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText) || string.IsNullOrWhiteSpace(AccessToken)) return;

            var result = await SearchUsersService.SearchUsersAsync(SearchText, AccessToken);
            if (!result.IsSuccess)
            {
                return;
            }

            _totalPageCount = (int)Math.Ceiling(result.Value!.Count / (double)PageSize);
            _usersTotal.Clear();
            _usersTotal.AddRange(result.Value!);
            UpdateUsersPage(1);
        }

        private void UpdateUsersPage(int page)
        {
            if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));

            if (_usersTotal.Count < PageSize)
            {
                _currentPage = 1;
                _users = new ObservableCollection<UserViewModel>(_usersTotal);

                return;
            }

            _users = new ObservableCollection<UserViewModel>(_usersTotal.Skip((page - 1) * PageSize).Take(PageSize));
        }
    }
}
