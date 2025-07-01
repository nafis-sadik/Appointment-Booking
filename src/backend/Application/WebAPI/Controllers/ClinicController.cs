using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Abstraction;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicController(IClinicService clinicService) : ControllerBase
    {
        private readonly IClinicService _clinicService = clinicService;

        [HttpGet]
        public async Task<IActionResult> GetClinics()
            => Ok(await _clinicService.GetClinics());

        [HttpGet]
        [Route("Doctors/{ClinicId}")]
        public async Task<IActionResult> GetClinicDoctors(int ClinicId)
            => Ok(await _clinicService.GetClinicDoctors(ClinicId));

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Update(ClinicViewModel model)
            => Ok(await _clinicService.Update(model));

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(ClinicViewModel model)
            => Ok(await _clinicService.Add(model));
    }
}
