using System.Text.Encodings.Web;
using ManagementHub.Models.Abstraction.Contexts.Providers;
using ManagementHub.Models.Domain.Tournament;
using ManagementHub.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManagementHub.Service.Areas.Tournaments;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("tournaments/{tournamentId}")]
public class TournamentShareController(
	IWebHostEnvironment environment,
	ManagementHubDbContext dbContext,
	ITournamentContextProvider tournamentContextProvider) : ControllerBase
{
	[HttpGet]
	[HttpHead]
	public async Task<IActionResult> Page(string tournamentId, CancellationToken cancellationToken)
	{
		var file = environment.WebRootFileProvider.GetFileInfo("index.html");
		if (!file.Exists)
		{
			return this.NotFound();
		}

		if (!TournamentIdentifier.TryParse(tournamentId, out var id))
		{
			return this.File(file.CreateReadStream(), "text/html; charset=utf-8");
		}

		var tournament = await dbContext.Tournaments.AsNoTracking()
			.Where(t => t.UniqueId == id.ToString() && t.DeletedAt == null && !t.IsPrivate)
			.Select(t => new { t.Name, t.Description })
			.SingleOrDefaultAsync(cancellationToken);

		if (tournament == null)
		{
			return this.File(file.CreateReadStream(), "text/html; charset=utf-8");
		}

		using var reader = new StreamReader(file.CreateReadStream());
		var html = await reader.ReadToEndAsync(cancellationToken);
		var headEnd = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
		if (headEnd < 0)
		{
			return this.Content(html, "text/html; charset=utf-8");
		}

		var path = this.Request.PathBase.Add(new PathString($"/tournaments/{id}"));
		var url = UriHelper.BuildAbsolute(this.Request.Scheme, this.Request.Host, path);
		var tags = Meta("og:type", "website") + Meta("og:title", tournament.Name) + Meta("og:url", url);
		if (!string.IsNullOrWhiteSpace(tournament.Description))
		{
			tags += Meta("og:description", tournament.Description);
		}

		var banner = await tournamentContextProvider.GetTournamentBannerUriAsync(id, cancellationToken);
		if (banner != null)
		{
			tags += Meta("og:image", url + "/share-image");
		}

		return this.Content(html.Insert(headEnd, tags), "text/html; charset=utf-8");
	}

	[HttpGet("share-image")]
	[HttpHead("share-image")]
	[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
	public async Task<IActionResult> Image(string tournamentId, CancellationToken cancellationToken)
	{
		if (!TournamentIdentifier.TryParse(tournamentId, out var id) ||
			!await dbContext.Tournaments.AnyAsync(t => t.UniqueId == id.ToString() &&
				t.DeletedAt == null && !t.IsPrivate, cancellationToken))
		{
			return this.NotFound();
		}

		var banner = await tournamentContextProvider.GetTournamentBannerUriAsync(id, cancellationToken);
		return banner == null ? this.NotFound() : this.Redirect(banner.ToString());
	}

	private static string Meta(string property, string value) =>
		$"<meta property=\"{property}\" content=\"{HtmlEncoder.Default.Encode(value)}\">\n";
}
