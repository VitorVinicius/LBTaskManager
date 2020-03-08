using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TM_Task = TaskManager.Models.Task;


namespace TaskManager.Controllers.API
{

    ///<summary>Task Management Routes</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class TasksController : ControllerBase
    {
        private readonly TaskManagerContext _context;
        private User _currentUserData = null;
        /// <summary>Initializes the controller</summary>
        public TasksController(ITaskManagerContext context)
        {
            _context = (TaskManagerContext)context;



        }


        // GET: api/Tasks
        ///<summary>Get user task list</summary>
        ///<returns>Task List</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TM_Task>>> GetTask()
        {
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.UserId == UserData.Id);
            return await System.Threading.Tasks.Task.Run(() => { return Ok(_knownTask?.ToList() ?? new List<TM_Task>()); });
        }

        /// <summary>
        /// Get Current Logged User Data
        /// </summary>
        /// <returns></returns>
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
        ///<summary>Get user task by id</summary>
        ///<returns>Task Data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(TM_Task))]
        [ProducesResponseType(404, Type = typeof(void))]
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
        ///<summary>Update user task by id and Task Data</summary>
        ///<returns>New Task Data Stored Changes</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(204, Type = typeof(void))]
        [ProducesResponseType(404, Type = typeof(void))]
        public async Task<IActionResult> PutTask(long id, TM_Task task)
        {
            if (id != task.Id && task.Id > 0)
            {
                return BadRequest();
            }
            User UserData = GetCurrentUserData();
            var _knownTask = _context.Task.Where(x => x.Id == id && x.UserId == UserData.Id).FirstOrDefault();
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
        ///<summary>Add user task</summary>
        ///<returns>New Task Stored</returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(void))]
        public async Task<ActionResult<TM_Task>> PostTask(TM_Task task)
        {
            User UserData = GetCurrentUserData();
            UserData.Task.Add(task);
            _context.Entry(UserData).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTask", new { id = task.Id }, task);
        }




        // POST: api/Tasks/SetConcluded/5
        ///<summary>Set user task as concluded</summary>
        ///<returns>Task Changes</returns>
        [HttpPost("~/api/[controller]/{id}/SetConcluded")]
        [ProducesResponseType(200, Type = typeof(TM_Task))]
        [ProducesResponseType(404, Type = typeof(void))]
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
        ///<summary>Delete user task</summary>
        ///<returns>Task Data before deletion</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(TM_Task))]
        [ProducesResponseType(404, Type = typeof(void))]
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
        /// <summary>
        /// Check Task exists
        /// </summary>
        /// <param name="id">Task Id</param>
        /// <returns>true if exists, false if not</returns>
        private bool TaskExists(long id)
        {
            return _context.Task.Any(e => e.Id == id);
        }
    }
}
