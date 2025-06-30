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
        public Task<IActionResult> Add(PatientViewModel model)
        {
            throw new ArgumentException();
        }

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
        public Task<IActionResult> Book(AppointmentViewModel model)
        {
            throw new ArgumentException();
        }
    }
}
