namespace Blanquita.Domain.ValueObjects;

public record BranchId
{
    public int Value { get; }

    private BranchId(int value)
    {
        Value = value;
    }

    public static BranchId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Branch ID must be greater than zero", nameof(value));

        return new BranchId(value);
    }

    public static implicit operator int(BranchId branchId) => branchId.Value;
}
