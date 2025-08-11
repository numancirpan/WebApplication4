using System.Diagnostics;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication4.Data;
using WebApplication4.Models;
using iText.IO.Font;

namespace WebApplication4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Employee()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }
        [HttpGet]
        public IActionResult GetEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            return Json(employee);
        }

        // Ekleme/Güncelleme (ortak action)
        [HttpPost]
        public async Task<IActionResult> SaveEmployee([FromBody] Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (employee.Id == 0) // ADD
                    {
                        _context.Employees.Add(employee);
                    }
                    else // UPDATE
                    {
                        var existingEmployee = await _context.Employees.FindAsync(employee.Id);
                        if (existingEmployee != null)
                        {
                            existingEmployee.FullName = employee.FullName;
                            existingEmployee.Location = employee.Location;
                            existingEmployee.PhoneNumber = employee.PhoneNumber;
                            existingEmployee.Note = employee.Note;
                            existingEmployee.Department = employee.Department;
                            existingEmployee.IsActive = employee.IsActive;
                            existingEmployee.Email = employee.Email;
                            _context.Employees.Update(existingEmployee);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving employee");
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest(ModelState);
        }

        // Silme
        [HttpPost]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                _context.SaveChanges();
            }
            return RedirectToAction("Employee");
        }
        public ActionResult Equipment()
        {
            var equipments = _context.Equipments.ToList();
            return View(equipments);
        }
        [HttpGet]
        public IActionResult GetEquipment(int id)
        {
            var equipment = _context.Equipments.Find(id);
            return Json(equipment);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEquipment([FromBody] Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (equipment.Id == 0) // ADD
                    {
                        _context.Equipments.Add(equipment);
                    }
                    else // UPDATE
                    {
                        var existingEquipment = await _context.Equipments.FindAsync(equipment.Id);
                        if (existingEquipment != null)
                        {
                            existingEquipment.EquipmentName = equipment.EquipmentName;
                            existingEquipment.AssetNumber = equipment.AssetNumber;
                            existingEquipment.ServiceTag = equipment.ServiceTag;
                            existingEquipment.Status = equipment.Status;
                            existingEquipment.EquipmentType = equipment.EquipmentType;
                            existingEquipment.SerialOrIMEI = equipment.SerialOrIMEI;
                            existingEquipment.ModelYear = equipment.ModelYear;
                            existingEquipment.Notes = equipment.Notes;
                            _context.Equipments.Update(existingEquipment);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving equipment");
                    return BadRequest(ex.Message);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost]
        public IActionResult DeleteEquipment(int id)
        {
            var equipment = _context.Equipments.Find(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                _context.SaveChanges();
            }
            return RedirectToAction("Equipment");
        }

        public IActionResult Assignment()
        {
            var viewModel = new AssignmentViewModel
            {
                Employees = _context.Employees.ToList(), // Ensure this line exists
                Equipment = _context.Equipments.ToList(),
                Assignments = _context.Assignments
                    .Include(a => a.Employee)
                    .Include(a => a.Equipment)
                    .ToList()
            };

            return View(viewModel);
        }

        // Get assignments
        [HttpGet]
        public IActionResult GetAssignments()
        {
            var assignments = _context.Assignments
    .Include(a => a.Employee)
    .Include(a => a.Equipment)
    .Where(a => a.Status == "Active" && a.Employee != null && a.Equipment != null)
    .Select(a => new {
        a.Id,
        EmployeeId = a.Employee!.Id, // Null forgiving operator (!) kullanımı
        EmployeeName = a.Employee!.FullName,
        EquipmentId = a.Equipment!.Id,
        EquipmentName = a.Equipment!.EquipmentName,
        a.AssignmentDate,
        a.HasCharger,
        a.HasHeadset,
        a.Notes
    })
    .ToList();
            return Json(assignments);
        }

        // Save assignment
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SaveAssignment([FromBody] Assignment assignment)
        {
            try
            {
                // Validate required fields
                if (assignment.EmployeeId <= 0 || assignment.EquipmentId <= 0)
                {
                    return BadRequest("Employee and Equipment must be selected");
                }

                // Check if equipment is already assigned
                var existingAssignment = await _context.Assignments
                    .FirstOrDefaultAsync(a => a.EquipmentId == assignment.EquipmentId && a.Status == "Active");

                if (existingAssignment != null && existingAssignment.Id != assignment.Id)
                {
                    return BadRequest("This equipment is already assigned to another employee");
                }

                // Get the equipment
                var equipment = await _context.Equipments.FindAsync(assignment.EquipmentId);
                if (equipment == null)
                {
                    return NotFound("Equipment not found");
                }

                if (assignment.Id == 0) // New assignment
                {
                    assignment.AssignmentDate = DateTime.Now;
                    assignment.Status = "Active";
                    _context.Assignments.Add(assignment);

                    // Update equipment status to "Assigned"
                    equipment.Status = "Assigned";
                    _context.Equipments.Update(equipment);
                }
                else // Update existing assignment
                {
                    var dbAssignment = await _context.Assignments.FindAsync(assignment.Id);
                    if (dbAssignment == null)
                    {
                        return NotFound("Assignment not found");
                    }

                    // If equipment is being changed
                    if (dbAssignment.EquipmentId != assignment.EquipmentId)
                    {
                        // Set old equipment status to "Available"
                        var oldEquipment = await _context.Equipments.FindAsync(dbAssignment.EquipmentId);
                        if (oldEquipment != null)
                        {
                            oldEquipment.Status = "Available";
                            _context.Equipments.Update(oldEquipment);
                        }

                        // Set new equipment status to "Assigned"
                        equipment.Status = "Assigned";
                        _context.Equipments.Update(equipment);
                    }

                    // Update all properties
                    dbAssignment.EmployeeId = assignment.EmployeeId;
                    dbAssignment.EquipmentId = assignment.EquipmentId;
                    dbAssignment.HasCharger = assignment.HasCharger;
                    dbAssignment.HasHeadset = assignment.HasHeadset;
                    dbAssignment.HasDeckStation = assignment.HasDeckStation;
                    dbAssignment.HasWirelessHeadset = assignment.HasWirelessHeadset;
                    dbAssignment.HasKeyboard = assignment.HasKeyboard;
                    dbAssignment.HasLaptopBag = assignment.HasLaptopBag;
                    dbAssignment.HasMouse = assignment.HasMouse;
                    dbAssignment.HasJabraSpeak = assignment.HasJabraSpeak;
                    dbAssignment.OtherAccessories = assignment.OtherAccessories;
                    dbAssignment.Notes = assignment.Notes;

                    _context.Assignments.Update(dbAssignment);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Assignment saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving assignment");
                return StatusCode(500, "An error occurred while saving the assignment");
            }
        }

        // Return equipment
        [HttpPost]
        [HttpGet]
        public IActionResult GetAssignment(int id)
        {
            var assignment = _context.Assignments.Find(id);
            return Json(assignment);
        }

        [HttpPost]
        [HttpPost]
        [HttpPost]
        public IActionResult DeleteAssignment([FromBody] int id)
        {
            try
            {
                var assignment = _context.Assignments.Find(id);
                if (assignment == null)
                {
                    return NotFound("Assignment not found");
                }

                // Equipment durumunu "Available" olarak güncelle
                var equipment = _context.Equipments.Find(assignment.EquipmentId);
                if (equipment != null)
                {
                    equipment.Status = "Available";
                    _context.Equipments.Update(equipment);
                }

                _context.Assignments.Remove(assignment);
                _context.SaveChanges();

                return Ok(new { success = true, message = "Assignment deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment");
                return StatusCode(500, new { success = false, message = "Error deleting assignment" });
            }
        }

        // ViewModel class
        public class AssignmentViewModel
        {
            public List<Employee> Employees { get; set; } = new List<Employee>();
            public List<Equipment> Equipment { get; set; } = new List<Equipment>();
            public List<Assignment> Assignments { get; set; } = new List<Assignment>();
        }
       
        public ActionResult Export()
        {
            var viewModel = new ExportViewModel
            {
                Employees = _context.Employees.ToList()
            };
            return View(viewModel);
        }

        [HttpGet]
        [HttpGet]
        [HttpGet]
        public IActionResult GetEmployeeEquipment(int employeeId)
        {
            var equipment = _context.Assignments
                .Include(a => a.Equipment)
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == "Active" &&
                           a.Equipment != null)
                .Select(a => new {
                    AssignmentId = a.Id,
                    EquipmentId = a.Equipment!.Id,
                    EquipmentName = a.Equipment!.EquipmentName,
                    AssetNumber = a.Equipment!.AssetNumber,
                    ServiceTag = a.Equipment!.ServiceTag,
                    Status = a.Equipment!.Status,
                    AssignmentDate = a.AssignmentDate
                })
                .ToList();

            return Json(equipment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePdf([FromBody] PdfRequestData requestData)
        {
            try
            {
                // Input validation (existing code remains the same)
                if (requestData == null || requestData.EmployeeId <= 0)
                {
                    return BadRequest("Invalid request data");
                }

                // Get employee data (existing code remains the same)
                var employee = await _context.Employees.FindAsync(requestData.EmployeeId);
                if (employee == null)
                {
                    return NotFound("Employee not found");
                }

                // Get active assignments (existing code remains the same)
                var assignments = await _context.Assignments
                    .Include(a => a.Equipment)
                    .Where(a => a.EmployeeId == requestData.EmployeeId && a.Status == "Active")
                    .OrderBy(a => a.AssignmentDate)
                    .ToListAsync();

                if (!assignments.Any())
                {
                    return BadRequest("No active equipment assignments found");
                }

                // Load PDF template (existing code remains the same)
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "t.pdf");
                if (!System.IO.File.Exists(templatePath))
                {
                    return StatusCode(500, "PDF template not found");
                }

                using var reader = new PdfReader(templatePath);
                using var outputStream = new MemoryStream();
                using var writer = new PdfWriter(outputStream);
                using var pdfDoc = new PdfDocument(reader, writer);

                // Get first page (existing code remains the same)
                var page = pdfDoc.GetFirstPage();
                if (page == null)
                {
                    return StatusCode(500, "Template has no pages");
                }

                // Create canvas for editing (existing code remains the same)
                var canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

                // Font configuration (existing code remains the same)
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // 1. Write employee name (existing code remains the same)
                canvas.BeginText()
                    .SetFontAndSize(font, 12)
                    .MoveText(198, 713)
                    .ShowText(employee.FullName)
                    .EndText();

                // 2. Fill equipment data (max 5 items) (existing code remains the same)
                for (int i = 0; i < Math.Min(assignments.Count, 5); i++)
                {
                    var assignment = assignments[i];
                    var equipment = assignment.Equipment;
                    if (equipment == null) continue;

                    float yOffset = i * 96;

                    canvas.BeginText()
                        .SetFontAndSize(font, 10)
                        .MoveText(66, 670 - yOffset)
                        .ShowText(equipment.EquipmentName ?? "")
                        .MoveText(124, 0)
                        .ShowText(equipment.AssetNumber ?? "")
                        .MoveText(0, -45)
                        .ShowText(equipment.SerialOrIMEI ?? "")
                        .MoveText(146, 50)
                        .ShowText(assignment.HasCharger ? "X" : "")
                        .MoveText(0, -47)
                        .ShowText(assignment.HasDeckStation ? "X" : "")
                        .MoveText(72, -8)
                        .ShowText(assignment.AssignmentDate.ToString("dd.MM.yyyy"))
                        .EndText();
                }

                // 3. Fill accessories data - NEW IMPLEMENTATION
                // Collect all accessories from all assignments
                bool hasKeyboard = assignments.Any(a => a.HasKeyboard);
                bool hasMouse = assignments.Any(a => a.HasMouse);
                bool hasHeadset = assignments.Any(a => a.HasHeadset);
                bool hasWirelessHeadset = assignments.Any(a => a.HasWirelessHeadset);
                bool hasLaptopBag = assignments.Any(a => a.HasLaptopBag);
                bool hasJabraSpeak = assignments.Any(a => a.HasJabraSpeak);

                // Combine all OtherAccessories from all assignments
                var otherAccessories = assignments
                    .Where(a => !string.IsNullOrEmpty(a.OtherAccessories))
                    .Select(a => a.OtherAccessories)
                    .Distinct()
                    .ToList();

                canvas.BeginText()
                    .SetFontAndSize(font, 10)
                    .MoveText(62, 212)
                    .ShowText(hasKeyboard ? "X" : "")
                    .MoveText(0, -21)
                    .ShowText(hasMouse ? "X" : "")
                    .MoveText(115, 21)
                    .ShowText(hasHeadset ? "X" : "")
                    .MoveText(0, -21)
                    .ShowText(hasWirelessHeadset ? "X" : "")
                    .MoveText(130, 21)
                    .ShowText(hasLaptopBag ? "X" : "")
                    .MoveText(0, -21)
                    .ShowText(hasJabraSpeak ? "X" : "")
                    .MoveText(115, 21)
                    .ShowText(otherAccessories.Any() ? "X" : "")
                    .MoveText(38, 1)
                    .ShowText(string.Join(", ", otherAccessories) ?? "")
                    .EndText();

                // 4. Add generation date (existing code remains the same)
                canvas.BeginText()
                    .SetFontAndSize(font, 8)
                    .MoveText(450, 30)
                    .ShowText($"Generated on: {DateTime.Now:dd.MM.yyyy HH:mm}")
                    .EndText();

                pdfDoc.Close();

                var fileName = $"Equipment_{employee.FullName?.Replace(" ", "_") ?? requestData.EmployeeId.ToString()}.pdf";
                return File(outputStream.ToArray(), "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF generation error");
                return StatusCode(500, $"PDF generation failed: {ex.Message}");
            }
        }

        public class PdfRequestData
        {
            public int EmployeeId { get; set; }
            public string? EmployeeName { get; set; }
            public List<EquipmentData>? Equipment { get; set; }
        }

        public class EquipmentData
        {
            public int EquipmentId { get; set; }
            public string? EquipmentName { get; set; }
            public string? AssetNumber { get; set; }
            public string? ServiceTag { get; set; }
            public string? Status { get; set; }
            public DateTime AssignmentDate { get; set; }
        }

        // Add this class inside HomeController
        public class ExportViewModel
        {
            public List<Employee> Employees { get; set; } = new List<Employee>();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
