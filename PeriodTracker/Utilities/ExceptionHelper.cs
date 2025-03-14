using System.Text;

namespace PeriodTracker;

public static class ExceptionHelper
{
    public static string GetMessages(Exception ex)
    {
        var sb = new StringBuilder();
        while(ex is not null)
        {
            sb.Append(ex.Message);
            sb.Append(' ');

            ex = ex.InnerException!;
        }

        return sb.ToString();
    }
}