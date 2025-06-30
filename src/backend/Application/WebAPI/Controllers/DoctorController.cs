using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services.Abstraction;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController (IDoctorService doctorService) : ControllerBase
    {
        private readonly IDoctorService _doctorService = doctorService;
        
        [HttpGet]
        public async Task<IActionResult> GetDoctorList()
            => Ok(await _doctorService.GetDoctorList());

        [HttpPost]
        public async Task<IActionResult> Add(DoctorViewModel model)
            => Ok(await _doctorService.AddDoctor(model));

        [HttpGet]
        [Route("Appointment/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointments(int DoctorId)
            => Ok(await _doctorService.GetDoctorAppointments(DoctorId));

        [HttpPost]
        [Route("Schedule")]
        public async Task<IActionResult> AddSchedule(ScheduleViewModel model)
            => Ok(await _doctorService.AddSchedule(model));
    }
}
