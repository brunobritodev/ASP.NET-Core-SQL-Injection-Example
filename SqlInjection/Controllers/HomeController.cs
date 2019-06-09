using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjection.Database;
using SqlInjection.Models;

namespace SqlInjection.Controllers
{
    public class HomeController : Controller
    {
        private readonly SchoolContext _context;

        public HomeController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet("SearchStudentUnsecure")]
        public async Task<IActionResult> SearchStudentUnsecure(string name)
        {
            var conn = _context.Database.GetDbConnection();
            var query = "SELECT FirstName, LastName FROM Student WHERE FirstName Like '%" + name + "%'";
            IEnumerable<Student> students;

            try
            {
                await conn.OpenAsync();
                students = await conn.QueryAsync<Student>(query);
            }

            finally
            {
                conn.Close();
            }
            return Ok(students);
        }

        [HttpGet("SearchStudentSecure")]
        public async Task<IActionResult> SearchStudentSecure(string name)
        {
            var conn = _context.Database.GetDbConnection();
            var query = "SELECT FirstName, LastName FROM Student WHERE FirstName Like @name";
            IEnumerable<Student> students;

            try
            {
                await conn.OpenAsync();
                students = await conn.QueryAsync<Student>(query, new { name });
            }

            finally
            {
                conn.Close();
            }
            return Ok(students);
        }


        public async Task<IActionResult> Index()
        {
            try
            {
                var students = from s in _context.Students
                               select s;
                return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), 1, 10));
            }
            catch
            {
                return View(new PaginatedList<Student>(new List<Student>(), 1, 1, 10));
            }

        }

        [HttpGet("RecreateDatabase")]
        public async Task<ActionResult> RecreateDatabase()
        {
            await _context.Database.EnsureDeletedAsync();

            await _context.Database.MigrateAsync();
            await _context.Database.EnsureCreatedAsync();
            await DbInitializer.Initialize(_context);

            return RedirectToAction("Index");
        }
    }
}