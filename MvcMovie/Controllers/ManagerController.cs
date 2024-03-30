using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using MvcMovie.Models;
public class ManagerController : Controller
{
    private SQLiteDatabaseHelper dbHelper;

    public ManagerController()
    {
        dbHelper = new SQLiteDatabaseHelper();
    }

    public ActionResult Index()
    {

        DateTime today = DateTime.Today;
        DayOfWeek currentDayOfWeek = today.DayOfWeek;

        int daysToMonday = ((int)DayOfWeek.Monday - (int)currentDayOfWeek) % 7;
        DateTime monday = today.AddDays(daysToMonday);
        DateTime tuesday = monday.AddDays(1);
        DateTime wednesday = monday.AddDays(2);
        DateTime thursday = monday.AddDays(3);
        DateTime friday = monday.AddDays(4);

        string formattedMonday = monday.ToString("yyyy-MM-dd");
        string formattedTuesday = tuesday.ToString("yyyy-MM-dd");
        string formattedWednesday = wednesday.ToString("yyyy-MM-dd");
        string formattedThursday = thursday.ToString("yyyy-MM-dd");
        string formattedFriday = friday.ToString("yyyy-MM-dd");

        var visitData = new WeekVisits
        {
            MondayVisits = GetVisitsForDay(formattedMonday),
            TuesdayVisits = GetVisitsForDay(formattedTuesday),
            WednesdayVisits = GetVisitsForDay(formattedWednesday),
            ThursdayVisits = GetVisitsForDay(formattedThursday),
            FridayVisits = GetVisitsForDay(formattedFriday)
        };

        return View(visitData);
    }

    public ActionResult CreateDoctor()
    {
        return View();
    }

    private List<Visit> GetVisitsForDay(string day)
    {   
        List<Visit> visitData = new List<Visit>();

        dbHelper.OpenConnection();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, PatientId, DoctorId, Date " +
               "FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId " +
               $"WHERE Date(Date) = Date('{day:yyyy-MM-dd *:*:*}') " + // Filter by today's date
               "ORDER BY Date";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string doctorName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                    string patientId = !reader.IsDBNull(1) ? reader.GetString(1) : null;
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

        return visitData;
    }

