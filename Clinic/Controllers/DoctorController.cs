using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;

namespace MvcMovie.Controllers;

public class DoctorController : Controller
{
    private SQLiteDatabaseHelper dbHelper;

    public DoctorController()
    {
        dbHelper = new SQLiteDatabaseHelper();
    }

    public IActionResult MyVisitsDoctor(){
        dbHelper.OpenConnection();

        DateTime today = DateTime.Today;

        string query = "SELECT D.Name || ' ' || D.Surname AS DoctorFullName, P.Name || ' ' || P.Surname AS PatientFullName, V.PatientId, V.DoctorId, V.Description,"+
        $"V.Date FROM Visits V INNER JOIN AspNetUsers D ON V.DoctorId = D.Id LEFT JOIN AspNetUsers P ON V.PatientId = P.Id WHERE Date(V.Date) = Date('{today:yyyy-MM-dd}')"+
        "AND (V.PatientId IS NOT NULL OR V.PatientId = '') AND (V.Description IS NULL OR V.Description = '') ORDER BY V.Date";

        List<Visit> visitData = new List<Visit>();

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string doctorName = !reader.IsDBNull(0) ? reader.GetString(0) : null;
                    string patientName = !reader.IsDBNull(1) ? reader.GetString(1) : null;
                    string patientId = !reader.IsDBNull(2) ? reader.GetString(2) : null;
                    string doctorId = !reader.IsDBNull(3) ? reader.GetString(3) : null;
                    string description = !reader.IsDBNull(4) ? reader.GetString(4) : null;
                    DateTime visitDateTime = !reader.IsDBNull(5) ? reader.GetDateTime(5) : default(DateTime);


                    visitData.Add(new Visit()
                    {
                        DoctorName = doctorName,
                        PatientName = patientName,
                        PatientId = patientId,
                        DoctorId = doctorId,
                        Description = description,
                        VisitDateTime = visitDateTime
                    });
                }
            }
        }

        dbHelper.CloseConnection();

        return View(visitData);
    }

    public IActionResult StartVisit(string patientName, string patientId, DateTime visitDateTime)
    {

        ViewBag.PatientId = patientId;
        ViewBag.PatientName = patientName;
        ViewBag.VisitDateTime = visitDateTime;
        return View();
    }

    [HttpPost]
    public IActionResult ProcessVisit(string patientName, string patientId, DateTime visitDateTime, string description)
    {
        dbHelper.OpenConnection();

        string query = $"UPDATE Visits SET Description = '{description}' WHERE PatientId = '{patientId}' AND Date = '{visitDateTime:yyyy-MM-dd HH:mm:ss}'";

        using (var command = new SqliteCommand(query, dbHelper.GetConnection()))
        {
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

        return RedirectToAction("MyVisitsDoctor", "Doctor");
    }
}