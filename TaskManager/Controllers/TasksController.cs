using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TM_Task = TaskManager.Models.Task;
namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagerContext _context;
        private User _currentUserData = null;
        public TasksController(Models.ITaskManagerContext context)
        {
            _context = (TaskManagerContext)context;

            

        }
        

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TM_Task>>> GetTask()
        {
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.UserId == UserData.Id);
            return await System.Threading.Tasks.Task.Run(() => { return Ok(_knownTask?.ToList() ?? new List<TM_Task>()); });
        }

        private User GetCurrentUserData()
        {
           
            try
            {
                _currentUserData = _currentUserData ?? _context.User.Find(long.Parse(User.Identity.Name));
            }
            catch
            {
                //Do nothing here yet
            }

            return _currentUserData;
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TM_Task>> GetTask(long id)
        {
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.Id == id && x.UserId == UserData.Id).FirstOrDefault();

            if (_knownTask == null)
            {
                return NotFound();
            }
            return await System.Threading.Tasks.Task.Run(() => { return Ok(_knownTask); });
         
        }

        // PUT: api/Tasks/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(long id, TM_Task task)
        {
            if (id != task.Id && task.Id>0)
            {
                return BadRequest();
            }
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.Id == id && x.UserId ==  UserData.Id).FirstOrDefault();
            if (_knownTask == null)
            {
                return NotFound();
            }
            _knownTask.Description = task.Description;
            _knownTask.Concluded = task.Concluded;


            _context.Entry(_knownTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tasks
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<TM_Task>> PostTask(TM_Task task)
        {
            User UserData = GetCurrentUserData();
            UserData.Task.Add(task);
            _context.Entry(UserData).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }




        // POST: api/Tasks/SetConcluded/5
        [HttpPost("~/api/[controller]/{id}/SetConcluded")]
        public async Task<ActionResult<TM_Task>> SetConcluded(long id)
        {
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.Id == id && x.UserId == UserData.Id).FirstOrDefault();
            if (_knownTask == null)
            {
                return NotFound();
            }

            _knownTask.Concluded = true;

            _context.Entry(_knownTask).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return _knownTask;
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TM_Task>> DeleteTask(long id)
        {
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.Id == id && x.UserId == UserData.Id).FirstOrDefault();
            if (_knownTask == null)
            {
                return NotFound();
            }

            _context.Task.Remove(_knownTask);
            await _context.SaveChangesAsync();

            return _knownTask;
        }

        private bool TaskExists(long id)
        {
            return _context.Task.Any(e => e.Id == id);
        }
    }
}
