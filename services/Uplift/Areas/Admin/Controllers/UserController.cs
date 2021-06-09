using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uplift.DataAccess.Data.Repository.IRepository;
using Uplift.Utility;

namespace Uplift.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Admin)]    // only want admin accessing these pages
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // claims.Value - is the id of the logged in user and we dont want it bc
            // we dont want to accidently lock them out since they're the admin
            return View(_unitOfWork.User.GetAll(u=>u.Id!=claims.Value));
        }

        public IActionResult Lock(string id)
        {
            if(id==null)
            {
                return NotFound();
            }
            _unitOfWork.User.LockUser(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UnLock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _unitOfWork.User.UnLockUser(id);
            return RedirectToAction(nameof(Index));
        }
    }
}