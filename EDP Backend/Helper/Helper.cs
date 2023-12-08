namespace EDP_Backend.Helper
{
    public class Helper
    {
        public static Dictionary<string, string> GenerateError(string error)
        {
            return new Dictionary<string, string>
            {
                {"error", error}
            };
        }
    }
}