    public IActionResult CopyVisits(){

        Console.WriteLine("Copying visits for next week");  
        dbHelper.OpenConnection();

        DateTime today = DateTime.Today;
        DayOfWeek currentDayOfWeek = today.DayOfWeek;

        int daysToMonday = ((int)DayOfWeek.Monday - (int)currentDayOfWeek + 7) % 7;
        DateTime monday = today.AddDays(daysToMonday).AddDays(-7);
        DateTime friday = monday.AddDays(4);

        string formattedMonday = monday.ToString("yyyy-MM-dd");
        string formattedFriday = friday.ToString("yyyy-MM-dd");

        List<Visit> visitData = new List<Visit>();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, DoctorId, Date FROM AspNetUsers INNER JOIN Visits ON AspNetUsers.Id = DoctorId WHERE Date(Date) BETWEEN '@firstdate' AND '@seconddate' ORDER BY Date";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection())){
            
            command.Parameters.AddWithValue("@firstdate", formattedMonday);
            command.Parameters.AddWithValue("@seconddate", formattedFriday);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string doctorName = reader.GetString(0);
                    string doctorId = reader.GetString(1);
                    DateTime visitDateTime = reader.GetDateTime(2);

                    visitData.Add(new Visit()
                    {
                        DoctorName = doctorName,
                        PatientId = null,
                        DoctorId = doctorId,
                        VisitDateTime = visitDateTime.AddDays(7),
                        Description = null
                    });
                }
            }
        }

        string insertQuery = "INSERT OR IGNORE INTO Visits (DoctorId, Date) VALUES (@DoctorId, @Date)";

        foreach (var visit in visitData)
        {
            Console.WriteLine(visit.VisitDateTime);
            using (var command = new SqliteCommand(insertQuery, dbHelper.GetConnection()))
            {
                command.Parameters.AddWithValue("@DoctorId", visit.DoctorId);
                command.Parameters.AddWithValue("@Date", visit.VisitDateTime);

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
        }
        return RedirectToAction("Index");
    }

    public ActionResult ActivateAccount(string userId){

        dbHelper.OpenConnection();

        string query = "UPDATE AspNetUsers SET Activated = 1 WHERE Id = @userId;";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection())){
            command.Parameters.AddWithValue("@userId", userId);

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
        // Response.Headers.Add("Refresh", "5; url=/Manager/ListPatients");
        return Json(new { success = true });
    }

    public ActionResult Create()    //do i need this?
    {
        return RedirectToAction("Index");
    }

    public ActionResult ListDoctors()
    {
        List<Doctor> doctorData = GetDoctorsFromDatabase();

        StringBuilder doctorListHtml = new StringBuilder();
        doctorListHtml.Append("<div class='doctor-list'><ul>");

        doctorListHtml.Append("<header class='doctor-item'>Input time as HH:mm and Date as dd-MM-yyyy</header>");

        foreach (var doctor in doctorData)
        {
            doctorListHtml.Append($"<li class='doctor-item'>{doctor.Name} - {doctor.Specjalization}");
            doctorListHtml.Append($"<button class='add-schedule-button' data-doctor-id='{doctor.Id}'>Add Schedule</button>");
            doctorListHtml.Append("</li>");
        }

        doctorListHtml.Append("</ul></div>");

        ViewBag.DoctorList = doctorListHtml.ToString();
        return View();
    }

    private List<Doctor> GetDoctorsFromDatabase()
    {
        dbHelper.OpenConnection();

        List<Doctor> doctorData = new List<Doctor>();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, Specjalization, AspNetUsers.Id " +
                        "FROM AspNetUsers " +
                        "INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId " +
                        "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                        "WHERE AspNetRoles.Name = 'Doctor';";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    doctorData.Add(new Doctor()
                    {
                        Name = reader.GetString(0),
                        Specjalization = reader.GetString(1),
                        Id = reader.GetString(2)
                    });
                }
            }
        }

        dbHelper.CloseConnection();

        return doctorData;
    }

    // Action to create visits between shift begin and shift end every 15 minutes
    public IActionResult CreateVisits(string doctorId,string shiftBegin, string shiftEnd, string day)
    {
        //I have to check if the visit already exists

        shiftBegin = day + " " + shiftBegin;
        shiftEnd = day + " " + shiftEnd;

        DateTime shiftBeginP = DateTime.Parse(shiftBegin);
        DateTime shiftEndP = DateTime.Parse(shiftEnd);

        // Ensure the database connection is open
        dbHelper.OpenConnection();


        TimeSpan interval = TimeSpan.FromMinutes(15);

        // Generate time slots between shiftBegin and shiftEnd
        List<DateTime> timeSlots = new List<DateTime>();
        DateTime currentSlot = shiftBeginP;

        while (currentSlot < shiftEndP)
        {
            timeSlots.Add(currentSlot);
            currentSlot = currentSlot.Add(interval);
        }
        // Create visits for each time slot and add to the database
        foreach (var slot in timeSlots)
        {
            string formattedDateTime = slot.ToString("yyyy-MM-dd HH:mm:ss");
            string insertQuery = "INSERT OR IGNORE INTO Visits (DoctorId, Date) VALUES (@DoctorId, @Date)";

            using (var command = new SqliteCommand(insertQuery, dbHelper.GetConnection()))
            {
                command.Parameters.AddWithValue("@DoctorId", doctorId);
                command.Parameters.AddWithValue("@Date", formattedDateTime);

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
        }
        // Close the database connection
        dbHelper.CloseConnection();

        return RedirectToAction("ListDoctors");
    }

    private List<User> GetPatientsFromDatabase()
    {
        SQLiteDatabaseHelper dbHelper = new SQLiteDatabaseHelper();
        dbHelper.OpenConnection();

        List<User> patientData = new List<User>();

        string query = "SELECT AspNetUsers.Name || ' ' || AspNetUsers.Surname AS FullName, AspNetUsers.Id, AspNetUsers.Activated " +
                        "FROM AspNetUsers " +
                        "INNER JOIN AspNetUserRoles ON AspNetUsers.Id = AspNetUserRoles.UserId " +
                        "INNER JOIN AspNetRoles ON AspNetUserRoles.RoleId = AspNetRoles.Id " +
                        "WHERE AspNetRoles.Name = 'Patient' AND Activated = 0;";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    patientData.Add(new User()
                    {
                        Name = reader.GetString(0),
                        Id = reader.GetString(1),
                        Activated = reader.GetBoolean(2)
                    });
                }
            }
        }

        dbHelper.CloseConnection();

        return patientData;
    }

    public ActionResult ListPatients(){
        List<User> patientData = GetPatientsFromDatabase();

        StringBuilder patientListHtml = new StringBuilder();
        patientListHtml.Append("<div class='patient-list'><ul>");

        foreach (var user in patientData)
        {
            patientListHtml.Append($"<li class='patient-item'>{user.Name}");
            patientListHtml.Append($"<button class='add-schedule-button' data-patient-id='{user.Id}'>Activate Account</button>");
            patientListHtml.Append("</li>");
        }

        
        ViewBag.PatientList = patientListHtml.ToString();
        return View();
    }
}
