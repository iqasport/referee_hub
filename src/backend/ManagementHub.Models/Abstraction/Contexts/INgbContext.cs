using ManagementHub.Models.Domain.Ngb;

namespace ManagementHub.Models.Abstraction.Contexts;

public interface INgbContext
{
	NgbIdentifier NgbId { get; }

	NgbData NgbData { get; }
}
