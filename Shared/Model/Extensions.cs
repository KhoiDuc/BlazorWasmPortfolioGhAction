namespace BlazorWasmPortfolioGhAction.Shared.Model
{
    public static class Extensions
    {
        public static string GetName<T>(this T enumObj) where T : Enum
        {
            return Enum.GetName(typeof(T), enumObj) ?? "";
        }

        public static int GetValue<T>(this T enumObj) where T : Enum
        {
            return Convert.ToInt32(enumObj);
        }

        public static T ToEnum<T>(this string enumName) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), enumName, true);
        }

        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }
    }
}
