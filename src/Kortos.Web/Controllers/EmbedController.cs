using Kortos.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kortos.Web.Controllers;

public class EmbedController : Controller
{
    private readonly NethysService _nethysService;

    public EmbedController(NethysService nethysService)
    {
        _nethysService = nethysService;
    }

    // GET /Equipment/<id>?style=Dark
    public async Task<IActionResult> Equipment(int id, CancellationToken cancel, Style style = Style.None)
    {
        var html = await _nethysService.GetEquipmentHtml(id, cancel);
        if (style != Style.None)
        {
            ViewData[nameof(Style)] = $"{style.ToString().ToLowerInvariant()}.css";
        }
        return View(new EmbedModel(id, "", html, style));
    }
}