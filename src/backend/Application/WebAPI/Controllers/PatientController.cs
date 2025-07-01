using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services.Abstraction;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController(IPatientService patientService) : ControllerBase
    {
        private readonly IPatientService _patientService = patientService;

        [HttpPost]
        public async Task<IActionResult> Add(PatientViewModel model)
            => Ok(await _patientService.Add(model));

        [HttpPut]
        public Task<IActionResult> Update(PatientViewModel model)
        {
            throw new ArgumentException();
        }

        [HttpGet]
        public Task<IActionResult> Search(string SearchString)
        {
            throw new ArgumentException();
        }

        [HttpPost]
        [Route("Appointment")]
        public async Task<IActionResult> Book(AppointmentViewModel model)
        {
            await _patientService.Book(model);
            return Ok();
        }
    }
}
