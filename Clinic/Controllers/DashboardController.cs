using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

public class DashboardController : Controller
{
    private SQLiteDatabaseHelper dbHelper;

    public DashboardController()
    {
        dbHelper = new SQLiteDatabaseHelper();
    }
    public IActionResult Index()
    {
        dbHelper.OpenConnection();
        var activated = false;

        string query = "SELECT UserName, Activated FROM AspNetUsers WHERE UserName = @UserId";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            command.Parameters.AddWithValue("@UserId", User.Identity.Name);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // Retrieve the data from the database
                    var username = reader.GetString(0);
                    activated = reader.GetBoolean(1);

                }
            }
        }

        // Pass isActivated to the view
        ViewBag.IsActivated = activated;

        return View();
    }
}
