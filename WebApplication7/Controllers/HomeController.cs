using System.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WebApplication7.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.SqlServer.Types;

namespace WebApplication7.Controllers
{
    public class HomeController : Controller
    {
        public string username = null;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        public IActionResult Index(string username)
        {
            ViewBag.username = username;
            return View();
        }
        
        [HttpPost]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult AddLocation([FromForm] Cords model)
        {
            string connectionString = _configuration.GetConnectionString("con");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "INSERT INTO Punkty (Nazwa, Koordynaty, Username) VALUES (@Name, geography::STGeomFromText('POINT(' + CAST(@Longitude AS VARCHAR(20)) + ' ' + CAST(@Latitude AS VARCHAR(20)) + ')', 4326), @Username)";
                command.Parameters.AddWithValue("@Name", model.Name);
                command.Parameters.AddWithValue("@Longitude", model.Longitude);
                command.Parameters.AddWithValue("@Latitude", model.Latitude);
                command.Parameters.AddWithValue("@Username", model.Username);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Authorize(Policy = "RequireLoggedIn")]
        public IActionResult GetUserMarkers()
        {
            string username = User.Identity.Name;
            string connectionString = _configuration.GetConnectionString("con");
            List<Cords> markers = new List<Cords>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT Nazwa, Koordynaty.Long AS Longitude, Koordynaty.Lat AS Latitude, Username FROM Punkty WHERE Username=@Username";
                command.Parameters.AddWithValue("@Username", username);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    markers.Add(new Cords
                    {
                        Name = reader["Nazwa"].ToString(),
                        Longitude = double.Parse(reader["Longitude"].ToString()),
                        Latitude = double.Parse(reader["Latitude"].ToString()),
                    });
                }
                connection.Close();
            }
            return Json(markers);
        }
        [HttpPost]
        public IActionResult AddUser([FromForm] User model)
        {
            string connectionString = _configuration.GetConnectionString("con");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand checkUsername = new SqlCommand();
                checkUsername.Connection = connection;
                checkUsername.CommandType = CommandType.Text;
                checkUsername.CommandText = "SELECT * FROM Users WHERE Username LIKE (@Name)";
                checkUsername.Parameters.AddWithValue("@Name", model.Username);
                connection.Open();
                SqlDataReader reader = checkUsername.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Close();
                    ViewBag.UsernameTaken = true;
                    return Json(new { message = "Username is already taken" });
                }
                else
                {
                    reader.Close();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO Users (Username, Email, Password) VALUES (@Name, @Email, @Password)";
                    command.Parameters.AddWithValue("@Name", model.Username);
                    command.Parameters.AddWithValue("@Email", model.Email);
                    command.Parameters.AddWithValue("@Password", model.Password);
                    command.ExecuteNonQuery();
                    connection.Close();
                    return Json(new { redirectToUrl = Url.Action("Index") });
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> LoginUser([FromForm] User model)
        {
            string connectionString = _configuration.GetConnectionString("con");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand checkUser = new SqlCommand();
                checkUser.Connection = connection;
                checkUser.CommandType = CommandType.Text;
                checkUser.CommandText = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";
                checkUser.Parameters.AddWithValue("@Username", model.Username);
                checkUser.Parameters.AddWithValue("@Password", model.Password);
                connection.Open();
                SqlDataReader reader = checkUser.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Close();

                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username)
            };

                    var userIdentity = new ClaimsIdentity(claims, "login");

                    var principal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignInAsync(principal);

                    return RedirectToAction("Index", new { username = model.Username });
                }
                else
                {
                    reader.Close();
                    return Json(new { message = "Username or password is incorrect" });
                }
            }
        }
        public async Task<IActionResult> LogoutUser()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}