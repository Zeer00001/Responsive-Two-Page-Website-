using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using AcademicManagement;

namespace Lab3.Pages
{
    public class RegistrationModel : PageModel
    {
        [BindProperty]
        public string SelectedStudentId { get; set; }

        [BindProperty]
        public List<string> SelectedCourses { get; set; }

        [BindProperty]
        public Dictionary<string, string> CourseGrades { get; set; }

        public List<SelectListItem> Students { get; set; }

        public List<Course> AvailableCourses { get; set; }

        public List<AcademicRecord> StudentCourses { get; set; }

        public bool ShowCourses { get; set; } = false;

        public string ErrorMessage { get; set; }

        public string SortField { get; set; }
        public bool SortAscending { get; set; } = true;

        public void OnGet(string sortField = "CourseCode", bool sortAscending = true)
        {
            SortField = sortField;
            SortAscending = sortAscending;

            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("SelectedStudentId")))
            {
                SelectedStudentId = HttpContext.Session.GetString("SelectedStudentId");
                LoadStudentRecords();
            }

            LoadStudents();
            LoadAvailableCourses();
        }

        public IActionResult OnPostStudentSelected()
        {
            if (string.IsNullOrEmpty(SelectedStudentId))
            {
                ErrorMessage = "You must select a student.";
                LoadStudents();
                return Page();
            }

            HttpContext.Session.SetString("SelectedStudentId", SelectedStudentId);
            LoadStudentRecords();

            LoadStudents();
            return Page();
        }

        public IActionResult OnPostRegister()
        {
            if (string.IsNullOrEmpty(SelectedStudentId))
            {
                ErrorMessage = "You must select a student.";
                LoadStudents();
                return Page();
            }

            if (SelectedCourses == null || SelectedCourses.Count == 0)
            {
                ErrorMessage = "You must select at least one course!";
                LoadStudents();
                AvailableCourses = DataAccess.GetAllCourses();
                ShowCourses = true;
                return Page();
            }

            foreach (var courseCode in SelectedCourses)
            {
                var course = DataAccess.GetAllCourses().Find(c => c.CourseCode == courseCode);
                if (course != null)
                {
                    var newRecord = new AcademicRecord(SelectedStudentId, course.CourseCode);
                    DataAccess.AddAcademicRecord(newRecord);
                }
            }

            LoadStudentRecords();
            LoadStudents();
            return Page();
        }

        public IActionResult OnPostSubmitGrades()
        {
            foreach (var courseGrade in CourseGrades)
            {
                var record = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId)
                                       .FirstOrDefault(r => r.CourseCode == courseGrade.Key);

                if (record != null)
                {
                    if (double.TryParse(courseGrade.Value, out double parsedGrade))
                    {
                        record.Grade = parsedGrade; 
                    }
                    else
                    {
                        ErrorMessage = $"Invalid grade entered for course {record.CourseCode}. Please enter a valid numeric grade.";
                        LoadStudentRecords();
                        LoadStudents();
                        return Page();
                    }
                }
            }

            LoadStudentRecords();
            LoadStudents();
            return Page();
        }

        private void LoadStudents()
        {
            var students = DataAccess.GetAllStudents();
            Students = students.Select(s => new SelectListItem
            {
                Value = s.StudentId,
                Text = s.StudentName
            }).ToList();
        }

        private void LoadStudentRecords()
        {
            var academicRecords = DataAccess.GetAcademicRecordsByStudentId(SelectedStudentId);
            if (academicRecords.Count > 0)
            {
                StudentCourses = academicRecords;

                if (SortField == "CourseTitle")
                {
                    StudentCourses = SortAscending
                        ? StudentCourses.OrderBy(r => DataAccess.GetAllCourses().First(c => c.CourseCode == r.CourseCode).CourseTitle).ToList()
                        : StudentCourses.OrderByDescending(r => DataAccess.GetAllCourses().First(c => c.CourseCode == r.CourseCode).CourseTitle).ToList();
                }
                else if (SortField == "Grade")
                {
                    StudentCourses = SortAscending
                        ? StudentCourses.OrderBy(r => r.Grade).ToList()
                        : StudentCourses.OrderByDescending(r => r.Grade).ToList();
                }
                else
                {
                    StudentCourses = SortAscending
                        ? StudentCourses.OrderBy(r => r.CourseCode).ToList()
                        : StudentCourses.OrderByDescending(r => r.CourseCode).ToList();
                }
            }
            else
            {
                AvailableCourses = DataAccess.GetAllCourses();
            }

            ShowCourses = true;
        }

        private void LoadAvailableCourses()
        {
            AvailableCourses = DataAccess.GetAllCourses();

            if (SortField == "CourseTitle")
            {
                AvailableCourses = SortAscending
                    ? AvailableCourses.OrderBy(c => c.CourseTitle).ToList()
                    : AvailableCourses.OrderByDescending(c => c.CourseTitle).ToList();
            }
            else
            {
                AvailableCourses = SortAscending
                    ? AvailableCourses.OrderBy(c => c.CourseCode).ToList()
                    : AvailableCourses.OrderByDescending(c => c.CourseCode).ToList();
            }
        }
    }
}
