using System.Text.Json;
using Excos.Options;
using ManagementHub.Models.Abstraction.Commands;
using ManagementHub.Storage.Database.Transactions;
using Microsoft.Extensions.Options.Contextual;

namespace ManagementHub.Service.Experimentation;

public class UxExperimentVariantAssigner
{
	private readonly IContextualOptions<UxExperimentAssignmentOptions> contextualOptions;
	private readonly ISetUserAttributeCommand setUserAttributeCommand;
	private readonly IDatabaseTransactionProvider transactionProvider;

	public UxExperimentVariantAssigner(IContextualOptions<UxExperimentAssignmentOptions> contextualOptions, ISetUserAttributeCommand setUserAttributeCommand, IDatabaseTransactionProvider transactionProvider)
	{
		this.contextualOptions = contextualOptions;
		this.setUserAttributeCommand = setUserAttributeCommand;
		this.transactionProvider = transactionProvider;
	}

	public async Task SaveUxExperimentAssignmentsAsync(ExperimentContext context, CancellationToken cancellationToken)
	{
		var options = await this.contextualOptions.GetAsync(context, cancellationToken);

		if (options.ExperimentVariants.Count == 0)
		{
			return;
		}

		await using var transaction = await this.transactionProvider.BeginAsync();

		foreach (var (experiment, variant) in options.ExperimentVariants)
		{
			await this.setUserAttributeCommand.SetRootUserAttributeAsync(
				context.UserId, $"EXP.{experiment}", JsonDocument.Parse(JsonSerializer.Serialize(variant)), cancellationToken);
		}

		// TODO: persist metadata to a new table via a new interface and db based impl

		await transaction.CommitAsync(cancellationToken);
	}
}

public class UxExperimentAssignmentOptions
{
	public Dictionary<string, string> ExperimentVariants { get; set; }= new();

	public FeatureMetadata? Metadata { get; set; }
}
