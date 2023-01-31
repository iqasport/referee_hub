using System.Collections.Generic;

namespace ManagementHub.Models.Domain.Ngb;

/// <summary>
/// An NGB constraint that represents one, multiple, or any NGBs.
/// </summary>
public class NgbConstraint
{
    private bool anyNgb;
    private List<NgbIdentifier>? nationalGoverningBodies;
    
    private NgbConstraint() => anyNgb = true;

    public NgbConstraint(NgbIdentifier nationalGoverningBody)
        => this.nationalGoverningBodies = new List<NgbIdentifier>{nationalGoverningBody};
    public NgbConstraint(IEnumerable<NgbIdentifier> nationalGoverningBodies)
        => this.nationalGoverningBodies = new List<NgbIdentifier>(nationalGoverningBodies);

    public static NgbConstraint Any = new NgbConstraint();

    public bool AppliesTo(NgbIdentifier ngbId)
    {
        if (this.anyNgb)
        {
            return true;
        }

        return this.nationalGoverningBodies!.Contains(ngbId);
    }
}
