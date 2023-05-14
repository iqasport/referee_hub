// COPIED FROM https://github.com/lukencode/FluentEmail/issues/241#issuecomment-763242538
// with small adjustments

namespace ManagementHub.Mailers.Utils;

using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentEmail.Core.Interfaces;
using RazorLight;

/// <summary>
/// Very similar to <see cref="FluentEmail.Razor.RazorRenderer"/> but for RazorLight configured to use embedded resources.
/// </summary>
public class EmbeddedRazorRenderer : ITemplateRenderer
{
	private readonly RazorLightEngine engine;

	/// <summary>
	/// Initializes a new instance of the <see cref="EmbeddedRazorRenderer"/> class.
	/// Configures RazorLight to use a project whose persistent store an assembly manifest resource stream.
	/// </summary>
	/// <param name="embeddedResRootType">Any type in the root namespace (prefix) for your assembly manifest resource stream.</param>
	/// <remarks>Docs borrowed from RazorLight UseEmbeddedResourcesProject.</remarks>
	public EmbeddedRazorRenderer(Assembly assembly, string rootNamespace)
	{
		this.engine = new RazorLightEngineBuilder()
			.UseEmbeddedResourcesProject(assembly, rootNamespace)
			.SetOperatingAssembly(assembly)
			.UseMemoryCachingProvider()
			.Build();
	}

	/// <inheritdoc/>
	public async Task<string> ParseAsync<T>(string template, T model, bool isHtml = true) => await this.engine
		.CompileRenderAsync(template, model)
		.ConfigureAwait(false);

	/// <inheritdoc/>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "This is required to satisfy ITemplateRenderer implemenation")]
	public string Parse<T>(string template, T model, bool isHtml) => this.ParseAsync(template, model, isHtml)
		.GetAwaiter()
		.GetResult();
}
