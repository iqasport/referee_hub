using ManagementHub.Models.Domain.Language;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Languages;

[Route("api/[controller]")]
[ApiController]
public class LanguagesController : ControllerBase
{
	private readonly ManagementHubDbContext dbContext; // TODO: should I move this to the Storage project?

	public LanguagesController(ManagementHubDbContext dbContext)
	{
		this.dbContext = dbContext;
	}

	[HttpGet]
	[Authorize]
	public async Task<IEnumerable<LanguageIdentifier>> GetLanguages()
	{
		return await this.dbContext.Languages.Select(l => new LanguageIdentifier(l.ShortName, l.ShortRegion)).ToListAsync();
	}
}
