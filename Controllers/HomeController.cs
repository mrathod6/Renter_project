using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Rntr.Models;

namespace Rntr.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }


        // Homepage showing registration form for the user and the login link for registered users.
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }


        // Registration Post method
        [HttpPost("register")]
        public IActionResult Register(User newUser)
        {
            // If tModelState.IsValid...
            if (ModelState.IsValid)
            {
                // Check to see if a User exists with provided email, if so go back to Homepage
                if (dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                // else hash the password and insert newUser into the database
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();

                    //setting user in session and redirecting to the Success page
                    HttpContext.Session.SetInt32("LoggedInId", newUser.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            else
            {
                //If the !ModelState.IsValid return to the home-page
                return View("Index");
            }
        }


        // Route to display Dashboard
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            //Setting the user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            Console.WriteLine("User in Session, we are in the Dashboard");

            //If the user has no profile picture
            if (LoggedInUser.UserPhoto == null)
            {
                Console.WriteLine("Coming through WITHOUT a profile picture");
                ViewBag.LoggedInUser = LoggedInUser;
                var AllTools = dbContext.Tools;
                ViewBag.AllTools = AllTools;
                return View();
            }

            //If the user has a profile picture we'll convert that byte array to a string and use it to display it on the front end
            else
            {
                Console.WriteLine("Coming through WITH a Profile Picture in the database");
                string imageBase64Data = Convert.ToBase64String(LoggedInUser.UserPhoto);
                string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                ViewBag.ImageData = imageDataURL;

                ViewBag.LoggedInUser = LoggedInUser;

                ViewBag.AllTools = dbContext.Tools;
                return View();
            }
        }


        // Login page showing just the login form and a back button to go back to Homepage
        [HttpGet("login")]
        public IActionResult Login()
        {
            Console.WriteLine("Displaying Login Page");
            return View();
        }


        //Login Process
        [HttpPost("LoginProcess")]
        public IActionResult LoginProcess(LogUser LoggedIn)
        {
            // If ModelState.IsValid...
            if (ModelState.IsValid)
            {
                // Look for email in Users table
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == LoggedIn.LogEmail);

                // If e-mail doesn't exist in the databatse return to Login page with error message
                if (userInDb == null)
                {
                    Console.WriteLine("Email is invalid");
                    ModelState.AddModelError("LogEmail", "Invalid Email/Password");
                    return View("Login");
                }

                // else hash the password and check it against the email
                var hasher = new PasswordHasher<LogUser>();
                var result = hasher.VerifyHashedPassword(LoggedIn, userInDb.Password, LoggedIn.LogPassword);

                // If result doesn't match return to Login page with error message
                if (result == 0)
                {
                    Console.WriteLine("Password is invalid");
                    ModelState.AddModelError("LogEmail", "Invalid Email/Password");
                    return View("Login");
                }

                // If email and password are a match, set user in session using UserId and redirect to Dashboard
                Console.WriteLine("Login Success");
                HttpContext.Session.SetInt32("LoggedInId", userInDb.UserId);
                return RedirectToAction("Dashboard");
            }

            // If Model.State is Invalid return to the Login page.
            {
                Console.WriteLine("Model.State is invalid");
                return View("Login");
            }
        }


        // Route to display Profile picture form
        [HttpGet("/addprofilepicture")]
        public IActionResult ProfilePictureForm()
        {
            return View();
        }


        // Route to process and add Profile Picture
        [HttpPost("/profilepictureprocess")]
        public IActionResult AddProfilePictureProcess(ICollection<IFormFile> ProfilePicture)
        {
            // Setting the User in Session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));

            // If picture is empty then return back to Profile Picture Form
            if (ProfilePicture.Count == 0)
            {
                ModelState.AddModelError("UserPhoto", "Profile picture cannot be empty");
                Console.WriteLine("No Picture was added");
                return View("ProfilePictureForm");
            }

            // else convert it to a byte[] and save image in the database and return to the dashboard which should now display the profile picture
            else{
            foreach( var file in ProfilePicture)
            {
                 if(file.Length>0)
                {
                        using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToBase64String(fileBytes);
                        byte[] bites = Convert.FromBase64String(s);
                        LoggedInUser.UserPhoto = bites;
                        dbContext.SaveChanges();
                        Console.WriteLine("Profile picture successfully addded");
                    }
            }
            }
            return Redirect ("/Dashboard");
        }
        }

        // Route for MyProfile
        [HttpGet("myprofile")]
        public IActionResult MyProfile()
        {
            //Setting the user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));

            // If the user doesn't have their profile picture added yet go through here
            if(LoggedInUser.UserPhoto == null){
            ViewBag.LoggedInUser = LoggedInUser;
             var AllMyTools = dbContext.Tools
            .Where( ui => ui.Owner.UserId == LoggedInUser.UserId);

            ViewBag.AllMyTools = AllMyTools;
            return View();
            }

            // else convert the profile picture to a string and go through this route
            else{
            string imageBase64Data = Convert.ToBase64String(LoggedInUser.UserPhoto);
            string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
            ViewBag.LoggedInUser = LoggedInUser;
            ViewBag.Image = imageDataURL;

            var AllMyTools = dbContext.Tools
            .Where( ui => ui.Owner.UserId == LoggedInUser.UserId);

            ViewBag.AllMyTools = AllMyTools;
            Console.WriteLine("Coming through WITH a profile picture");
            return View();
            }
        }


        // Route to show Edit My Profile Form
        [HttpGet("editmyprofile")]
        public IActionResult EditMyProfile()
        {
            return View();
        }


        // Edit My Profile Process
        [HttpPost("editmyprofileprocess")]
        public IActionResult EditMyProfileProcess(User editUser, ICollection<IFormFile> ProfilePicture)
        {
            // If the model state is valid
            if (ModelState.IsValid)
            {
                int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
                var LoggedInUser = dbContext.Users
                .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));

                // Setting the users name, last name and email to inputs from the form
                LoggedInUser.FirstName = editUser.FirstName;
                LoggedInUser.LastName = editUser.LastName;
                LoggedInUser.Email = editUser.Email;

                // Setting the password after hashing it
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                LoggedInUser.Password = Hasher.HashPassword(editUser, editUser.Password);
                // Converting the inputted picture to a byte[]
                foreach (var file in ProfilePicture)
                {
                    if(file.Length>0)
                    {
                        using (var ms = new MemoryStream())
                        {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        string s = Convert.ToBase64String(fileBytes);
                        byte[] bites = Convert.FromBase64String(s);
                        LoggedInUser.UserPhoto = bites;
                        dbContext.SaveChanges();
                        Console.WriteLine("Profile picture successfully addded");
                        }
                    }
                }

                // Updating the Updated at to Now
                LoggedInUser.Updated_at = DateTime.Now;
                dbContext.SaveChanges();

                // Successful addition goes to dashboard
                return Redirect ("/Dashboard");
            }

            //If ModelState is InValid return back to edit my profile view
            return Redirect("editmyprofile");
        }


        // Edit My Tool View
        [HttpGet("editmytools")]
        public IActionResult EditMyTools()
        {
            // Setting the user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

            // ViewBagging All Tools and ordering by Updated at displaying the recently changed tools
            ViewBag.AllMyTools = dbContext.Tools
            .Where(n => n.Owner.UserId == LoggedInUser.UserId)
            .OrderBy(i => i.Updated_at)
            .ToList();
            return View();
        }


        // Add Tool View
        [HttpGet("addtool")]
        public IActionResult AddTool()
        {
            // Setting the user in Session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;
            return View();
        }


        //Process for adding a tool
        [HttpPost("addtoolprocess")]
        public IActionResult AddToolProcess(Tool newTool, ICollection<IFormFile> ToolImage)
        {
            // Setting the user in Session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

                // If the image input was empty return back to AddTool View
                if(ToolImage.Count == 0){
                    Console.WriteLine("Tool not passing through");
                    return View("AddTool");
                }

                // Else set the new tools Owner to LoggedInUser and id to same
                else{
                newTool.Owner = LoggedInUser;
                newTool.Owner.UserId = LoggedInUser.UserId;

                    // Convert image to byte[] and add to database
                    foreach(var file in ToolImage)
                    {
                        if(file.Length>0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                string s = Convert.ToBase64String(fileBytes);
                                byte[] bites = Convert.FromBase64String(s);
                                newTool.ToolImage1 = bites;
                                dbContext.Add(newTool);
                                dbContext.SaveChanges();
                                Console.WriteLine("New Tool Successfully added");
                            }
                        }
                    }
                }

                // Return back to My Profile page with new tool displaying at the bottom of All My Tools
        return RedirectToAction("MyProfile");
        }


        // Route to delete a tool, returns back to MyProfile page
        [HttpGet("delete/{toolId}")]
        public IActionResult DeleteTool(int toolId)
        {
            // Locating the specific tool in the database and removing it
            Tool RetrievedTool = dbContext.Tools.SingleOrDefault(t => t.ToolId == toolId);
            dbContext.Tools.Remove(RetrievedTool);
            dbContext.SaveChanges();

            // Returning back to MyProfile page
            return Redirect("/myprofile");
        }


        // EditTool form View
        [HttpGet("edittoolform/{toolid}")]
        public IActionResult EditToolForm(int toolid)
        {
            // Setting user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

            // ViewBagging the tool we want to display
            ViewBag.ThisTool = dbContext.Tools
            .SingleOrDefault(n => n.ToolId == toolid);
            return View();
        }


        // Edit Tool Process Route
        [HttpPost("edittoolprocess/{toolid}")]
        public IActionResult EditToolProcess(Tool editTool, int toolid,  ICollection<IFormFile> toolimage)
{
            // Setting user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

            // If either of the inputs are invalid, we go back to the EditToolForm
            if(toolimage.Count == 0 || editTool.ToolName == null || editTool.Description == null){
                ViewBag.ThisTool = dbContext.Tools
            .SingleOrDefault(n => n.ToolId == toolid);
                return View("EditToolForm");
            }

            // else...
            else {

                //Grabbing this tool by the int passed in the route
                var thisthing = dbContext.Tools
                .SingleOrDefault(t => t.ToolId == toolid);
                thisthing.ToolName = editTool.ToolName;
                thisthing.Description = editTool.Description;
                thisthing.Updated_at = DateTime.Now;
                dbContext.SaveChanges();

                // converting input image to byte []
                foreach (var file in toolimage){
                    if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                                {
                                    file.CopyTo(ms);
                                    var fileBytes = ms.ToArray();
                                    string s = Convert.ToBase64String(fileBytes);
                                    byte[] bites = Convert.FromBase64String(s);
                                    thisthing.ToolImage1 = bites;
                                    dbContext.SaveChanges();
                                }
                        }
                }
                dbContext.SaveChanges();
                ViewBag.AllTools = dbContext.Tools
                .Where(n => n.Owner.UserId == LoggedInUser.UserId)
                .OrderBy(i => i.Updated_at)
                .ToList();
                return Redirect("/editmytools");
            }
            }


        // Route that directs to All Tools page, displaying All Tools in the db
        [HttpGet("alltools")]
        public IActionResult AllTools()
        {
            // Setting the user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

            // Variable for all the tools
            List<Tool> AllTools = dbContext.Tools
            .Where(a => a.ToolAvailability == true)
            .Include(o => o.Owner)
            .ToList();
            ViewBag.AllTools = AllTools;
            return View();
        }


        // Route to view individual tool
        [HttpGet("/viewTool/{toolid}")]
        public IActionResult ViewTool(int toolid)
        {
            ViewBag.AllTools = dbContext.Tools.Where( t => t.ToolId == toolid)
            .Include( o => o.Owner);
            return View();
        }


        //Route for Rent A Tool View
        [HttpGet("rentatool")]
        public IActionResult RentToolView()
        {
        // Setting user in session
        int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
        var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
        ViewBag.LoggedInUser = LoggedInUser;

        // ViewBagging AllTools including Owner object
        List<Tool> AllTools = dbContext.Tools
        .Where( j => j.ToolAvailability == true)
        .Where( i => i.Owner.UserId != LoggedInUser.UserId)
        .Include(o => o.Owner)
        .ToList();

        ViewBag.AllTools = AllTools;

        return View();
        }


        // Rent tool process, making a request to the Rntr
        [HttpPost("renttoolprocess/{toolid}")]
        public IActionResult RentalToolProcess(Rental newRental, int toolid)
        {
        // Setting user in session
        int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
        var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
        ViewBag.LoggedInUser = LoggedInUser;

        // Here we are setting the variable thistool to the tool in question
        var thistool = dbContext.Tools
        .FirstOrDefault( t => t.ToolId == toolid);

        // Setting newRental to tool and setting availability to false
        newRental.ToolId = thistool.ToolId;
        newRental.ToolRented = thistool;
        newRental.RenteeId = LoggedInUser.UserId;
        newRental.Rentee = LoggedInUser;
        thistool.ToolAvailability = false;
        dbContext.Add(newRental);
        dbContext.SaveChanges();
        Console.WriteLine("Tool successfully requested");

        return Redirect("/requestprocessed");
        }


        //Request Processed View showing that the tool rental request was successfully processed
        [HttpGet("requestprocessed")]
        public IActionResult RequestProcessedView()
            {
                return View();
            }


        // Route for MyRentals page
        [HttpGet("myrentals")]
        public IActionResult MyRentals()
        {
        // Setting user in session
        int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
        var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
        ViewBag.LoggedInUser = LoggedInUser;

        // ViewBagging AllRentals, including the Rentee and the ToolRented
            var AllRentals = dbContext.Rentals
            .Where( ri => ri.RenteeId == LoggedInUser.UserId)
            .Where (c => c.check == true)
            .Include(r => r.Rentee)
            .Include( tr => tr.ToolRented)
            .ThenInclude( o => o.Owner)
            .ToList();
            ViewBag.AllRentals = AllRentals;
            return View();
        }


        // Route for requests page
        [HttpGet("requests")]
        public IActionResult Requests()
        {
            // Setting user in session
            int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
            var LoggedInUser = dbContext.Users
            .FirstOrDefault(u => u.UserId ==     Convert.ToInt32(IntVariable));
            ViewBag.LoggedInUser = LoggedInUser;

            // ViewBagging AllRequests including Rentee and the ToolRented
            ViewBag.AllRequests = dbContext.Rentals
            .Where(c => c.check == false)
            .Where( ri => ri.RenteeId != LoggedInUser.UserId)
            .Include( r => r.Rentee)
            .Include( t => t.ToolRented)
            .ToList();
            return View();
        }


        // Process to Accept a rental
        [HttpPost("acceptrentalprocess/{toolid}")]
        public IActionResult AcceptRentalProcess(int toolid)
        {
        // Setting the user in session
        int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
        var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
        ViewBag.LoggedInUser = LoggedInUser;

        // Here we are setting the variable thistool to the tool in question
        var thisRental = dbContext.Rentals
        .FirstOrDefault( t => t.ToolId == toolid);
        thisRental.check = true;
        dbContext.SaveChanges();
        Console.WriteLine("Rental Accepted");
        return Redirect("/dashboard");
        }


        //Return Process
        [HttpPost("returnprocess/{rentalid}/{toolid}")]
        public IActionResult ReturnProcess(Rental returnRental, int rentalid, int toolid)
        {
        // Setting the user in session
        int? IntVariable = HttpContext.Session.GetInt32("LoggedInId");
        var LoggedInUser = dbContext.Users.FirstOrDefault(u => u.UserId == Convert.ToInt32(IntVariable));
        ViewBag.LoggedInUser = LoggedInUser;

        // Here we are setting the variable thistool to the tool in question
        var thistool = dbContext.Tools
        .FirstOrDefault( t => t.ToolId == toolid);
        thistool.ToolAvailability = true;
        dbContext.SaveChanges();
        Console.WriteLine(thistool.ToolId);
        Console.WriteLine(thistool.ToolName);
        Console.WriteLine(thistool.ToolAvailability);

        // Finding the rental and then removing it from the database
        var ugh = dbContext.Rentals
        .FirstOrDefault( i => i.RentalId == rentalid);
        dbContext.Remove(ugh);
        dbContext.SaveChanges();
        Console.WriteLine("Rental successfully returned");
        return Redirect("/dashboard");
        }


        // Logout Route, clearing session and then returning to Homepage
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Console.WriteLine("Logging out User, Clearing Session");
            HttpContext.Session.Clear();
            return Redirect("/");
        }
    }
    }
