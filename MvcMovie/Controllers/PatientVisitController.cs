using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

public class PatientVisitController : Controller
{
    private SQLiteDatabaseHelper dbHelper;
    private readonly UserManager<User> _userManager;

    public PatientVisitController(UserManager<User> userManager)
    {
        dbHelper = new SQLiteDatabaseHelper();
        _userManager = userManager;
    }

    public ActionResult Index()
    {
        dbHelper.OpenConnection();

        string patientId = _userManager.GetUserId(User);
        
        List<Visit> visitData = new List<Visit>();

        string query = "SELECT D.Name || ' ' || D.Surname AS DoctorFullName, P.Name || ' ' || P.Surname AS PatientFullName, D.Specjalization, V.PatientId, V.DoctorId, V.Description," +
    "V.Date FROM Visits V INNER JOIN AspNetUsers D ON V.DoctorId = D.Id LEFT JOIN AspNetUsers P ON V.PatientId = P.Id " +
    "WHERE Date(V.Date) < datetime('now') AND (V.PatientId = @patientId) AND (V.Description IS NOT NULL) ORDER BY V.Date";

        
        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            command.Parameters.AddWithValue("@patientId", patientId);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string doctorName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                    string patientName = !reader.IsDBNull(1) ? reader.GetString(1) : null;
                    string doctorId = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    string description = !reader.IsDBNull(5) ? reader.GetString(5) : null;
                    DateTime visitDateTime = !reader.IsDBNull(6) ? reader.GetDateTime(6) : default(DateTime);
                    string Specjalization = !reader.IsDBNull(2) ? reader.GetString(2) : null;


                    visitData.Add(new Visit()
                    {
                        DoctorName = doctorName,
                        PatientName = patientName,
                        PatientId = patientId,
                        DoctorId = doctorId,
                        Description = description,
                        VisitDateTime = visitDateTime,
                        Specjalization = Specjalization
                    });
                }
            }
        }

        return View(visitData);
    }

    [HttpPost]
    public IActionResult Resign(string doctorId, DateTime visitDateTime)
    {
        dbHelper.OpenConnection();

        string query = "UPDATE Visits SET PatientId = NULL " +
                   "WHERE DoctorId = @doctorId AND Date = @visitDateTime";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection())){

            command.Parameters.AddWithValue("@doctorId", doctorId);
            command.Parameters.AddWithValue("@visitDateTime", visitDateTime.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException ex)
            {
                // Handle exceptions or log errors if needed
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        dbHelper.CloseConnection();

        return RedirectToAction("Index", "Dashboard");
    }

        //show my visits
    public ActionResult MyVisits(){
        string patientId = _userManager.GetUserId(User);

        dbHelper.OpenConnection();

        List<Visit> visitData = new List<Visit>();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date, Specjalization " +
               "FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId " +
               $"WHERE Date > datetime('now') AND PatientId = '{patientId}' " +
               "ORDER BY Date";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection())){

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string Specjalization = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    string doctorName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                    string doctorId = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    DateTime visitDateTime = !reader.IsDBNull(3) ? reader.GetDateTime(3) : default(DateTime);

                    visitData.Add(new Visit()
                    {
                        DoctorName = doctorName,
                        DoctorId = doctorId,
                        VisitDateTime = visitDateTime,
                        Specjalization = Specjalization
                    });
                }
            }
        }

        return View(visitData);
    }

    public ActionResult Schedule()
    {
        return View();
    }

    public IActionResult ListVisits(){
        string Specjalization = Request.Form["selection"];

        dbHelper.OpenConnection();

        List<Visit> visitData = new List<Visit>();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date, Specjalization " +
               "FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId " +
               $"WHERE Date > datetime('now') AND (PatientId IS NULL OR PatientId = '') AND Specjalization = '{Specjalization}' " +
               "ORDER BY Date";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            // command.Parameters.AddWithValue("@specjalization", Specjalization);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string patientId = !reader.IsDBNull(1) ? reader.GetString(1) : null;
                    string doctorName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                    string doctorId = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    DateTime visitDateTime = !reader.IsDBNull(3) ? reader.GetDateTime(3) : default(DateTime);

                    visitData.Add(new Visit()
                    {
                        DoctorName = doctorName,
                        PatientId = patientId,
                        DoctorId = doctorId,
                        VisitDateTime = visitDateTime
                    });
                }
            }
        }
        dbHelper.CloseConnection();

        return View(visitData);
    }

    [HttpPost]
    public IActionResult ReserveVisit(string doctorId, DateTime visitDateTime)      
    {
    // Assuming you have the patient's ID available in your session or from authentication
        string patientId = _userManager.GetUserId(User);

        dbHelper.OpenConnection();

    // Check if the patient has any other visits at the selected time
        string query = "SELECT COUNT(*) FROM Visits " +
                   "WHERE PatientId = @patientId AND Date = @visitDateTime";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            command.Parameters.AddWithValue("@patientId", patientId);
            command.Parameters.AddWithValue("@visitDateTime", visitDateTime);

            object result = command.ExecuteScalar();
            int count = Convert.ToInt32(result);


            if (count == 0)
            {
            // No conflicting visits, proceed to add the patient to this visit
                string insertQuery = "UPDATE Visits SET PatientId = @patientId " +
                                 "WHERE DoctorId = @doctorId AND Date = @visitDateTime";

                using (var insertCommand = new SqliteCommand(insertQuery, dbHelper.GetConnection()))
                {
                    insertCommand.Parameters.AddWithValue("@patientId", patientId);
                    insertCommand.Parameters.AddWithValue("@doctorId", doctorId);
                    insertCommand.Parameters.AddWithValue("@visitDateTime", visitDateTime);

                    insertCommand.ExecuteNonQuery();
                }

                dbHelper.CloseConnection();    
            
            // Redirect to a success page or back to the list of visits
            return RedirectToAction("Index", "Dashboard");

            }
            else
            {
                dbHelper.CloseConnection();
                // Redirect to an error page indicating the conflict
                return RedirectToAction("ErrorPage");
            }
        }
    }
}
