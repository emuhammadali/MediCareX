using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class Disease : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiseaseSeverity Severity { get; set; }
    public string? Treatment { get; set; }
    public string? SpecialistType { get; set; }
    public ICollection<DiseaseSymptom> DiseaseSymptoms { get; set; } = new List<DiseaseSymptom>();
}

/// <summary>Explicit join entity for the Disease ⇄ Symptom many-to-many (carries a weight).</summary>
public class DiseaseSymptom
{
    public Guid DiseaseId { get; set; }
    public Disease Disease { get; set; } = null!;
    public Guid SymptomId { get; set; }
    public Symptom Symptom { get; set; } = null!;
    public double Weight { get; set; } = 1.0;
}
