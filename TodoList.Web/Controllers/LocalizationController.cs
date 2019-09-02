using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace TodoList.Web.Controllers
{
    public interface ILocalizationController
    {

    }

    public class LocalizationController : Controller
    {

        private readonly IStringLocalizer<LocalizationController> _localizer;
        public LocalizationController(IStringLocalizer<LocalizationController> localizer)
        {
            _localizer = localizer;
        }

    }
}